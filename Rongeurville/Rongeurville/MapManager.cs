using MPI;
using Rongeurville.Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Rongeurville.Map;

namespace Rongeurville
{
    class MapManager
    {
        private Intracommunicator comm;
        private Logger logger;
        
        private ActorProcess[] actors;
        private Map map;
        
        private volatile bool ContinueExecution = true;
        private Task messageListenerTask;
        private BlockingCollection<Message> messageQueue = new BlockingCollection<Message>();

        class ActorProcess
        {
            public int Rank;
            public ProcessType Type;
            public bool IsDeadProcess = false;
            public bool Playing = true;
            public Coordinates Position;
        }

        public MapManager(Intracommunicator comm, string mapFilePath, ActorsDivider divider)
        {
            this.comm = comm;

            if (!File.Exists(mapFilePath))
            {
                throw new FileNotFoundException("The map file was not found.");
            }

            map = LoadMapFromFile(mapFilePath);

            if (map.Rats.Count != divider.NumberOfRats || map.Cats.Count != divider.NumberOfCats)
            {
                throw new Exception("Rat or cat counts does not match with map loaded.");
            }

            InitActorProcesses();

            // Logger
            logger = new Logger(divider.GetProcesTypeByRank);

            // Initialize the message listener task
            messageListenerTask = new Task(ReceiveMessages);
        }

        /// <summary>
        /// Initialize ActorProcess lists for rats and cats
        /// </summary>
        private void InitActorProcesses()
        {
            List<ActorProcess> actorsList = new List<ActorProcess>();
            int count = 0;
            actorsList.AddRange(map.Rats.Select(t => new ActorProcess
            {
                Rank = ++count,
                Type = ProcessType.Rat,
                IsDeadProcess = false,
                Playing = true,
                Position = t.Position
            }).ToArray());

            actorsList.AddRange(map.Cats.Select(t => new ActorProcess
            {
                Rank = ++count,
                Type = ProcessType.Cat,
                IsDeadProcess = false,
                Playing = true,
                Position = t.Position
            }).ToArray());

            // We need to access elements in this collection by index (rank - 1), so an array is more efficient than a list even though initialization is more costful.
            actors = actorsList.ToArray();
        }

        /// <summary>
        /// Method that receives messages 
        /// </summary>
        private void ReceiveMessages()
        {
            while (ContinueExecution)
            {
                // Receive next message and handle it
                ReceiveRequest asyncReceive = null;
                lock (comm)
                {
                    // starts an asynchronous receive of the next message
                    // Note: Must be asynchronous otherwise the lock of the comm object may cause a dead lock (unable to send message because we are waiting for one)
                    asyncReceive = comm.ImmediateReceive<Message>(Communicator.anySource, 0); 
                }

                // Creates a task that waits for the result to arrive.
                // Note : We must use a task because the Wait function of a task allows for a timeout parameter which we need to validate if the execution is still ongoing
                Task waitForMessage = new Task(() =>
                {
                    asyncReceive.Wait();
                });
                waitForMessage.Start();

                // waits for a message to be received, stops (within 200ms) if the execution stops
                while (ContinueExecution && !waitForMessage.Wait(200))
                    ;

                if (!ContinueExecution)
                {
                    asyncReceive.Cancel();
                    // We need to wait for the asyncReceive cancellation because the completion of this thread will close MPI but we need to make sure the cancel is over by then.
                    asyncReceive.Wait();

                    break; // if the execution is over, the message may not be valid and should be ignored.
                }

                messageQueue.Add((Message)asyncReceive.GetValue());
            }

            // Signal the main MapManager thread that this thread is correctly closed and we won't receive any other messages
            messageQueue.CompleteAdding();
        }

        public void Start()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Start the task that listens to incomming messages and queue them
            messageListenerTask.Start();

            // Send map and their respective position to everyone and start the game
            SignalGameStartToAllActors();

            while (true)
            {
                try
                {
                    // Waits for the next message to arrive
                    Communication.Request request = messageQueue.Take() as Communication.Request;
                    if (request != null)
                    {
                        HandleRequest(request);
                    }
                }
                catch (InvalidOperationException e)
                {
                    break; // No more message to receive and the task receiving messages is finished, get out of the infinite while loop
                }
            }

            stopwatch.Stop();
            logger.LogExecutionTime((int)stopwatch.ElapsedMilliseconds, map.Rats.Count == 0 ? ProcessType.Cat : ProcessType.Rat);
        }

        /// <summary>
        /// Informs every other processes that the game started and provides the map and position informations
        /// </summary>
        private void SignalGameStartToAllActors()
        {
            StartSignal startSignal = new StartSignal { Map = map };
            lock (comm)
            {
                foreach (ActorProcess actor in actors)
                {
                    startSignal.Position = actor.Position;
                    comm.Send(startSignal, actor.Rank, 0);
                }
            }
        }

        /// <summary>
        /// Ends the current game by blocking every processes from playing and signaling them to stop
        /// </summary>
        private void EndGame()
        {
            // Stops all actors from playing
            foreach (ActorProcess actor in actors)
            {
                actor.Playing = false;
            }

            // Signal to everyone that the game is over and they should stop.
            Broadcast(new KillSignal());
        }

        /// <summary>
        /// Handle a request message 
        /// </summary>
        /// <param name="request"></param>
        private void HandleRequest(Communication.Request request)
        {
            ActorProcess sender = GetActorProcessByRank(request.Rank);

            // Death confirmation
            DeathConfirmation deathConfirmation = request as DeathConfirmation;
            if (deathConfirmation != null)
            {
                HandleDeath(deathConfirmation, sender);

                return;
            }

            // Validate that the process is still in the game (playing).
            // This is meant to deny requests that were sent in between the moment the actor was removed from the game and the moment the actor learned about it.
            if (!sender.Playing)
            {
                return; // Ignore the request, the process is no longer playing
            }

            // Move request
            MoveRequest moveRequest = request as MoveRequest;
            if (moveRequest != null)
            {
                HandleMovePlayer(moveRequest, sender);

                return;
            }

            // Meow request
            MeowRequest meowRequest = request as MeowRequest;
            if (meowRequest != null)
            {
                HandleMeow(meowRequest, sender);

                return;
            }
        }

        /// <summary>
        /// Handles a move request by a player and signal it to the other processes
        /// </summary>
        /// <param name="moveRequest"></param>
        /// <param name="sender"></param>
        private void HandleMovePlayer(MoveRequest moveRequest, ActorProcess sender)
        {
            // Pessimistically assume the move is going to be denied
            MoveSignal moveSignal = new MoveSignal { InitialTile = sender.Position, FinalTile = sender.Position };

            // Try to move and update the final position
            MoveEffect effect = map.CheckForMoveEffects(sender.Position, moveRequest.DesiredTile);
            if (effect != MoveEffect.InvalidMove)
            {
                logger.LogMove(sender.Rank, true, sender.Position, moveRequest.DesiredTile);
                switch (effect)
                {
                    case MoveEffect.RatCaptured:
                        ActorProcess rat = GetActorProcessByCoordinates(moveRequest.DesiredTile);
                        logger.LogCaptureRat(rat.Rank, sender.Rank);
                        HandleRatRemoval(rat);
                        break;
                    case MoveEffect.RatEscaped:
                        logger.LogExitRat(sender.Rank, moveRequest.DesiredTile);
                        HandleRatRemoval(GetActorProcessByCoordinates(sender.Position));
                        break;
                    case MoveEffect.CheeseEaten:
                        logger.LogCheeseConsumption(sender.Rank, moveRequest.DesiredTile);
                        break;
                }

                // Move is valid and the destination tile no longer has important information
                map.ApplyMove(sender.Position, moveRequest.DesiredTile);
                sender.Position = moveRequest.DesiredTile;
                moveSignal.FinalTile = moveRequest.DesiredTile;
            }
            else
            {
                logger.LogMove(sender.Rank, false, sender.Position, moveRequest.DesiredTile);
            }

            // TODO Delete this and replace it by a timer or something like this
            logger.LogMap(map.ToString());

            if (IsGameOver())
            {
                EndGame();
            }
            else
            {
                // Broadcast the result of the move
                Broadcast(moveSignal);
            }
        }

        /// <summary>
        /// Remove rat from playing actors and signal him that he died.
        /// </summary>
        /// <param name="rat"></param>
        private void HandleRatRemoval(ActorProcess rat)
        {
            // The cat cannot play anymore
            rat.Playing = false;

            lock (comm)
            {
                // Notify the rat that he should stop
                comm.Send(new KillSignal(), rat.Rank, 0);
            }
        }

        /// <summary>
        /// Handles a meow and signal it to other processes
        /// </summary>
        /// <param name="meowRequest"></param>
        /// <param name="sender"></param>
        private void HandleMeow(MeowRequest meowRequest, ActorProcess sender)
        {
            logger.LogMeow(sender.Rank, sender.Position);
            Broadcast(new MeowSignal { MeowLocation = sender.Position });
        }

        /// <summary>
        /// Handles a death
        /// </summary>
        /// <param name="deathConfirmation"></param>
        /// <param name="sender"></param>
        private void HandleDeath(DeathConfirmation deathConfirmation, ActorProcess sender)
        {
            GetActorProcessByRank(deathConfirmation.Rank).IsDeadProcess = true;
            
            if (AreAllActorsFinished())
            {
                // Stop the MapManager reception of messages if all actor's processes are dead
                ContinueExecution = false;
            }
        }

        /// <summary>
        /// Check if the game is over (cat or rat win)
        /// </summary>
        /// <returns></returns>
        private bool IsGameOver()
        {
            return map.Cheese.Count == 0 || map.Rats.Count == 0;
        }

        /// <summary>
        /// Check if all actors are finished
        /// </summary>
        /// <returns></returns>
        private bool AreAllActorsFinished()
        {
            return actors.All(a => a.IsDeadProcess);
        }

        /// <summary>
        /// Get the ActorProcess linked to the rank
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        private ActorProcess GetActorProcessByRank(int rank)
        {
            return actors[rank - 1];
        }

        /// <summary>
        /// Get the ActorProcess linked to the coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private ActorProcess GetActorProcessByCoordinates(Coordinates position)
        {
            return actors.FirstOrDefault(a => a.Position.Equals(position));
        }

        /// <summary>
        /// Broadcast a message to all actors as if the message was directly sent at them
        /// </summary>
        /// <param name="message"></param>
        private void Broadcast(Message message)
        {
            lock (comm)
            {
                foreach (ActorProcess actor in actors)
                {
                    comm.Send(message, actor.Rank, 0);
                }
            }
        }
    }
}

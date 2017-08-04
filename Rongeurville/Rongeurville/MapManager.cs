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
        private Map map;
        private Intracommunicator comm;
        private Logger logger;

        private Task messageListenerTask;
        private BlockingCollection<Message> messageQueue = new BlockingCollection<Message>();

        private ActorProcess[] rats;
        private ActorProcess[] cats;

        private bool ContinueExecution = true;

        class ActorProcess
        {
            public int Rank;
            public bool IsDeadProcess = false;
            public bool Playing = true;
            public Coordinates Position;
        }

        public MapManager(Intracommunicator comm, string mapFilePath, int numberOfRats, int numberOfCats)
        {
            this.comm = comm;

            if (!File.Exists(mapFilePath))
            {
                throw new FileNotFoundException("The map file was not found.");
            }

            map = Map.LoadMapFromFile(mapFilePath);

            if (map.Rats.Count != numberOfRats || map.Cats.Count != numberOfCats)
            {
                throw new Exception("Rat or cat counts does not match with map loaded.");
            }

            InitActorProcesses();

            // Logger
            logger = new Logger(rank =>
            {
                if (rank == 0)
                    return ProcessType.Map;
                if (rank <= numberOfRats)
                    return ProcessType.Rat;
                return ProcessType.Cat;
            });
        }

        /// <summary>
        /// Initialize ActorProcess lists for rats and cats
        /// </summary>
        private void InitActorProcesses()
        {
            int count = 0;
            rats = map.Rats.Select(t => new ActorProcess
            {
                Rank = ++count,
                IsDeadProcess = false,
                Position = t.Position
            }).ToArray();

            cats = map.Cats.Select(t => new ActorProcess
            {
                Rank = ++count,
                IsDeadProcess = false,
                Position = t.Position
            }).ToArray();
        }

        /// <summary>
        /// Broadcast a message to all actors as if the message was directly aimed at them
        /// </summary>
        /// <param name="message"></param>
        private void Broadcast(Message message)
        {
            //comm.Broadcast(ref message, 0);
            lock (comm)
            {
                foreach (ActorProcess actor in cats)
                {
                    comm.Send(message, actor.Rank, 0);
                }
                foreach (ActorProcess actor in rats)
                {
                    comm.Send(message, actor.Rank, 0);
                }
            }
        }

        public void Start()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            messageListenerTask = new Task(() =>
            {
                while (ContinueExecution)
                {
                    // Receive next message and handle it
                    Console.WriteLine("pre receive");
                    ReceiveRequest asyncReceive = null;
                    lock (comm)
                    {
                        asyncReceive = comm.ImmediateReceive<Message>(Communicator.anySource, 0);
                    }
                    Console.WriteLine("post receive");

                    while (ContinueExecution && asyncReceive.Test() == null)
                        ;
                    if (ContinueExecution)
                    {
                        Message message = (Message)asyncReceive.GetValue();

                        messageQueue.Add(message);
                        Console.WriteLine("++++ Message queued");

                    }
                    else
                    {
                        asyncReceive.Cancel();
                    }
                }
                Console.WriteLine("********Completed adding");
                messageQueue.CompleteAdding();
            });

            messageListenerTask.Start();

            // Send map to everyone and start the game
            StartSignal startSignal = new StartSignal { Map = map };
            lock (comm)
            {
                comm.Broadcast(ref startSignal, 0);
            }

            while (ContinueExecution)
            {
                // Treat all message for as long as the execution is supposed to continue
                HandleMessageReceive();
            }

            stopwatch.Stop();
            logger.LogExecutionTime((int)stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Handles the reception of a message by interpreting it and taking the necessary actions.
        /// </summary>
        private void HandleMessageReceive()
        {
            // Receive next message and handle it
            //Message message = comm.Receive<Message>(Communicator.anySource, 0);
            Message message = null;
            try
            {
                message = messageQueue.Take();
                Console.WriteLine("---- Message dequeued");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("**** No more messages");
                return;
            }

            Communication.Request request = message as Communication.Request;
            if (request != null)
            {
                HandleRequest(request);
            }
            else
            {
                // TODO This is an invalid message, only request are accepted, log it
                Console.WriteLine("*************** Not supposed to happen!!! invalid message");
            }
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
                return; // Ignore the message, the process is no longer playing
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

            // TODO At this point, this is an unsupported request, log it

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
                Console.WriteLine("****** MapManager called ApplyMove");
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
                // Signal to everyone that the game is over and they should stop.
                KillSignal killSignal = new KillSignal { KillAll = true };
                Broadcast(killSignal);
            }
            else
            {
                Console.WriteLine($"Map accepted move of actor {sender.Rank}");

                // Broadcast the result of the move
                Broadcast(moveSignal);
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
        /// Remove rat from playing actors and signal him that he died.
        /// </summary>
        /// <param name="rat"></param>
        private void HandleRatRemoval(ActorProcess rat)
        {
            // The cat cannot play anymore
            rat.Playing = false;

            // Notify the rat that he should stop
            //comm.Send(new KillSignal(), rat.Rank, 0);

            lock (comm)
            {
                comm.Send(new KillSignal() { RanksTargeted = new List<int> { rat.Rank } }, rat.Rank, 0);
            }

            //KillSignal killSignal = new KillSignal { RanksTargeted = new List<int> { rat.Rank } };
            //comm.Broadcast(ref killSignal, 0);
        }

        /// <summary>
        /// Handles a meow and signal it to other processes
        /// </summary>
        /// <param name="meowRequest"></param>
        /// <param name="sender"></param>
        private void HandleMeow(MeowRequest meowRequest, ActorProcess sender)
        {
            logger.LogMeow(sender.Rank, sender.Position);
            MeowSignal meowSignal = new MeowSignal { MeowLocation = sender.Position };
            Broadcast(meowSignal);
        }

        /// <summary>
        /// Handles a death
        /// </summary>
        /// <param name="deathConfirmation"></param>
        /// <param name="sender"></param>
        private void HandleDeath(DeathConfirmation deathConfirmation, ActorProcess sender)
        {
            ActorProcess dyingActorProcess = GetActorProcessByRank(deathConfirmation.Rank);
            dyingActorProcess.IsDeadProcess = true;

            // TODO Change this for a count of the number of death processes 
            if (AreAllActorsFinished())
            {
                // Stop the MapManager
                ContinueExecution = false;
            }
        }

        /// <summary>
        /// Check if all actors are finished
        /// </summary>
        /// <returns></returns>
        private bool AreAllActorsFinished()
        {
            return !(rats.Any(rat => !rat.IsDeadProcess) || cats.Any(cat => !cat.IsDeadProcess));
        }

        /// <summary>
        /// Get the ActorProcess linked to the rank
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        private ActorProcess GetActorProcessByRank(int rank)
        {
            return cats.FirstOrDefault(c => c.Rank == rank) ?? rats.FirstOrDefault(r => r.Rank == rank);
        }

        /// <summary>
        /// Get the ActorProcess linked to the coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private ActorProcess GetActorProcessByCoordinates(Coordinates position)
        {
            return cats.FirstOrDefault(c => c.Position.Equals(position)) ?? rats.FirstOrDefault(r => r.Position.Equals(position));
        }
    }
}

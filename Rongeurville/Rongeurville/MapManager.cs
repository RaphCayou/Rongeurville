using MPI;
using Rongeurville.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rongeurville
{
    class MapManager
    {
        private Map map;
        private Intracommunicator comm;

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

        public void Start()
        {
            // Send map to everyone and start the game
            StartSignal startSignal = new StartSignal { Map = map };
            comm.Broadcast(ref startSignal, 0);

            while (ContinueExecution)
            {
                // Treat all message for as long as the execution is supposed to continue
                HandleMessageReceive();
            }
        }

        /// <summary>
        /// Handles the reception of a message by interpreting it and taking the necessary actions.
        /// </summary>
        private void HandleMessageReceive()
        {
            // Receive next message and handle it
            Message message = comm.Receive<Message>(Communicator.anySource, 0);

            Communication.Request request = message as Communication.Request;
            if (request != null)
            {
                ActorProcess sender = GetActorProcessByRank(request.Rank);

                // Death confirmation
                DeathConfirmation deathConfirmation = message as DeathConfirmation;
                if (deathConfirmation != null)
                {
                    HandleDeath(deathConfirmation, sender);

                    // TODO Change this for a count of the number of death processes 
                    if (AreAllActorsFinished())
                    {
                        // Stop the MapManager
                        ContinueExecution = false;
                    }

                    return;
                }

                // Validate that the process is still in the game (playing).
                // This is meant to deny requests that were sent in between the moment the actor was removed from the game and the moment the actor learned about it.
                if (!sender.Playing)
                {
                    return; // Ignore the message, the process is no longer playing
                }

                // Move request
                MoveRequest moveRequest = message as MoveRequest;
                if (moveRequest != null)
                {
                    HandleMovePlayer(moveRequest, sender);

                    return;
                }

                // Meow request
                MeowRequest meowRequest = message as MeowRequest;
                if (meowRequest != null)
                {
                    HandleMeow(meowRequest, sender);

                    return;
                }
                
                // TODO At this point, this is an unsupported request, log it

            }
            else
            {
                // TODO This is an invalid message, only request are accepted, log it

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
            bool moveAccepted = map.ApplyMove(sender.Position, moveRequest.DesiredTile);
            if (moveAccepted)
            {
                moveSignal.FinalTile = moveRequest.DesiredTile;
            }

            // TODO Kill rats or cats

            // Broadcast the result
            comm.Broadcast(ref moveSignal, 0);
        }

        /// <summary>
        /// Handles a meow and signal it to other processes
        /// </summary>
        /// <param name="meowRequest"></param>
        /// <param name="sender"></param>
        private void HandleMeow(MeowRequest meowRequest, ActorProcess sender)
        {
            MeowSignal meowSignal = new MeowSignal { MeowLocation = sender.Position };
            comm.Broadcast(ref meowSignal, 0);
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
            return cats.First(c => c.Rank == rank) ?? rats.First(r => r.Rank == rank);
        }
    }
}

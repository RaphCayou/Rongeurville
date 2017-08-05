using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using MPI;
using Priority_Queue;
using Rongeurville.Communication;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected const int NO_PATH = -1;
        private const int MAP_RANK = 0;
        private const int DEFAULT_TAG = 0;
        protected int rank;

        protected Tile currentTile;
        public Tile CurrentTile => currentTile;

        protected Map map;
        protected Intracommunicator comm;
        protected bool shouldDie;


        /// <summary>
        /// Gives the tiles around the target tile.
        /// </summary>
        /// <param name="center">The tile to get neighbors.</param>
        /// <returns>The surrounding tiles.</returns>
        public abstract List<Tile> GetNeighbors(Tile center);

        /// <summary>
        /// Return if the Actor can go on the targeted tile.
        /// </summary>
        /// <param name="content">The content of the tile that we want to go on.</param>
        /// <returns>Return true if we can go on that tile.</returns>
        public abstract bool CanGoToNeighbor(TileContent content);

        /// <summary>
        /// Return if the targeted tile is an objective.
        /// </summary>
        /// <param name="target">The targeted tile.</param>
        /// <returns>Return true if the tile is a goal.</returns>
        public abstract bool IsGoal(Tile target);

        /// <summary>
        /// Signal that the Actor has move.
        /// </summary>
        /// <param name="distanceToObjective">Distance with the current objective.</param>
        protected abstract void MoveEvent(int distanceToObjective);
        /// <summary>
        /// Signal the Actor that a cat meow on the map.
        /// </summary>
        /// <param name="moewTile">The tile that the cap meow on.</param>
        protected abstract void ListenMeow(Tile moewTile);

        /// <summary>
        /// Return if the actor still have an objective on the map.
        /// </summary>
        /// <returns>True if the map still contain objective.</returns>
        protected abstract bool IHaveAGoalRemaning();

        public abstract TileContent GetTileContent();

        protected Actor()
        {
            shouldDie = false;
        }

        /// <summary>
        /// Constructor for Actor
        /// </summary>
        /// <param name="communicator">MPI Communicator</param>
        protected Actor(Intracommunicator communicator)
        {
            comm = communicator;
            rank = comm.Rank;
            shouldDie = false;
        }

        /// <summary>
        /// Start the alive process
        /// </summary>
        public void Start()
        {
            StartSignal startSignal = comm.Receive<StartSignal>(0, 0);
            // We wait for the broadcast of the start of the game.

            map = startSignal.Map;
            currentTile = map.GetTileByCoordinates(startSignal.Position);

            AliveLoop();
        }

        public void SetMapAndCurrentTile(Map newMap, Tile newCurrentTile)
        {
            this.currentTile = newCurrentTile;
            this.map = newMap;
        }

        /// <summary>
        /// Find the closest objective to go on. Dijkstra Pathfinding
        /// </summary>
        /// <returns>Next to tile to go on and the cost to go on that tile. Postion is null and cost is equal to NO_COST if path find.</returns>
        //http://www.redblobgames.com/pathfinding/a-star/introduction.html Python -> C#
        public Tuple<Coordinates, int> GetDirection()
        {
            // We make sure that we still have a possible target.
            if (!IHaveAGoalRemaning())
            {
                return Tuple.Create(currentTile.Position, 0);
            }
            SimplePriorityQueue<Tile> frontier = new SimplePriorityQueue<Tile>();
            frontier.Enqueue(currentTile, 0);
            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
            Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int> {{currentTile,0}};
            Tile current = currentTile, last = currentTile;

            // While we still have tiles to go.
            while (frontier.Any())
            {
                current = frontier.Dequeue();

                // If the tile is an objective, we stop the search.
                if (IsGoal(current))
                    break;

                foreach (Tile next in GetNeighbors(current))
                {
                    int newCost = costSoFar[current] + 1;
                    // We update or add the cost of the tile so far.
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        frontier.Enqueue(next, newCost);
                        cameFrom[next] = current;
                    }
                }
            }

            // We reconstruct the path the find the next tile to go on.
            int cost = costSoFar[cameFrom[current]] + 1;
            while (!current.Equals(currentTile))
            {
                last = current;
                current = cameFrom[current];
            }

            return Tuple.Create(last.Position, cost);
        }

        /// <summary>
        /// Determine if the tile is a target for the actor
        /// </summary>
        /// <param name="target">Target tile</param>
        /// <returns>Tile is a goal for the actor</returns>
        private double GetEstimate(Tile target)
        {
            //We are using the Dijkstra, we each tile have an estimate of 1 except the goals
            return IsGoal(target) ? 0 : 1;
        }

        /// <summary>
        /// While the actor is alive:
        ///     find a move position,
        ///     update map with other actor's moves,
        ///     and communicate move intentions with map
        /// </summary>
        public void AliveLoop()
        {
            while (!shouldDie)
            {
                Tuple<Coordinates, int> searchResult = GetDirection();
                MoveEvent(searchResult.Item2);
                // If we did not find a path, we stay where we are.
                Coordinates targetCoordinates = searchResult.Item2 == NO_PATH
                    ? currentTile.Position
                    : searchResult.Item1;

                comm.Send(new MoveRequest { Rank = rank, DesiredTile = targetCoordinates }, MAP_RANK, DEFAULT_TAG);

                bool waitingMoveResponse = true;
                // We wait to receive our new position.
                while (waitingMoveResponse)
                {
                    waitingMoveResponse = !HandleMessage(comm.Receive<Message>(MAP_RANK, DEFAULT_TAG));
                }
            }
            // We confirme to the map that we are dead.
            comm.Send(new DeathConfirmation { Rank = rank }, MAP_RANK, DEFAULT_TAG);
        }

        /// <summary>
        /// Handle the message and call the corresponding methods. Return if the message was our response.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <returns>True if the message was targeted at the current actor.</returns>
        private bool HandleMessage(Message message)
        {
            MoveSignal moveSignal = message as MoveSignal;
            if (moveSignal != null)
            {
                // Update the map based on the move received.
                map.ApplyMove(moveSignal.InitialTile, moveSignal.FinalTile);
                if (moveSignal.InitialTile.Equals(currentTile.Position))
                {
                    currentTile = map.Tiles[moveSignal.FinalTile.Y, moveSignal.FinalTile.X];
                    return true;
                }
                return false;
            }

            MeowSignal meowSignal = message as MeowSignal;
            if (meowSignal != null)
            {
                // We notif that a cat Meow.
                ListenMeow(map.Tiles[meowSignal.MeowLocation.Y, meowSignal.MeowLocation.X]);
                return false;
            }

            KillSignal killSignal = message as KillSignal;
            if (killSignal != null)
            {
                // The map said that we need to die.
                shouldDie = true;
                return true;
            }

            return false;
        }
    }
}
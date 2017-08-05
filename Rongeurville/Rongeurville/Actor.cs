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
        protected int rank;

        protected Tile currentTile;
        public Tile CurrentTile => currentTile;

        protected Map map;
        protected Intracommunicator comm;
        protected bool shouldDie;


        public abstract List<Tile> GetNeighbors(Tile center);
        public abstract bool CanGoToNeighbor(TileContent content);
        public abstract bool IsGoal(Tile target);
        protected abstract void MoveEvent(int distanceToObjective);
        protected abstract void ListenMeow(Tile moewTile);
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
            if (!IHaveAGoalRemaning())
            {
                return Tuple.Create(currentTile.Position, 0);
            }
            SimplePriorityQueue<Tile> frontier = new SimplePriorityQueue<Tile>();
            frontier.Enqueue(currentTile, 0);
            Dictionary<Tile, Tile> came_from = new Dictionary<Tile, Tile>();
            Dictionary<Tile, int> cost_so_far = new Dictionary<Tile, int> {{currentTile,0}};
            Tile current = currentTile, last = currentTile;

            while (frontier.Any())
            {
                current = frontier.Dequeue();

                if (IsGoal(current))
                    break;

                foreach (Tile next in GetNeighbors(current))
                {
                    int newCost = cost_so_far[current] + 1;
                    if (!cost_so_far.ContainsKey(next) || newCost < cost_so_far[next])
                    {
                        cost_so_far[next] = newCost;
                        frontier.Enqueue(next, newCost);
                        came_from[next] = current;
                    }
                }
            }

            int cost = cost_so_far[came_from[current]] + 1;
            while (!current.Equals(currentTile))
            {
                last = current;
                current = came_from[current];
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
                Coordinates targetCoordinates = searchResult.Item2 == NO_PATH
                    ? currentTile.Position
                    : searchResult.Item1;

                comm.Send(new MoveRequest { Rank = rank, DesiredTile = targetCoordinates }, 0, 0);

                bool waitingMoveResponse = true;
                while (waitingMoveResponse)
                {
                    waitingMoveResponse = !HandleMessage(comm.Receive<Message>(0, 0));
                }
            }
            comm.Send(new DeathConfirmation { Rank = rank }, 0, 0);
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
                ListenMeow(map.Tiles[meowSignal.MeowLocation.Y, meowSignal.MeowLocation.X]);
                return false;
            }

            KillSignal killSignal = message as KillSignal;
            if (killSignal != null)
            {
                shouldDie = true;
                return true;
            }

            return false;
        }
    }
}
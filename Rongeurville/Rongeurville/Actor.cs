using System;
using System.Collections.Generic;
using System.Linq;
using MPI;
using Rongeurville.Communication;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected const int NO_PATH = -1;
        protected int rank;

        protected Tile currentTile;

        protected Map map;
        protected Intracommunicator comm;
        protected bool shouldDie;

        public abstract List<Tile> GetNeighbors(Tile center);
        public abstract bool IsGoal(Tile target);
        protected abstract void MoveEvent(int distanceToObjective);
        protected abstract void ListenMeow(Tile moewTile);

        public abstract TileContent GetTileContent();

        protected Actor(Intracommunicator communicator)
        {
            comm = communicator;
            rank = comm.Rank;
            shouldDie = false;
        }

        public void Start()
        {
            map = comm.Receive<Map>(0, 0);
            currentTile = map.GetCurrentTileByRank(rank);
            AliveLoop();
        }

        /// <summary>
        /// Find the closest objective to go on.
        /// </summary>
        /// <returns>Next to tile to go on and the cost to go on that tile. Postion is null and cost is equal to NO_COST if path find.</returns>
        public Tuple<Coordinates, int> GetDirection()
        {
            PathTile lookingTile =
                new PathTile { CostSoFar = 0, Estimate = GetEstimate(currentTile), Value = currentTile };
            bool pathFind = false;
            int pathCost = -1;
            List<PathTile> openedTiles = new List<PathTile>();
            List<PathTile> closedTiles = new List<PathTile>();

            openedTiles.Add(lookingTile);
            while (!pathFind)
            {
                if (!openedTiles.Any())
                {
                    pathFind = true;
                    pathCost = NO_PATH;
                }
                openedTiles.Sort((tile1, tile2) => tile1.TotalCost().CompareTo(tile2.TotalCost()));
                lookingTile = openedTiles[0];
                openedTiles.RemoveAt(0);
                if (IsGoal(lookingTile.Value))
                {
                    pathFind = true;
                    pathCost = lookingTile.CostSoFar;
                }
                closedTiles.Add(lookingTile);
                foreach (Tile tile in GetNeighbors(lookingTile.Value))
                {
                    PathTile neighbor = new PathTile
                    {
                        CostSoFar = lookingTile.CostSoFar + 1,
                        Estimate = GetEstimate(tile),
                        Value = tile,
                        Parent = lookingTile
                    };
                    foreach (List<PathTile> pathTiles in new[] { openedTiles, closedTiles })
                    {
                        PathTile inPathTile =
                            pathTiles.FirstOrDefault(
                                openTile => Equals(openTile.Value, neighbor.Value) &&
                                            openTile.CostSoFar >= neighbor.CostSoFar);
                        if (inPathTile != null)
                        {
                            pathTiles.Remove(inPathTile);
                            openedTiles.Add(neighbor);
                        }
                    }
                    if (!openedTiles.Contains(neighbor) && !closedTiles.Contains(neighbor))
                    {
                        openedTiles.Add(neighbor);
                    }
                }
            }
            Tile tileToGo = null;
            if (pathCost != NO_PATH)
            {
                tileToGo = currentTile;
                while (lookingTile.Parent != null)
                {
                    tileToGo = lookingTile.Value;
                }
            }

            return new Tuple<Coordinates, int>(tileToGo?.Position, pathCost);
        }

        private double GetEstimate(Tile target)
        {
            //We are using the Dijkstra, we each tile have an estimate of 1 except the goals
            return IsGoal(target) ? 0 : 1;
        }

        public void AliveLoop()
        {
            while (!shouldDie)
            {
                Tuple<Coordinates, int> searchResult = GetDirection();
                MoveEvent(searchResult.Item2);
                Coordinates targetCoordinates = searchResult.Item2 == NO_PATH
                    ? currentTile.Position
                    : searchResult.Item1; 
                comm.ImmediateSend(new MoveRequest { Rank = rank, DesiredTile = targetCoordinates }, 0, 0);
                bool waitingMoveResponse = true;
                while (waitingMoveResponse)
                {
                    Message message = comm.Receive<Message>(0, 0);
                    waitingMoveResponse = !HandleMessage(message);
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
                if (Equals(moveSignal.InitialTile, currentTile.Position))
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
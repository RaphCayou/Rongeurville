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
            //Console.WriteLine("Constructing Actor");
            comm = communicator;
            rank = comm.Rank;
            shouldDie = false;
        }

        /// <summary>
        /// Start the alive process
        /// </summary>
        public void Start()
        {
            //Console.WriteLine("Starting Actor" + rank);
            StartSignal mapReceived = new StartSignal();
            comm.Broadcast(ref mapReceived, 0);
            map = mapReceived.Map;
            //Console.WriteLine(map);

            //map = comm.Receive<StartSignal>(0, 0).Map;
            //Console.WriteLine("Map received in Actor" + rank);
            currentTile = map.GetCurrentTileByRank(rank);
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
        public Tuple<Coordinates, int> GetDirection()
        {

            //Console.WriteLine("Actor is getting a direction" + rank);
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
                    //Console.WriteLine("Actor did not find a direction." + rank);
                }
                else
                {
                    openedTiles.Sort((tile1, tile2) => tile1.TotalCost().CompareTo(tile2.TotalCost()));
                    lookingTile = openedTiles[0];
                    openedTiles.RemoveAt(0);
                    if (IsGoal(lookingTile.Value))
                    {
                        pathFind = true;
                        pathCost = lookingTile.CostSoFar;
                    }
                    else
                    {
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
                            //Console.WriteLine($"Testing the tile x:{neighbor.Value.X} Y:{neighbor.Value.Y} and the cost:{neighbor.CostSoFar}, {neighbor.Estimate}");
                            foreach (List<PathTile> pathTiles in new[] { openedTiles, closedTiles })
                            {
                                PathTile inPathTile =
                                    pathTiles.FirstOrDefault(
                                        openTile => openTile.Value.Equals(neighbor.Value) &&
                                                    openTile.CostSoFar >= neighbor.CostSoFar);
                                if (inPathTile != null)
                                {
                                    //Console.WriteLine($"The new tile is better.Old:{inPathTile.Value.X}x{inPathTile.Value.Y} cost:{inPathTile.TotalCost()} New:{neighbor.Value.X}x{neighbor.Value.Y} cost:{neighbor.TotalCost()}");
                                    pathTiles.RemoveAll(pathTile =>pathTile.Value.Equals(inPathTile.Value));
                                    openedTiles.Add(neighbor);
                                }
                            }
                            if (!openedTiles.Contains(neighbor) && !closedTiles.Contains(neighbor))
                            {
                                openedTiles.Add(neighbor);
                            }
                        }
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
                    lookingTile = lookingTile.Parent;
                }
            }

            //Console.WriteLine($"{rank} Actor at {currentTile.Position} got a direction {tileToGo?.Position} in {closedTiles.Count} with the cost {pathCost}  " + rank);
            return new Tuple<Coordinates, int>(tileToGo?.Position, pathCost);
        }

        //http://www.redblobgames.com/pathfinding/a-star/introduction.html Python -> C#
        public Tuple<Coordinates, int> GetDirection(bool isThisDijkstraBetter)
        {
            SimplePriorityQueue<Tile> frontier = new SimplePriorityQueue<Tile>();
            frontier.Enqueue(currentTile, 0);
            Dictionary<Tile, Tile> came_from = new Dictionary<Tile, Tile>();
            Dictionary<Tile, int> cost_so_far = new Dictionary<Tile, int> {{currentTile,0}};
            Tile current = currentTile, last = currentTile;

            while (frontier.Count != 0)
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

            int cost = 0;
            while (!current.Equals(currentTile))
            {
                cost++;
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

            //Console.WriteLine("Actor is Alive" + rank);
            while (!shouldDie)
            {
                Console.WriteLine($"Actor {rank} begins its path searching");
                Tuple<Coordinates, int> searchResult = GetDirection(true);
                MoveEvent(searchResult.Item2);
                Coordinates targetCoordinates = searchResult.Item2 == NO_PATH
                    ? currentTile.Position
                    : searchResult.Item1;
                //Console.WriteLine("Actor is sending Move" + rank);
                Console.WriteLine($"Actor {rank} ends its path searching");
                comm.Send(new MoveRequest { Rank = rank, DesiredTile = targetCoordinates }, 0, 0);
                bool waitingMoveResponse = true;
                while (waitingMoveResponse)
                {
                    //Console.WriteLine("Actor is receiving Move" + rank);
                    //Message message = null;
                    if (rank == 3)
                        Console.WriteLine($"Actor {rank} ready for message");
                    Message message = comm.Receive<Message>(0, 0);
                    //comm.Broadcast<Message>(ref message, 0);
                    if (rank == 3)
                        Console.WriteLine($"Actor {rank} receives message");

                    waitingMoveResponse = !HandleMessage(message);
                }
                //Console.WriteLine("Actor as received is Move" + rank);
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
                Console.WriteLine($"****** Actor #{rank} called ApplyMove");
                map.ApplyMove(moveSignal.InitialTile, moveSignal.FinalTile);
                //Console.WriteLine($"{rank} Receive move: initial: {moveSignal.InitialTile.X} x {moveSignal.InitialTile.Y} target : {moveSignal.FinalTile.X} x {moveSignal.FinalTile.Y} current position {currentTile.X} x {currentTile.Y}");
                if (moveSignal.InitialTile.Equals(currentTile.Position))
                {
                    //Console.WriteLine("Actor got is new postion " + rank);
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
                Console.WriteLine($"Actor {rank} knows that : Following actors should die : {aaaa(killSignal.RanksTargeted)}. {killSignal.KillAll}");
                if (killSignal.KillAll || killSignal.RanksTargeted.Contains(rank))
                {
                    Console.WriteLine($"actor {rank} accepts its death");
                    shouldDie = true;
                    return true;
                }
                return false;
            }

            return false;
        }

        private string aaaa(List<int> list)
        {
            if (list == null)
            {
                return "(empty list)";
            }
            string testt = "(";
            foreach (int i in list)
            {
                testt = testt + ", " + i.ToString();
            }
            return testt + ")";
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected const int NO_PATH = -1;
        protected int rank;

        protected Tile currentTile;

        protected Map map;
        protected Intracommunicator comm;
        protected bool shouldDie = false;

        public abstract List<Tile> GetNeighbors(Tile center);
        public abstract bool IsGoal(Tile target);
        protected abstract void DoYourThings();

        public abstract TileContent GetTileContent();

        protected Actor(Intracommunicator communicator)
        {
            comm = communicator;
            rank = comm.Rank;
        }

        public void Start()
        {
            //TODO reveive la map et le start location et le signal de debut de partie
            DoThing();
        }

        /// <summary>
        /// Find the closest objective to go on.
        /// </summary>
        /// <returns>Next to tile to go on and the cost to go on that tile.</returns>
        public Tuple<Tile, int> GetDirection()
        {
            PathTile lookingTile = new PathTile { CostSoFar = 0, Estimate = GetEstimate(currentTile), Value = currentTile };
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
                    foreach (List<PathTile> aStarTiles in new[] { openedTiles, closedTiles })
                    {
                        PathTile inOpened =
                            aStarTiles.FirstOrDefault(
                                openTile => Equals(openTile.Value, neighbor.Value) &&
                                            openTile.CostSoFar >= neighbor.CostSoFar);
                        if (inOpened != null)
                        {
                            aStarTiles.Remove(inOpened);
                            openedTiles.Add(neighbor);
                        }
                    }
                    if (!openedTiles.Contains(neighbor) && !closedTiles.Contains(neighbor))
                    {
                        openedTiles.Add(neighbor);
                    }
                }
            }
            Tile tileToGo;
            if (pathCost != NO_PATH)
            {
                tileToGo = currentTile;
                while (lookingTile.Parent != null)
                {
                    tileToGo = lookingTile.Value;
                }
            }
            else
            {
                tileToGo = new Tile();
            }

            return new Tuple<Tile, int>(tileToGo, pathCost);
        }

        private double GetEstimate(Tile target)
        {
            return IsGoal(target) ? 0 : 1;
        }

        public void DoThing()
        {
            while (!shouldDie)
            {
                DoYourThings();
                //TODO update avec tout les mises à jours de la carte(tant que notre position est pas update)
            }
        }
    }
}
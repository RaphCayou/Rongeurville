using System;
using System.Collections.Generic;
using System.Linq;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected const int NO_PATH = -1;
        protected int rang;

        protected Tile currentTile;

        protected Map map;
        public abstract List<Tile> GetNeighboors(Tile center);

        public abstract TileContent GetTileContent();

        /// <summary>
        /// TODO tester que tout est ok.
        /// </summary>
        /// <param name="target">Targeted tile.</param>
        /// <returns>Next to tile to go on.</returns>
        public Tuple<Tile, int> GetDirectionWithAStar(Tile target)
        {
            AStarTile lookingTile = new AStarTile { CostSoFar = 0, Estimate = GetDistance(target), Value = currentTile };
            bool pathFind = false;
            int pathCost = -1;
            List<AStarTile> openedTiles = new List<AStarTile>();
            List<AStarTile> closedTiles = new List<AStarTile>();

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
                if (Equals(lookingTile.Value, target))
                {
                    pathFind = true;
                    pathCost = lookingTile.CostSoFar;
                }
                closedTiles.Add(lookingTile);
                foreach (Tile tile in GetNeighboors(lookingTile.Value))
                {
                    AStarTile neighboor = new AStarTile
                    {
                        CostSoFar = lookingTile.CostSoFar + 1,
                        Estimate = GetDistance(tile),
                        Value = tile,
                        Parent = lookingTile
                    };
                    foreach (List<AStarTile> aStarTiles in new[] { openedTiles, closedTiles })
                    {
                        AStarTile inOpened =
                            aStarTiles.FirstOrDefault(
                                openTile => Equals(openTile.Value, neighboor.Value) &&
                                            openTile.CostSoFar >= neighboor.CostSoFar);
                        if (inOpened != null)
                        {
                            aStarTiles.Remove(inOpened);
                            openedTiles.Add(neighboor);
                        }
                    }
                    if (!openedTiles.Contains(neighboor) && !closedTiles.Contains(neighboor))
                    {
                        openedTiles.Add(neighboor);
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

        private double GetDistance(Tile target)
        {
            return Math.Sqrt(Math.Pow(currentTile.X - target.X, 2) + Math.Pow(currentTile.Y - target.Y, 2));
        }
    }
}
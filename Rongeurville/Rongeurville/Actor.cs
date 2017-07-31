using System;
using System.Collections.Generic;
using System.Linq;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected int rang;

        protected Tile currentTile;

        //protected Map map;
        public abstract List<Tile> GetNeighboors(Tile center);

        public KeyValuePair<Tile, bool> GetDirectionWithAStar(Tile target)
        {
            AStarTile lookingTile = new AStarTile { CostSoFar = 0, Estimate = GetDistance(target), Value = currentTile };
            AStarTile nextLookingTile;
            bool pathFind = false;
            bool pathValid = true;
            SortedList<int, AStarTile> openedTiles = new SortedList<int, AStarTile>();
            List<Tile> closedTiles = new List<Tile>();

            openedTiles.Add(lookingTile.CostSoFar, lookingTile);
            while (!pathFind)
            {
                if (!openedTiles.Any())
                {
                    pathFind = true;
                    pathValid = false;
                }
                lookingTile = openedTiles[0];
            }

            return new KeyValuePair<Tile, bool>(new Tile(), pathValid);
        }

        private double GetDistance(Tile target)
        {
            return Math.Sqrt(Math.Pow(currentTile.X - target.X, 2) + Math.Pow(currentTile.Y - target.Y, 2));
        }
    }
}
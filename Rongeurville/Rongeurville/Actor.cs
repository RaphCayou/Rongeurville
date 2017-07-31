using System;
using System.Collections.Generic;
using System.Linq;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected int rang;

        protected Tile currentTile;

        protected Map map;
        public abstract List<Tile> GetNeighboors(Tile center);

        public Tuple<Tile, int> GetDirectionWithAStar(Tile target)
        {
            AStarTile lookingTile = new AStarTile { CostSoFar = 0, Estimate = GetDistance(target), Value = currentTile };
            AStarTile nextLookingTile;
            bool pathFind = false;
            int pathCost = -1;
            SortedList<int, AStarTile> openedTiles = new SortedList<int, AStarTile>();
            List<AStarTile> closedTiles = new List<AStarTile>();

            openedTiles.Add(lookingTile.CostSoFar, lookingTile);
            while (!pathFind)
            {
                if (!openedTiles.Any())
                {
                    pathFind = true;
                    pathCost = -1;
                }
                lookingTile = openedTiles[0];
                openedTiles.RemoveAt(0);
                if (lookingTile.Value == target)
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
                    KeyValuePair<int, AStarTile> inOpened = openedTiles.First(pair => pair.Value.Value == neighboor.Value && pair.Value.CostSoFar >= neighboor.CostSoFar);
                    //if (inOpened)
                    //{
                        
                    //}
                }
            }

            return new Tuple<Tile, int>(new Tile(), pathCost);
        }

        private double GetDistance(Tile target)
        {
            return Math.Sqrt(Math.Pow(currentTile.X - target.X, 2) + Math.Pow(currentTile.Y - target.Y, 2));
        }
    }
}
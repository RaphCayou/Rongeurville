using System.Collections.Generic;
using System.Linq;

namespace Rongeurville
{
    public abstract class Actor
    {
        protected int rang;
        protected Tile currentTile;
        //protected Map map;

        public KeyValuePair<Tile, bool> GetDirectionWithAStar(Tile target)
        {
            Tile lookingTile;
            Tile nextLookingTile;
            bool pathFind = false;
            bool pathValid = false;
            List<Tile> openedTiles = new List<Tile>();
            List<Tile> closedTiles = new List<Tile>();
            openedTiles.Add(currentTile);
            while (!pathFind)
            {
                if (!openedTiles.Any())
                {
                    pathFind = true;
                    pathValid = false;
                }
                //lookingTile = openedTiles.fi
            }

            return new KeyValuePair<Tile, bool>(new Tile(), pathValid);
        }

        public abstract List<Tile> GetNeighboors(Tile center);
    }
}
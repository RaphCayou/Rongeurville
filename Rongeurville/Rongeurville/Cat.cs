using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace Rongeurville
{
    public class Cat : Actor
    {
        private static readonly TileContent[] GO_THROUGH = { TileContent.Rat, TileContent.Empty };
        private bool ShouldDie { get; set; } = false;
        private Intracommunicator comm;

        public Cat()
        {
            //Task.Run(() => DoCatThings());
            comm = Communicator.world;
            DoCatThings();
        }

        private void DoCatThings()
        {
            while (!ShouldDie)
            {
                // Get closest rat
                int closestRatDistance = int.MaxValue;
                Tile closestRat = null;
                List<Tile> rats = map.Rats;
                foreach (var rat in rats)
                {
                    Tuple<Tile, int> aStarResult = GetDirectionWithAStar(rat);
                    if (aStarResult.Item2 == -1) continue;
                    if (aStarResult.Item2 >= closestRatDistance) continue;
                    closestRatDistance = aStarResult.Item2;
                    closestRat = rat;
                }
                // MEOW
                if (closestRatDistance <= 10)
                {
                    var request = comm.ImmediateSend("MEOW", 0, 0);
                }
                // Communicate intent with map
                if (closestRat != null)
                {
                    ;
                    string response;
                    comm.SendReceive("PLEASE MOVE CAT (RANG) TO DEST (closestRat)", 0, 0, out response);
                    // TODO Move according to response, Die if necessary
                }
            }
        }

        public override List<Tile> GetNeighboors(Tile center)
        {
            List<Tile> neighbors = new List<Tile>();
            // UP
            if (center.Y - 1 >= 0 && GO_THROUGH.Contains(map.Tiles[center.Y - 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y - 1, center.X]);
            }
            // DOWN
            if (center.Y + 1 < map.Height && GO_THROUGH.Contains(map.Tiles[center.Y + 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y + 1, center.X]);
            }

            // LEFT
            if (center.X - 1 >= 0 && GO_THROUGH.Contains(map.Tiles[center.Y, center.X - 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y, center.X - 1]);
            }

            // RIGHT
            if (center.X + 1 < map.Width && GO_THROUGH.Contains(map.Tiles[center.Y, center.X + 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y, center.X + 1]);
            }
            return neighbors;
        }

        public override TileContent GetTileContent()
        {
            return TileContent.Cat;
        }
    }
}

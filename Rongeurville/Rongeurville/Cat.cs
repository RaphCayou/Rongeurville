using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace Rongeurville
{
    public class Cat : Actor
    {
        private static readonly TileContent[] GO_THROUGH = { TileContent.Rat, TileContent.Empty };

        public Cat(Intracommunicator communicator) : base(communicator)
        {
        }

        protected override void DoYourThings()
        {
            // Get closest rat
            Tuple<Tile, int> aStarResult = GetDirection();
            // MEOW
            if (aStarResult.Item2 <= 10)
            {
                var request = comm.ImmediateSend("MEOW", 0, 0);
            }
            // Communicate intent with map
            string response;
            comm.SendReceive("PLEASE MOVE CAT (RANG) TO DEST (closestRat)", 0, 0, out response);
            // TODO Move according to response, Die if necessary
        }

        public override List<Tile> GetNeighbors(Tile center)
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

        public override bool IsGoal(Tile target)
        {
            return target.Content == TileContent.Rat;
        }

        public override TileContent GetTileContent()
        {
            return TileContent.Cat;
        }
    }
}

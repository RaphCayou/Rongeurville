using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Rongeurville
{
    public class Rat : Actor
    {
        private static readonly TileContent[] GO_THROUGH = { TileContent.Cheese, TileContent.Empty };
        private int timeSinceLastMeow;
        private Tile lastMeowLocation;

        public Rat(Intracommunicator communicator) : base(communicator)
        {
            timeSinceLastMeow = 0;
        }

        protected override void DoYourThings()
        {
            // TODO Listen for Meows <= 7 tiles from rat
            //if meow -> time += 5
            Tuple<Tile, int> aStarResult = GetDirection();

            // TODO Communicate intent with map

            // Be less scared
            if (timeSinceLastMeow > 0)
            {
                timeSinceLastMeow -= 1;
            }
            throw new NotImplementedException();
        }

        protected override void ListenMoew(Tile moewTile)
        {
            //TODO Count the number of rat step between the tile and the rat
        }

        public override List<Tile> GetNeighbors(Tile center)
        {
            List<Tile> neighbors = new List<Tile>();
            // UP
            if (center.Y - 1 >= 0 && GO_THROUGH.Contains(map.Tiles[center.Y - 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y - 1, center.X]);
            }
            // UP LEFT
            if (center.Y - 1 >= 0 && center.X - 1 >= 0 && GO_THROUGH.Contains(map.Tiles[center.Y - 1, center.X - 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y - 1, center.X - 1]);
            }
            // UP RIGHT
            if (center.Y - 1 >= 0 && center.X + 1 < map.Width && GO_THROUGH.Contains(map.Tiles[center.Y - 1, center.X + 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y - 1, center.X + 1]);
            }

            // DOWN
            if (center.Y + 1 < map.Height && GO_THROUGH.Contains(map.Tiles[center.Y + 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y + 1, center.X]);
            }
            // DOWN LEFT
            if (center.Y + 1 < map.Height && center.X - 1 >= 0 && GO_THROUGH.Contains(map.Tiles[center.Y + 1, center.X - 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y + 1, center.X - 1]);
            }
            // DOWN RIGHT
            if (center.Y + 1 < map.Height && center.X + 1 < map.Width && GO_THROUGH.Contains(map.Tiles[center.Y + 1, center.X + 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y + 1, center.X + 1]);
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
            return timeSinceLastMeow > 0 ? map.Exits.Contains(target) : TileContent.Cheese == target.Content;
        }

        public override TileContent GetTileContent()
        {
            return TileContent.Rat;
        }
    }
}

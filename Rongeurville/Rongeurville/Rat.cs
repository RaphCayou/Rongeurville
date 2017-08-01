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

        public Rat(Intracommunicator communicator) : base(communicator)
        {
            timeSinceLastMeow = 0;
        }

        protected override void MoveEvent(int distanceToObjective)
        {
            // Be less scared
            if (timeSinceLastMeow > 0)
            {
                timeSinceLastMeow -= 1;
            }
        }

        protected override void ListenMeow(Tile meowTile)
        {
            int x = currentTile.X;
            int y = currentTile.Y;
            int steps = 0;

            // UP LEFT Diagonal
            while (meowTile.X > x && meowTile.Y > y)
            {
                steps += 1;
                x--;
                y--;
            }
            // DOWN RIGHT Diagonal
            while (meowTile.X < x && meowTile.Y < y)
            {
                steps += 1;
                x++;
                y++;
            }
            // UP RIGHT Diagonal
            while (meowTile.X < x && meowTile.Y > y)
            {
                steps += 1;
                x++;
                y--;
            }
            // DOWN LEFT Diagonal
            while (meowTile.X > x && meowTile.Y < y)
            {
                steps += 1;
                x--;
                y++;
            }
            // Remaining Manhattan distance
            steps += Math.Abs(meowTile.X - x) + Math.Abs(meowTile.Y - y);

            // Cat is close by
            if (steps <= 7)
            {
                timeSinceLastMeow = 5;
            }
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

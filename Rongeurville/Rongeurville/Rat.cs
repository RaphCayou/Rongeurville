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
        private bool shouldDie = false;
        private int timeSinceLastMeow = 0;
        private Tile lastMeowLocation;
        private Intracommunicator comm;

        public Rat()
        {
            comm = Communicator.world;
            DoRatThings();
        }

        private void DoRatThings()
        {
            while (!shouldDie)
            {
                // Listen for Meows <= 7 tiles from rat
                //if meow -> time += 5

                List<Tile> objectiveList = timeSinceLastMeow > 0 ? map.Exits : map.Cheese;
                int closestObjectiveDistance = int.MaxValue;
                Tile closestObjective = null;
                foreach (var objective in objectiveList)
                {
                    Tuple<Tile, int> aStarResult = GetDirectionWithAStar(objective);
                    if (aStarResult.Item2 == -1) continue;
                    if (aStarResult.Item2 >= closestObjectiveDistance) continue;
                    closestObjectiveDistance = aStarResult.Item2;
                    closestObjective = objective;
                }


                // Communicate intent with map


                // Be less scared
                if (timeSinceLastMeow > 0)
                {
                    timeSinceLastMeow -= 1;
                }
            }
            throw new NotImplementedException();
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
    }
}

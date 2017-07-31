using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Rongeurville
{
    public class Rat:Actor
    {
        private bool shouldDie = false;
        private bool shouldBeScared = false;
        private int timeSinceLastMeow = 0;
        private Tile lastMeow;
        private Intracommunicator comm;

        public Rat()
        {
            comm = Communicator.world;
            DoRatThings();
        }

        private void DoRatThings()
        {
            // Listen for Meows


            List<Tile> objectiveList = shouldBeScared ? map.Exits : map.Cheese;
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


            // MOVE


            if (timeSinceLastMeow > 0)
            {
                timeSinceLastMeow -= 1;
            }
            throw new NotImplementedException();
        }

        public override List<Tile> GetNeighboors(Tile center)
        {
            throw new NotImplementedException();
        }
    }
}

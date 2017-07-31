using System;
using System.Collections.Generic;
using MPI;

namespace Rongeurville
{
    public class Cat:Actor
    {
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
                {;
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
            
            // DOWN

            // LEFT

            // RIGHT
            throw new NotImplementedException();
        }
    }
}

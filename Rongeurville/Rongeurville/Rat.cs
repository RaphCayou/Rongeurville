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
        private bool ShouldDie { get; set; } = false;
        private Intracommunicator comm;


        public Rat()
        {
            comm = Communicator.world;
            DoRatThings();
        }

        private void DoRatThings()
        {
            throw new NotImplementedException();
        }

        public override List<Tile> GetNeighboors(Tile center)
        {
            throw new NotImplementedException();
        }
    }
}

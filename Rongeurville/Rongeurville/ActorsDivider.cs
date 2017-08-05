using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rongeurville
{
    class ActorsDivider
    {
        public int NumberOfRats { get; }
        public int NumberOfCats { get; }

        public ActorsDivider(int numberOfRats, int numberOfCats)
        {
            NumberOfRats = numberOfRats;
            NumberOfCats = numberOfCats;
        }

        public ProcessType GetProcesTypeByRank(int rank)
        {
            if (rank == 0)
                return ProcessType.Map;
            if (rank <= NumberOfRats)
                return ProcessType.Rat;
            return ProcessType.Cat;
        }
    }
}

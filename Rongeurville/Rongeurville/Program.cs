using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Rongeurville
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;

                if (args.Length != 3)
                    throw new ArgumentException("You must have 3 command line arguments : the map file path, the number of rats and the number of cats");
                
                string mapFilePath = args[0];
                int nbrRats = Int32.Parse(args[1]);
                int nbrCats = Int32.Parse(args[2]);

                if (nbrRats + nbrCats + 1 != comm.Size)
                    throw new ArgumentException("The total number of actors (rats + cats + map) must be equal to the number of processes");

                ActorsDivider divider = new ActorsDivider(nbrRats, nbrCats);

                switch (divider.GetProcesTypeByRank(comm.Rank))
                {
                    case ProcessType.Map:
                        Console.WriteLine("Creating Map ... from rank {0}", comm.Rank);
                        new MapManager(comm, mapFilePath, divider).Start();
                        Console.WriteLine("~ Closing Map ... from rank {0}", comm.Rank);
                        break;
                    case ProcessType.Rat:
                        Console.WriteLine("Creating Rat ... from rank {0}", comm.Rank);
                        new Rat(comm).Start();
                        Console.WriteLine("~ Closing Rat ... from rank {0}", comm.Rank);
                        break;
                    case ProcessType.Cat:
                        Console.WriteLine("Creating Cat ... from rank {0}", comm.Rank);
                        new Cat(comm).Start();
                        Console.WriteLine("~ Closing Cat ... from rank {0}", comm.Rank);
                        break;
                }
            }
        }
    }
}

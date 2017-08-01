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

                if (args.Length != 2)
                    throw new ArgumentException("You must have 2 command line arguments : the number of rats followed by the number of cats");
                
                int nbrRats = Int32.Parse(args[0]);
                int nbrCats = Int32.Parse(args[1]);

                if (nbrRats + nbrCats + 1 != comm.Size)
                    throw new ArgumentException("The total nuumber of actors (rats + cats + map) must be equal to the number of processes");
                
                if (comm.Rank == 0) // Master
                {
                    Console.WriteLine("Creating Map ... from rank {0}", comm.Rank);
                    new MapManager(comm);//.Start()
                    Console.WriteLine("~ Closing Map ... from rank {0}", comm.Rank);
                }
                else // Slave
                {
                    if (comm.Rank > 0 && comm.Rank <= nbrRats)
                    {
                        Console.WriteLine("Creating Rat ... from rank {0}", comm.Rank);
                        new Rat(comm);//.Start();
                        Console.WriteLine("~ Closing Rat ... from rank {0}", comm.Rank);
                    }
                    else if (comm.Rank > nbrRats && comm.Rank <= comm.Size)
                    {
                        Console.WriteLine("Creating Cat ... from rank {0}", comm.Rank);
                        new Cat(comm);//.Start();
                        Console.WriteLine("~ Closing Cat ... from rank {0}", comm.Rank);
                    }
                }
            }
        }
    }
}

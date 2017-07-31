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
                // MPI program goes here!
                Console.WriteLine("Hello, World! from rank " + Communicator.world.Rank
                                  + " (running on " + MPI.Environment.ProcessorName + ")");

                Intracommunicator comm = Communicator.world;
                if (comm.Rank == 0)
                {
                    // program for rank 0
                    Map map = new Map(comm);
                    
                }
                else // not rank 0
                {
                    // program for all other ranks
                    comm.Send("Hello", 0, 0);
                }
            }
        }
    }
}

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
                
                Console.WriteLine("Opening MPI ... from rank {0} (running on {1})", comm.Rank, MPI.Environment.ProcessorName);
                
                if (comm.Rank == 0)
                {
                    // program for rank 0
                    MapManager mapManager = new MapManager(comm);
                    
                }
                else // not rank 0
                {
                    // program for all other ranks
                    comm.Send("Hello", 0, 0);
                }

                Console.WriteLine("Closing MPI ... from rank {0}", comm.Rank);
            }
        }
    }
}

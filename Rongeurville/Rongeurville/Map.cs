using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Rongeurville
{
    public class Map
    {
        public Map(Intracommunicator comm)
        {
            Console.WriteLine("Map constructed.");

            while (true)
            {
                //string msg = comm.Receive<string>(Communicator.anySource, 0);
                ReceiveRequest rr = comm.ImmediateReceive<string>(Communicator.anySource, 0);
                string msg = (string) rr.GetValue();
                Console.WriteLine(msg + " from " + comm.Rank);
            }
        }
    }
}

using MPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rongeurville
{
    class MapManager
    {
        private Map map;

        public MapManager(Intracommunicator comm)
        {
            Console.WriteLine("Map constructed.");

            //while (true)
            //{
            //    //string msg = comm.Receive<string>(Communicator.anySource, 0);
            //    ReceiveRequest rr = comm.ImmediateReceive<string>(Communicator.anySource, 0);
            //    string msg = (string)rr.GetValue();
            //    Console.WriteLine(msg + " from " + comm.Rank);
            //}

            Map map = Map.LoadMapFromFile("map.txt");

            Console.WriteLine(map.ToString());
            Console.ReadKey();
        }
    }
}

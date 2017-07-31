using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Rongeurville
{
    public class Cat
    {
        private bool ShouldDie { get; set; } = false;

        public Cat()
        {
            //Task.Run(() => DoCatThings());
            DoCatThings();
        }

        private void DoCatThings()
        {
            while (!ShouldDie)
            {
                //Get closest rat
                //if rat <= 10 tiles, MEOW
                //Get path to the rat
                //Send move command to map
                //Listen for new position or die
            }
        }

        //MEOW

    }
}

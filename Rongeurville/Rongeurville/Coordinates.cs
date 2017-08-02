using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rongeurville
{
    [Serializable]
    public class Coordinates
    {
        public int X;
        public int Y;

        protected bool Equals(Coordinates other)
        {
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}

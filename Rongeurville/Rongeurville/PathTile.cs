using System.Collections.Generic;

namespace Rongeurville
{
    public class PathTile
    {
        public int CostSoFar;
        public double Estimate;
        public Tile Value;
        public PathTile Parent;

        private sealed class ValueEqualityComparer : IEqualityComparer<PathTile>
        {
            public bool Equals(PathTile x, PathTile y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Equals(x.Value, y.Value);
            }

            public int GetHashCode(PathTile obj)
            {
                return (obj.Value != null ? obj.Value.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<PathTile> ValueComparer { get; } = new ValueEqualityComparer();

        /// <summary>
        /// Calculating the cost so far with the estimate.
        /// </summary>
        /// <returns>Total cost value.</returns>
        public double TotalCost()
        {
            return CostSoFar + Estimate;
        }
    }
}
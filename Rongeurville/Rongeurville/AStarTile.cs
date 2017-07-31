namespace Rongeurville
{
    public class AStarTile
    {
        public int CostSoFar;
        public double Estimate;
        public Tile Value;
        public AStarTile Parent;

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
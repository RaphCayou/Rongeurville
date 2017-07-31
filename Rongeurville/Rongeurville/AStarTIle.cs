namespace Rongeurville
{
    public class AStarTile
    {
        public int CostSoFar;
        public double Estimate;
        public Tile Value;
        public AStarTile Parent;

        public double TotalCost()
        {
            return CostSoFar + Estimate;
        }
    }
}
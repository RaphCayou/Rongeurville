namespace Rongeurville
{
    public enum TileContent
    {
        Empty,
        Wall,
        Cheese,
        Cat,
        Rat,
    }
    public class Tile
    {
        public int X;
        public int Y;
        public TileContent Content;

        protected bool Equals(Tile other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }
}
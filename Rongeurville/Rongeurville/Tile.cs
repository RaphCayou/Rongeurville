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
    }
}
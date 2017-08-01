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
        public Coordinates Position;
        public TileContent Content;

        public int X {
            get => Position.X;
            set => Position.X = value;
        }

        public int Y
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        protected bool Equals(Tile other)
        {
            return Position.Equals(other.Position);
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

        /// <summary>
        /// Set Tile content from character
        /// </summary>
        /// <param name="charTileContent"></param>
        public void SetTileContent(char charTileContent)
        {
            switch (charTileContent)
            {
                case '#':
                    Content = TileContent.Wall;
                    break;
                case 'F':
                    Content = TileContent.Cheese;
                    break;
                case 'C':
                    Content = TileContent.Cat;
                    break;
                case 'R':
                    Content = TileContent.Rat;
                    break;
                default:
                    Content = TileContent.Empty;
                    break;
            }
        }

        /// <summary>
        /// Character associated with the tile content
        /// </summary>
        public char FormattedContent
        {
            get
            {
                switch (Content)
                {
                    case TileContent.Wall:
                        return '#';
                    case TileContent.Cheese:
                        return 'F';
                    case TileContent.Cat:
                        return 'C';
                    case TileContent.Rat:
                        return 'R';
                    default:
                        return ' ';
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;
using System.IO;

namespace Rongeurville
{
    public class Map
    {
        public int Height { get; private set; }
        public int Width { get; private set; }

        public Tile[,] Tiles { get; private set; }

        public List<Tile> Rats { get; }
        public List<Tile> Cats { get; }
        public List<Tile> Cheese { get; }
        public List<Tile> Exits { get; }

        public Map()
        {
            Rats = new List<Tile>();
            Cats = new List<Tile>();
            Cheese = new List<Tile>();
            Exits = new List<Tile>();
        }

        public Tile GetTileByCoordinates(Coordinates position)
        {
            return Tiles[position.Y, position.X];
        }

        private bool ValidateDestinationTile(Coordinates source, Coordinates destination)
        {
            if (!(0 <= destination.Y && destination.Y < Height
                && 0 <= destination.X && destination.X < Width))
            {
                return false; // destination not part of the map
            }

            Tile sourceTile = GetTileByCoordinates(source);
            Tile destinationTile = GetTileByCoordinates(destination);

            // Basic validations
            if (destinationTile.Content == TileContent.Wall)
            {
                return false; // Cannot move on a wall
            }

            if (destinationTile.Content == sourceTile.Content)
            {
                return false; // Cannot move to a tile with another actor of the same type as us
            }

            // Validations for actor type
            if (sourceTile.Content == TileContent.Rat && (destinationTile.Content & (TileContent.Cheese | TileContent.Empty)) == 0)
            {
                return false; // Rats can only go on a Cheese or an empty tile
            }

            if (sourceTile.Content == TileContent.Cat && (destinationTile.Content & (TileContent.Rat | TileContent.Empty)) == 0)
            {
                return false; // Cats can only go on a Rat or an empty tile
            }

            // We are trying to move something else than a rat or a cat. Impossible, do not allow it.
            return false;
        }

        public enum MoveEffect
        {
            InvalidMove,
            AcceptedMove,
            CheeseEaten,
            RatCaptured,
            RatEscaped,
        }

        public MoveEffect CheckForMoveEffects(Coordinates source, Coordinates destination)
        {
            if (!ValidateDestinationTile(source, destination))
            {
                return MoveEffect.InvalidMove;
            }

            Tile sourceTile = GetTileByCoordinates(source);
            Tile destinationTile = GetTileByCoordinates(destination);

            if (destinationTile.Content == TileContent.Cheese)
            {
                return MoveEffect.CheeseEaten;
            }

            if (destinationTile.Content == TileContent.Rat)
            {
                return MoveEffect.RatCaptured;
            }

            if (sourceTile.Content == TileContent.Rat && destinationTile.Content == TileContent.Empty && Exits.Contains(destinationTile))
            {
                return MoveEffect.RatEscaped;
            }

            return MoveEffect.AcceptedMove;
        }

        public void ApplyMove(Coordinates source, Coordinates destination)
        {
            // TODO apply move
            Tile sourceTile = GetTileByCoordinates(source);
            Tile destinationTile = GetTileByCoordinates(destination);

            switch (destinationTile.Content)
            {
                case TileContent.Cheese:
                    Cheese.Remove(destinationTile);
                    break;
                case TileContent.Rat:
                    Rats.Remove(destinationTile);
                    break;
            }

            destinationTile.Content = sourceTile.Content;
            sourceTile.Content = TileContent.Empty;
        }

        /// <summary>
        /// Return the Tile associated with the rank of the process. May be a rat or a cat.
        /// </summary>
        /// <param name="rank">Rank of the process</param>
        /// <returns></returns>
        public Tile GetCurrentTileByRank(int rank)
        {
            if (1 <= rank && rank <= Rats.Count)
            {
                return Rats[rank - 1];
            }
            else if (Rats.Count + 1 <= rank && rank <= Rats.Count + Cats.Count)
            {
                return Cats[rank - Rats.Count - 1];
            }

            throw new Exception("Rank is not a cat nor a rat.");
        }

        /// <summary>
        /// Load a file and parse its content to create a map from it.
        /// </summary>
        /// <param name="mapFilePath">The file to open for reading</param>
        /// <returns>The map parsed</returns>
        public static Map LoadMapFromFile(string mapFilePath)
        {
            return ParseMap(File.ReadAllText(mapFilePath));
        }

        /// <summary>
        /// Parse a string containing the map and creates a Map.
        /// </summary>
        /// <param name="mapComposition">String representing the map. Must be a valid map.</param>
        /// <returns>The map parsed</returns>
        public static Map ParseMap(string mapContent)
        {
            Map parsedMap = new Map();

            string[] lines = mapContent.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

            // find farthest # on a line to find map Width
            int biggestIndex = 0;
            foreach (string line in lines)
            {
                int lastMapDelimiter = line.LastIndexOf('#');
                if (lastMapDelimiter > biggestIndex)
                {
                    biggestIndex = lastMapDelimiter;
                }
            }
            parsedMap.Width = 1 + biggestIndex;

            // Find lowest line containing a # to find map Height
            parsedMap.Height = 1 + Array.FindLastIndex(lines, line => { return line.Contains('#'); });

            // Fills the list of tiles with valid instances 
            parsedMap.Tiles = new Tile[parsedMap.Height, parsedMap.Width];
            for (int i = 0; i < parsedMap.Height; ++i)
            {
                for (int j = 0; j < parsedMap.Width; ++j)
                {
                    parsedMap.Tiles[i, j] = new Tile
                    {
                        Y = i,
                        X = j
                    };

                    if (j < lines[i].Length)
                    {
                        // Parse tile content
                        parsedMap.Tiles[i, j].SetTileContent(lines[i][j]);

                        // If the tile is special, put it in the corresponding list
                        switch (parsedMap.Tiles[i, j].Content)
                        {
                            case TileContent.Rat:
                                parsedMap.Rats.Add(parsedMap.Tiles[i, j]);
                                break;
                            case TileContent.Cat:
                                parsedMap.Cats.Add(parsedMap.Tiles[i, j]);
                                break;
                            case TileContent.Cheese:
                                parsedMap.Cheese.Add(parsedMap.Tiles[i, j]);
                                break;
                        }
                    }
                }
            }

            // Fills the exits list
            FindExits(ref parsedMap);

            return parsedMap;
        }

        /// <summary>
        /// Fills the Exits list in the map 
        /// </summary>
        /// <param name="map"></param>
        public static void FindExits(ref Map map)
        {
            for (int i = 0; i < map.Height; ++i)
            {
                if (map.Tiles[i, map.Width - 1].Content == TileContent.Empty)
                {
                    map.Exits.Add(map.Tiles[i, map.Width - 1]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    sb.Append(Tiles[i, j].FormattedContent);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}

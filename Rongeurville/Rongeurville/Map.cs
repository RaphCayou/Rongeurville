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
        private int height;
        private int width;

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

            // find farthest # on a line to find map width
            int biggestIndex = 0;
            foreach (string line in lines)
            {
                int lastMapDelimiter = line.LastIndexOf('#');
                if (lastMapDelimiter > biggestIndex)
                {
                    biggestIndex = lastMapDelimiter;
                }
            }
            parsedMap.width = 1 + biggestIndex;

            // Find lowest line containing a # to find map height
            parsedMap.height = 1 + Array.FindLastIndex(lines, line => { return line.Contains('#'); });

            // Fills the list of tiles with valid instances 
            parsedMap.Tiles = new Tile[parsedMap.height, parsedMap.width];
            for (int i = 0; i < parsedMap.height; ++i)
            {
                for (int j = 0; j < parsedMap.width; ++j)
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
            for (int i = 0; i < map.height; ++i)
            {
                if (map.Tiles[i, map.width - 1].Content == TileContent.Empty)
                {
                    map.Exits.Add(map.Tiles[i, map.width - 1]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    sb.Append(Tiles[i, j].FormattedContent);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}

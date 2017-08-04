using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace Rongeurville
{
    /// <summary>
    /// Class for Rat specific actor actions
    /// </summary>
    public class Rat : Actor
    {
        private int timeSinceLastMeow;

        public Rat() : base()
        {
        }

        /// <summary>
        /// Constructor for Rat actor
        /// </summary>
        /// <param name="communicator">MPI Communicator</param>
        public Rat(Intracommunicator communicator) : base(communicator)
        {
            timeSinceLastMeow = 0;
        }

        /// <summary>
        /// Additional rat specific actions for alive loop
        /// </summary>
        /// <param name="distanceToObjective">Unused for rat (Necessary because of hierarchy)</param>
        protected override void MoveEvent(int distanceToObjective)
        {
            // Be less scared of nearby cat after move action
            if (timeSinceLastMeow > 0)
            {
                timeSinceLastMeow -= 1;
            }
        }

        /// <summary>
        /// Determine whether heard meow was close enough to be scared
        /// </summary>
        /// <param name="meowTile">Tile on map from where meow was heard</param>
        protected override void ListenMeow(Tile meowTile)
        {
            // Cat is close by
            if (Math.Abs(currentTile.X - meowTile.X) <= 7 || Math.Abs(currentTile.Y - meowTile.Y) <= 7)
            {
                timeSinceLastMeow = 5;
            }
        }

        /// <summary>
        /// Determine acceptable move tiles for a rat (8 adjacent tiles)
        /// </summary>
        /// <param name="center">Tile position to determine acceptable moves from</param>
        /// <returns>List of acceptable move tiles around rat</returns>
        public override List<Tile> GetNeighbors(Tile center)
        {
            List<Tile> neighbors = new List<Tile>();

            if (center.Y - 1 >= 0)
            {
                // UP
                if (CanGoToNeighbor(map.Tiles[center.Y - 1, center.X].Content))
                {
                    neighbors.Add(map.Tiles[center.Y - 1, center.X]);
                }
                // UP LEFT
                if (center.X - 1 >= 0 &&
                    CanGoToNeighbor(map.Tiles[center.Y - 1, center.X - 1].Content))
                {
                    neighbors.Add(map.Tiles[center.Y - 1, center.X - 1]);
                }
                // UP RIGHT
                if (center.X + 1 < map.Width &&
                    CanGoToNeighbor(map.Tiles[center.Y - 1, center.X + 1].Content))
                {
                    neighbors.Add(map.Tiles[center.Y - 1, center.X + 1]);
                }
            }

            if (center.Y + 1 < map.Height)
            {
                // DOWN
                if (CanGoToNeighbor(map.Tiles[center.Y + 1, center.X].Content))
                {
                    neighbors.Add(map.Tiles[center.Y + 1, center.X]);
                }
                // DOWN LEFT
                if (center.X - 1 >= 0 && CanGoToNeighbor(map.Tiles[center.Y + 1, center.X - 1].Content))
                {
                    neighbors.Add(map.Tiles[center.Y + 1, center.X - 1]);
                }
                // DOWN RIGHT
                if (center.X + 1 < map.Width && CanGoToNeighbor(map.Tiles[center.Y + 1, center.X + 1].Content))
                {
                    neighbors.Add(map.Tiles[center.Y + 1, center.X + 1]);
                }
            }

            // LEFT
            if (center.X - 1 >= 0 && CanGoToNeighbor(map.Tiles[center.Y, center.X - 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y, center.X - 1]);
            }

            // RIGHT
            if (center.X + 1 < map.Width && CanGoToNeighbor(map.Tiles[center.Y, center.X + 1].Content))
            {
                neighbors.Add(map.Tiles[center.Y, center.X + 1]);
            }

            return neighbors;
        }

        /// <summary>
        /// Determine whether the rat can move to the tile
        /// </summary>
        /// <param name="content">Target tile to check for valid move</param>
        /// <returns>Cat can move to the tile</returns>
        public override bool CanGoToNeighbor(TileContent content)
        {
            return content == TileContent.Cheese || content == TileContent.Empty;
        }

        /// <summary>
        /// Determine whether the tile is a current goal for the rat based on current scared status (cheese or exit)
        /// </summary>
        /// <param name="target">Target tile to check for current goal</param>
        /// <returns>Whether the tile is a current goal for the rat</returns>
        public override bool IsGoal(Tile target)
        {
            // TODO Remove this if, its a test
            if (map.Exits.Contains(target) != map.Exits.Any(e => e.Position.Equals(target.Position)))
            {
                Console.WriteLine("******************* error in goal function ********************");
            }
            return timeSinceLastMeow > 0 ? map.Exits.Contains(target) : TileContent.Cheese == target.Content;
        }

        /// <summary>
        /// Current tile content at our position
        /// </summary>
        /// <returns>We are a Rat</returns>
        public override TileContent GetTileContent()
        {
            return TileContent.Rat;
        }
    }
}
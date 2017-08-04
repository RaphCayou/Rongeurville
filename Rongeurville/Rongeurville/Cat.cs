using System.Collections.Generic;
using System.Linq;
using MPI;
using Rongeurville.Communication;

namespace Rongeurville
{
    public class Cat : Actor
    {
        private static readonly TileContent[] GO_THROUGH = { TileContent.Rat, TileContent.Empty };

        public Cat() : base()
        {
        }

        /// <summary>
        /// Constructor for Cat actor
        /// </summary>
        /// <param name="communicator">MPI Communicator</param>
        public Cat(Intracommunicator communicator) : base(communicator)
        {
        }

        /// <summary>
        /// Additional cat specific actions for alive loop
        /// </summary>
        /// <param name="distanceToObjective">Distance to the closest rat</param>
        protected override void MoveEvent(int distanceToObjective)
        {
            // MEOW
            if (distanceToObjective <= 10)
            {
                comm.Send(new MeowRequest { Rank = rank }, 0, 0);
            }
        }

        /// <summary>
        /// Meow was heard (unused for cat, necessary for hierarchy)
        /// </summary>
        /// <param name="meowTile">Tile on map from where meow was heard</param>
        protected override void ListenMeow(Tile meowTile)
        {
            //We do not react to Meow as a Cat.
        }

        protected override bool IHaveAGoalRemaning()
        {
            return map.Rats.Any();
        }

        /// <summary>
        /// Determine acceptable move tiles for a cat (4 adjacent tiles)
        /// </summary>
        /// <param name="center">Tile position to determine acceptable moves from</param>
        /// <returns>List of acceptable move tiles around rat</returns>
        public override List<Tile> GetNeighbors(Tile center)
        {
            List<Tile> neighbors = new List<Tile>();

            // UP
            if (center.Y - 1 >= 0 && CanGoToNeighbor(map.Tiles[center.Y - 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y - 1, center.X]);
            }

            // DOWN
            if (center.Y + 1 < map.Height && CanGoToNeighbor(map.Tiles[center.Y + 1, center.X].Content))
            {
                neighbors.Add(map.Tiles[center.Y + 1, center.X]);
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
        /// Determine whether the cat can move to the tile
        /// </summary>
        /// <param name="content">Target tile to check for valid move</param>
        /// <returns>Cat can move to the tile</returns>
        public override bool CanGoToNeighbor(TileContent content)
        {
            return content == TileContent.Rat || content == TileContent.Empty;
        }

        /// <summary>
        /// Determine whether the tile is a current goal for the cat (we look for rats)
        /// </summary>
        /// <param name="target">Target tile to check for current goal</param>
        public override bool IsGoal(Tile target)
        {
            return target.Content == TileContent.Rat;
        }

        /// <summary>
        /// Current tile content at our position
        /// </summary>
        /// <returns>We are a Cat</returns>
        public override TileContent GetTileContent()
        {
            return TileContent.Cat;
        }
    }
}

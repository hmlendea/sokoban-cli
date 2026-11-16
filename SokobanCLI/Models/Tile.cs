using System;

namespace SokobanCLI.Models
{
    /// <summary>
    /// Single map tile.
    /// </summary>
    public sealed class Tile
    {
        /// <summary>
        /// Gets or sets the tile type.
        /// </summary>
        /// <value>The tile type.</value>
        public TileType Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the character.
        /// </summary>
        /// <value>The character.</value>
        public char Character { get; set; }

        /// <summary>
        /// Gets or sets the colour.
        /// </summary>
        /// <value>The colour.</value>
        public ConsoleColor Colour { get; set; }

        /// <summary>
        /// Gets a value indicating whether the player can occupy this tile.
        /// </summary>
        /// <value><c>true</c> if the tile is walkable; otherwise, <c>false</c>.</value>
        public bool IsWalkable => Type is TileType.Empty or TileType.Goal;
    }
}

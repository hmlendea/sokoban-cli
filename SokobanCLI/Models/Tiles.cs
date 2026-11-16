using System;

namespace SokobanCLI.Models
{
    /// <summary>
    /// Tile factory.
    /// </summary>
    public static class Tiles
    {
        /// <summary>
        /// Gets the tile with the specified identifier.
        /// </summary>
        /// <param name="id">Tile identifier.</param>
        /// <returns>The tile.</returns>
        public static Tile ById(int id)
            => id switch
            {
                1 => new Tile
                {
                    Id = id,
                    Character = '█',
                    Colour = ConsoleColor.Gray
                },
                2 => new Tile
                {
                    Id = id,
                    Character = 'O',
                    Colour = ConsoleColor.DarkYellow
                },
                3 => new Tile
                {
                    Id = id,
                    Character = '+',
                    Colour = ConsoleColor.DarkRed
                },
                5 => new Tile
                {
                    Id = id,
                    Character = '@',
                    Colour = ConsoleColor.DarkGreen
                },
                _ => new Tile
                {
                    Id = id,
                    Character = ' ',
                    Colour = ConsoleColor.Black
                },
            };
    }
}

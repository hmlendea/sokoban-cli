using System;

namespace SokobanCLI.Models
{
    /// <summary>
    /// Tile factory.
    /// </summary>
    public static class Tiles
    {
        /// <summary>
        /// Gets the tile with the specified type.
        /// </summary>
        /// <param name="type">Tile type.</param>
        /// <returns>The tile.</returns>
        public static Tile OfType(TileType type) => CreateTile(type, (int)type);

        /// <summary>
        /// Gets the tile with the specified identifier.
        /// </summary>
        /// <param name="id">Tile identifier.</param>
        /// <returns>The tile.</returns>
        public static Tile ById(int id) => CreateTile(GetTypeForId(id), id);

        private static Tile CreateTile(TileType type, int id)
            => new()
            {
                Type = type,
                Id = id,
                Character = GetCharacter(type),
                Colour = GetColour(type)
            };

        private static TileType GetTypeForId(int id)
        {
            foreach (TileType type in Enum.GetValues<TileType>())
            {
                if ((int)type == id)
                {
                    return type;
                }
            }

            return TileType.Empty;
        }

        private static char GetCharacter(TileType type)
        {
            switch (type)
            {
                case TileType.Wall:
                    return '█';

                case TileType.Box:
                    return 'O';

                case TileType.Goal:
                    return '+';

                case TileType.BoxOnGoal:
                    return '@';

                default:
                    return ' ';
            }
        }

        private static ConsoleColor GetColour(TileType type)
        {
            switch (type)
            {
                case TileType.Wall:
                    return ConsoleColor.Gray;

                case TileType.Box:
                    return ConsoleColor.DarkYellow;

                case TileType.Goal:
                    return ConsoleColor.DarkRed;

                case TileType.BoxOnGoal:
                    return ConsoleColor.DarkGreen;

                default:
                    return ConsoleColor.Black;
            }
        }
    }
}

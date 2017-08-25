using System;

namespace SokobanCLI.Models
{
    public class Tile
    {
        public int Id { get; set; }

        public char Character { get; set; }

        public ConsoleColor Colour { get; set; }
    }

    public static class Tiles
    {
        public static Tile ById(int id)
        {
            switch (id)
            {
                default:
                    return new Tile
                    {
                        Id = id,
                        Character = ' ',
                        Colour = ConsoleColor.Black
                    };

                case 1:
                    return new Tile
                    {
                        Id = id,
                        Character = '█',
                        Colour = ConsoleColor.Gray
                    };

                case 2:
                    return new Tile
                    {
                        Id = id,
                        Character = 'O',
                        Colour = ConsoleColor.DarkYellow
                    };

                case 3:
                    return new Tile
                    {
                        Id = id,
                        Character = 'X',
                        Colour = ConsoleColor.DarkRed
                    };

                case 5:
                    return new Tile
                    {
                        Id = id,
                        Character = '@',
                        Colour = ConsoleColor.DarkGreen
                    };
            }
        }
    }
}

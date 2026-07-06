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
            return id switch
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
}

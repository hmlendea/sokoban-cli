using System;

namespace SokobanCLI
{
    public class Tile
    {
        public int ID { get; set; }

        public char Char { get; set; }

        public ConsoleColor Color { get; set; }

        public Tile(int id, char c, ConsoleColor color)
        {
            ID = id;
            Char = c;
            Color = color;
        }
    }

    public static class Tiles
    {
        public static Tile ByID(int id)
        {
            switch (id)
            {
                default:
                    return new Tile(id, ' ', ConsoleColor.Black);

                case 1:
                    return new Tile(id, '█', ConsoleColor.Gray);

                case 2:
                    return new Tile(id, 'O', ConsoleColor.DarkYellow);

                case 3:
                    return new Tile(id, 'X', ConsoleColor.DarkRed);

                case 5:
                    return new Tile(id, 'O', ConsoleColor.DarkGreen);
            }
        }
    }
}

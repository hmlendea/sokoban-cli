using System;

namespace SokobanCLI.Models
{
    public sealed class Tile
    {
        public int Id { get; set; }

        public char Character { get; set; }

        public ConsoleColor Colour { get; set; }
    }
}

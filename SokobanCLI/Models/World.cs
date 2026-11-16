using System;
using System.IO;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Models
{
    public sealed class World
    {
        private static readonly int DefaultSize = 14;

        public readonly Tile[,] Tiles;

        public Point2D PlayerPosition { get; set; }

        public Size2D Size { get; private set; }

        public int Moves { get; set; }

        public int Level { get; private set; }

        public World(int level)
        {
            string levelPath = Path.Combine("Levels", level + ".lvl");
            string[] rows = File.ReadAllLines(levelPath);

            ValidateRows(rows, levelPath);
            Size = new Size2D(DefaultSize, DefaultSize);
            Tiles = new Tile[Size.Width, Size.Height];
            Level = level;

            LoadTiles(rows);
        }

        public Tile GetTile(int x, int y) => Tiles[x, y];

        public bool Contains(Point2D position)
            => position.X >= 0 && position.X < Size.Width &&
                position.Y >= 0 && position.Y < Size.Height;

        private void LoadTiles(string[] rows)
        {
            for (int y = 0; y < Size.Height; y += 1)
            {
                for (int x = 0; x < Size.Width; x += 1)
                {
                    int id = rows[x][y] - '0';
                    Tiles[x, y] = Models.Tiles.ById(id);

                    if (id == 4)
                    {
                        PlayerPosition = new Point2D(x, y);
                        Tiles[x, y] = Models.Tiles.ById(0);
                    }
                    else if (id == 6)
                    {
                        PlayerPosition = new Point2D(x, y);
                        Tiles[x, y] = Models.Tiles.ById(3);
                    }
                }
            }
        }

        private static void ValidateRows(string[] rows, string levelPath)
        {
            if (rows.Length != DefaultSize)
            {
                throw new InvalidDataException($"Level '{levelPath}' must contain {DefaultSize} rows.");
            }

            foreach (string row in rows)
            {
                if (row.Length != DefaultSize)
                {
                    throw new InvalidDataException($"Level '{levelPath}' rows must contain {DefaultSize} tiles.");
                }
            }
        }
    }
}

using System.IO;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Models
{
    public class World
    {
        public readonly Tile[,] Tiles;

        public Point2D PlayerPosition { get; set; }

        public Size2D Size { get; set; }

        public int Moves { get; set; }

        public int Level { get; private set; }

        public World(int level)
        {
            string[] rows = File.ReadAllLines(@"Levels/" + level + ".lvl");
            Size = new Size2D(14, 14);
            Moves = 0;

            Tiles = new Tile[Size.Width, Size.Height];
            Level = level;

            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    Tiles[x, y] = Models.Tiles.ById((int)char.GetNumericValue(rows[x][y]));

                    if (Tiles[x, y].Id == 4)
                    {
                        PlayerPosition = new Point2D(x, y);
                        Tiles[x, y] = Models.Tiles.ById(0);
                    }
                    else if (Tiles[x, y].Id == 6)
                    {
                        PlayerPosition = new Point2D(x, y);
                        Tiles[x, y] = Models.Tiles.ById(3);
                    }
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y];
        }
    }
}


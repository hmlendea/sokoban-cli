using System;
using System.Drawing;
using System.IO;

namespace SokobanCLI.Models
{
    public class World
    {
        public readonly Tile[,] Tiles;

        public Point PlayerPosition { get; set; }

        public Size Size { get; set; }

        public int Moves { get; set; }

        public int Level { get; private set; }

        public World(int level)
        {
            string[] rows = File.ReadAllLines(@"Levels/" + level + ".lvl");
            Size = new Size(14, 14);
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
                        PlayerPosition = new Point(x, y);
                        Tiles[x, y] = Models.Tiles.ById(0);
                    }
                    else if (Tiles[x, y].Id == 6)
                    {
                        PlayerPosition = new Point(x, y);
                        Tiles[x, y] = Models.Tiles.ById(3);
                    }
                }
            }
        }
        
        public void DrawLevel(int destX, int destY)
        {
            //Console.Clear();
            Console.SetCursorPosition(destX, destY);

            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    if (x == PlayerPosition.X && y == PlayerPosition.Y)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                        if (Tiles[PlayerPosition.X, PlayerPosition.Y].Id == 3)
                        {
                            Console.Write('☻');
                        }
                        else
                        {
                            Console.Write('☺');
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = Tiles[x, y].Colour;

                        if (Tiles[x, y].Id == 1)
                        {
                            Console.Write(GetWallShape(x, y));
                        }
                        else
                        {
                            Console.Write(Tiles[x, y].Character);
                        }
                    }
                }
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y];
        }

        char GetWallShape(int x, int y)
        {
            char c = '?';

            bool n, w, s, e;

            if (x > 0)
            {
                n = (Tiles[x - 1, y].Id == 1);
            }
            else
            {
                n = false;
            }

            if (y > 0)
            {
                w = (Tiles[x, y - 1].Id == 1);
            }
            else
            {
                w = false;
            }

            if (y < Size.Height - 1)
            {
                e = (Tiles[x, y + 1].Id == 1);
            }
            else
            {
                e = false;
            }

            if (x < Size.Width - 1)
            {
                s = (Tiles[x + 1, y].Id == 1);
            }
            else
            {
                s = false;
            }

            if (n && w && s && e)
            {
                c = '╬';
            }
            else if (n && w && s)
            {
                c = '╣';
            }
            else if (n && w && e)
            {
                c = '╩';
            }
            else if (n && s && e)
            {
                c = '╠';
            }
            else if (n && w)
            {
                c = '╝';
            }
            else if (n && s)
            {
                c = '║';
            }
            else if (n && e)
            {
                c = '╚';
            }
            else if (n)
            {
                c = '║';
            }
            else if (w && s && e)
            {
                c = '╦';
            }
            else if (w && s)
            {
                c = '╗';
            }
            else if (w && e)
            {
                c = '═';
            }
            else if (w)
            {
                c = '═';
            }
            else if (s && e)
            {
                c = '╔';
            }
            else if (s)
            {
                c = '║';
            }
            else if (e)
            {
                c = '═';
            }

            return c;
        }
    }
}


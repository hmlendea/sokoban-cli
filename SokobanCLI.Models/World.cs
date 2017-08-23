using System;
using System.Drawing;
using System.IO;

namespace SokobanCLI.Models
{
    public class World
    {
        readonly Tile[,] tile;

        public Point PlayerPosition { get; set; }

        public Size Size { get; set; }

        public int Moves { get; private set; }

        public int Level { get; private set; }

        public bool Completed
        {
            get
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    for (int x = 0; x < Size.Width; x++)
                    {
                        if (tile[x, y].Id == 3)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public World(int level)
        {
            string[] rows = File.ReadAllLines(@"Levels/" + level + ".lvl");
            Size = new Size(14, 14);
            Moves = 0;

            tile = new Tile[Size.Width, Size.Height];
            Level = level;

            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    tile[x, y] = Tiles.ById((int)Char.GetNumericValue(rows[x][y]));

                    if (tile[x, y].Id == 4)
                    {
                        PlayerPosition = new Point(x, y);
                        tile[x, y] = Tiles.ById(0);
                    }
                    else if (tile[x, y].Id == 6)
                    {
                        PlayerPosition = new Point(x, y);
                        tile[x, y] = Tiles.ById(3);
                    }
                }
            }
        }

        public void MovePlayer(int x, int y)
        {
            int dirX = x - PlayerPosition.X;
            int dirY = y - PlayerPosition.Y;
            bool moved = false;

            if (x >= 0 && y >= 0 && x < 14 && y < 14)
            {
                switch (tile[x, y].Id)
                {
                    case 0:
                    case 3:
                        moved = true;
                        break;

                    case 2:
                        if (PlayerPosition.X >= 2 && PlayerPosition.X <= 12 &&
                            PlayerPosition.Y >= 2 && PlayerPosition.Y <= 12)
                        {
                            switch (tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2].Id)
                            {
                                case 0:
                                    tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2] = Tiles.ById(2);
                                    tile[PlayerPosition.X + dirX, PlayerPosition.Y + dirY] = Tiles.ById(0);
                                    moved = true;
                                    break;

                                case 3:
                                    tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2] = Tiles.ById(5);
                                    tile[PlayerPosition.X + dirX, PlayerPosition.Y + dirY] = Tiles.ById(0);
                                    moved = true;
                                    break;
                            }
                        }
                        break;
                    case 5:
                        if (PlayerPosition.X >= 2 && PlayerPosition.X <= 12 && PlayerPosition.Y >= 2 && PlayerPosition.Y <= 12)
                        {
                            switch (tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2].Id)
                            {
                                case 0:
                                    tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2] = Tiles.ById(2);
                                    tile[PlayerPosition.X + dirX, PlayerPosition.Y + dirY] = Tiles.ById(3);
                                    moved = true;
                                    break;

                                case 3:
                                    tile[PlayerPosition.X + dirX * 2, PlayerPosition.Y + dirY * 2] = Tiles.ById(5);
                                    tile[PlayerPosition.X + dirX, PlayerPosition.Y + dirY] = Tiles.ById(3);
                                    moved = true;
                                    break;
                            }
                        }
                        break;
                }
            }

            if (moved)
            {
                PlayerPosition = new Point(x, y);
                Moves += 1;
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

                        if (tile[PlayerPosition.X, PlayerPosition.Y].Id == 3)
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
                        Console.ForegroundColor = tile[x, y].Colour;

                        if (tile[x, y].Id == 1)
                        {
                            Console.Write(GetWallShape(x, y));
                        }
                        else
                        {
                            Console.Write(tile[x, y].Character);
                        }
                    }
                }
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        public Tile GetTile(int x, int y)
        {
            return tile[x, y];
        }

        char GetWallShape(int x, int y)
        {
            char c = '?';

            bool n, w, s, e;

            if (x > 0)
            {
                n = (tile[x - 1, y].Id == 1);
            }
            else
            {
                n = false;
            }

            if (y > 0)
            {
                w = (tile[x, y - 1].Id == 1);
            }
            else
            {
                w = false;
            }

            if (y < Size.Height - 1)
            {
                e = (tile[x, y + 1].Id == 1);
            }
            else
            {
                e = false;
            }

            if (x < Size.Width - 1)
            {
                s = (tile[x + 1, y].Id == 1);
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


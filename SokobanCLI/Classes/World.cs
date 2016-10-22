using System;
using System.IO;

namespace SokobanCLI
{
    public class World
    {
        readonly Tile[,] tile;

        public int PlayerPosX { get; set; }

        public int PlayerPosY { get; set; }

        public int SizeX { get; private set; }

        public int SizeY { get; private set; }

        public int Moves { get; private set; }

        public int Level { get; private set; }

        public bool Completed
        {
            get
            {
                for (int i = 0; i < SizeX; i++)
                    for (int j = 0; j < SizeY; j++)
                        if (tile[i, j].ID == 3)
                            return false;
                return true;
            }
        }

        public World(int level)
        {
            string[] rows = File.ReadAllLines(@"Levels/" + level + ".lvl");
            SizeX = 14;
            SizeY = 14;
            Moves = 0;

            tile = new Tile[SizeX, SizeY];
            Level = level;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    tile[x, y] = Tiles.ByID((int)Char.GetNumericValue(rows[x][y]));

                    if (tile[x, y].ID == 4)
                    {
                        PlayerPosX = x;
                        PlayerPosY = y;
                        tile[x, y] = Tiles.ByID(0);
                    }
                    else if (tile[x, y].ID == 6)
                    {
                        PlayerPosX = x;
                        PlayerPosY = y;
                        tile[x, y] = Tiles.ByID(3);
                    }
                }
            }
        }

        public void MovePlayer(int x, int y)
        {
            int dirX = x - PlayerPosX;
            int dirY = y - PlayerPosY;
            bool moved = false;

            if (x >= 0 && y >= 0 && x < 14 && y < 14)
                switch (tile[x, y].ID)
                {
                    case 0:
                    case 3:
                        moved = true;
                        break;
			
                    case 2:
                        if (PlayerPosX >= 2 && PlayerPosX <= 12 && PlayerPosY >= 2 && PlayerPosY <= 12)
                            switch (tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2].ID)
                            {
                                case 0:
                                    tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2] = Tiles.ByID(2);
                                    tile[PlayerPosX + dirX, PlayerPosY + dirY] = Tiles.ByID(0);
                                    moved = true;
                                    break;
				
                                case 3:
                                    tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2] = Tiles.ByID(5);
                                    tile[PlayerPosX + dirX, PlayerPosY + dirY] = Tiles.ByID(0);
                                    moved = true;
                                    break;
                            }
                        break;
                    case 5:
                        if (PlayerPosX >= 2 && PlayerPosX <= 12 && PlayerPosY >= 2 && PlayerPosY <= 12)
                            switch (tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2].ID)
                            {
                                case 0:
                                    tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2] = Tiles.ByID(2);
                                    tile[PlayerPosX + dirX, PlayerPosY + dirY] = Tiles.ByID(3);
                                    moved = true;
                                    break;
				
                                case 3:
                                    tile[PlayerPosX + dirX * 2, PlayerPosY + dirY * 2] = Tiles.ByID(5);
                                    tile[PlayerPosX + dirX, PlayerPosY + dirY] = Tiles.ByID(3);
                                    moved = true;
                                    break;
                            }
                        break;
                }

            if (moved)
            {
                PlayerPosX = x;
                PlayerPosY = y;
                Moves += 1;
            }
        }

        public void DrawLevel(int destX, int destY)
        {
            //Console.Clear();
            Console.SetCursorPosition(destX, destY);

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (x == PlayerPosX && y == PlayerPosY)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                        if (tile[PlayerPosX, PlayerPosY].ID == 3)
                            Console.Write('☻');
                        else
                            Console.Write('☺');
                    }
                    else
                    {
                        Console.ForegroundColor = tile[x, y].Color;

                        if (tile[x, y].ID == 1)
                            Console.Write(GetWallShape(x, y));
                        else
                            Console.Write(tile[x, y].Char);
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
                n = (tile[x - 1, y].ID == 1);
            else
                n = false;

            if (y > 0)
                w = (tile[x, y - 1].ID == 1);
            else
                w = false;

            if (y < SizeY - 1)
                e = (tile[x, y + 1].ID == 1);
            else
                e = false;

            if (x < SizeX - 1)
                s = (tile[x + 1, y].ID == 1);
            else
                s = false;

            if (n && w && s && e)
                c = '╬';
            else if (n && w && s)
                c = '╣';
            else if (n && w && e)
                c = '╩';
            else if (n && s && e)
                c = '╠';
            else if (n && w)
                c = '╝';
            else if (n && s)
                c = '║';
            else if (n && e)
                c = '╚';
            else if (n)
                c = '║';
            else if (w && s && e)
                c = '╦';
            else if (w && s)
                c = '╗';
            else if (w && e)
                c = '═';
            else if (w)
                c = '═';
            else if (s && e)
                c = '╔';
            else if (s)
                c = '║';
            else if (e)
                c = '═';

            return c;
        }
    }
}


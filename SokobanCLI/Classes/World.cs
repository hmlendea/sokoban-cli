using System;
using System.IO;

namespace SokobanCLI
{
    public class World
    {
        public int PlayerPosX
        {
            get { return playerX; }
            set { playerX = value; }
        }

        public int PlayerPosY
        {
            get { return playerY; }
            set { playerY = value; }
        }

        public bool Completed
        {
            get
            {
                for (int i = 0; i < sizeX; i++)
                    for (int j = 0; j < sizeY; j++)
                        if (tile[i, j].ID == 3)
                            return false;
                return true;
            }
        }

        public int SizeX { get { return sizeX; } }

        public int SizeY { get { return sizeY; } }

        public int Level { get { return level; } }

        public int Moves { get { return moves; } }

        Tile[,] tile;
        int playerX, playerY;
        int sizeX, sizeY;
        int level;
        int moves;

        public World(int level)
        {
            string[] rows = File.ReadAllLines(@"Levels/" + level + ".lvl");
            sizeX = 14;
            sizeY = 14;
            moves = 0;

            tile = new Tile[sizeX, sizeY];
            this.level = level;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    tile[x, y] = Tiles.ByID((int)Char.GetNumericValue(rows[x][y]));

                    if (tile[x, y].ID == 4)
                    {
                        playerX = x;
                        playerY = y;
                        tile[x, y] = Tiles.ByID(0);
                    }
                    else if (tile[x, y].ID == 6)
                    {
                        playerX = x;
                        playerY = y;
                        tile[x, y] = Tiles.ByID(3);
                    }
                }
            }
        }

        public void MovePlayer(int x, int y)
        {
            int dirX = x - playerX;
            int dirY = y - playerY;
            bool moved = false;

            if (x >= 0 && y >= 0 && x < 14 && y < 14)
                switch (tile[x, y].ID)
                {
                    case 0:
                    case 3:
                        moved = true;
                        break;
			
                    case 2:
                        if (playerX >= 2 && playerX <= 12 && playerY >= 2 && playerY <= 12)
                            switch (tile[playerX + dirX * 2, playerY + dirY * 2].ID)
                            {
                                case 0:
                                    tile[playerX + dirX * 2, playerY + dirY * 2] = Tiles.ByID(2);
                                    tile[playerX + dirX, playerY + dirY] = Tiles.ByID(0);
                                    moved = true;
                                    break;
				
                                case 3:
                                    tile[playerX + dirX * 2, playerY + dirY * 2] = Tiles.ByID(5);
                                    tile[playerX + dirX, playerY + dirY] = Tiles.ByID(0);
                                    moved = true;
                                    break;
                            }
                        break;
                    case 5:
                        if (playerX >= 2 && playerX <= 12 && playerY >= 2 && playerY <= 12)
                            switch (tile[playerX + dirX * 2, playerY + dirY * 2].ID)
                            {
                                case 0:
                                    tile[playerX + dirX * 2, playerY + dirY * 2] = Tiles.ByID(2);
                                    tile[playerX + dirX, playerY + dirY] = Tiles.ByID(3);
                                    moved = true;
                                    break;
				
                                case 3:
                                    tile[playerX + dirX * 2, playerY + dirY * 2] = Tiles.ByID(5);
                                    tile[playerX + dirX, playerY + dirY] = Tiles.ByID(3);
                                    moved = true;
                                    break;
                            }
                        break;
                }

            if (moved)
            {
                playerX = x;
                playerY = y;
                moves += 1;
            }
        }

        public void DrawLevel(int destX, int destY)
        {
            //Console.Clear();
            Console.SetCursorPosition(destX, destY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (x == playerX && y == playerY)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                        if (tile[playerX, playerY].ID == 3)
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

        private char GetWallShape(int x, int y)
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

            if (y < sizeY - 1)
                e = (tile[x, y + 1].ID == 1);
            else
                e = false;

            if (x < sizeX - 1)
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


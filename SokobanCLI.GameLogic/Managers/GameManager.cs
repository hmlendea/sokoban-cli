using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Models;

namespace SokobanCLI.GameLogic.Managers
{
    public class GameManager : IGameManager
    {
        public World World { get; private set; }

        public bool IsRunning { get; private set; }

        public GameManager()
        {

        }

        public void Start(int level)
        {
            IsRunning = true;
            World = new World(level);
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void MovePlayer(int deltaX, int deltaY)
        {
            int x = World.PlayerPosition.X + deltaX;
            int y = World.PlayerPosition.Y + deltaY;

            bool moved = false;

            if (x >= 0 && y >= 0 && x < 14 && y < 14)
            {
                switch (World.Tiles[x, y].Id)
                {
                    case 0:
                    case 3:
                        moved = true;
                        break;

                    case 2:
                        if (World.PlayerPosition.X >= 2 && World.PlayerPosition.X <= 12 &&
                            World.PlayerPosition.Y >= 2 && World.PlayerPosition.Y <= 12)
                        {
                            switch (World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2].Id)
                            {
                                case 0:
                                    World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2] = Tiles.ById(2);
                                    World.Tiles[World.PlayerPosition.X + deltaX, World.PlayerPosition.Y + deltaY] = Tiles.ById(0);
                                    moved = true;
                                    break;

                                case 3:
                                    World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2] = Tiles.ById(5);
                                    World.Tiles[World.PlayerPosition.X + deltaX, World.PlayerPosition.Y + deltaY] = Tiles.ById(0);
                                    moved = true;
                                    break;
                            }
                        }
                        break;
                    case 5:
                        if (World.PlayerPosition.X >= 2 && World.PlayerPosition.X <= 12 &&
                            World.PlayerPosition.Y >= 2 && World.PlayerPosition.Y <= 12)
                        {
                            switch (World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2].Id)
                            {
                                case 0:
                                    World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2] = Tiles.ById(2);
                                    World.Tiles[World.PlayerPosition.X + deltaX, World.PlayerPosition.Y + deltaY] = Tiles.ById(3);
                                    moved = true;
                                    break;

                                case 3:
                                    World.Tiles[World.PlayerPosition.X + deltaX * 2, World.PlayerPosition.Y + deltaY * 2] = Tiles.ById(5);
                                    World.Tiles[World.PlayerPosition.X + deltaX, World.PlayerPosition.Y + deltaY] = Tiles.ById(3);
                                    moved = true;
                                    break;
                            }
                        }
                        break;
                }
            }

            if (moved)
            {
                World.PlayerPosition = new Point2D(x, y);
                World.Moves += 1;
            }
        }

        public bool GetCompletion()
        {
            for (int y = 0; y < World.Size.Height; y++)
            {
                for (int x = 0; x < World.Size.Width; x++)
                {
                    if (World.Tiles[x, y].Id == 3)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

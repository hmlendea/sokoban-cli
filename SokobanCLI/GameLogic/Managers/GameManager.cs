using SokobanCLI.GameLogic.Events;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Models;

namespace SokobanCLI.GameLogic.Managers
{
    public sealed class GameManager : IGameManager
    {
        public World World { get; private set; }

        public bool IsRunning { get; private set; }

        public PlayerMovementEventHandler PlayerMoved { get; set; }

        public void Start(int level)
        {
            World = new World(level);
            IsRunning = true;
        }

        public void Stop() => IsRunning = false;

        public void MovePlayer(int deltaX, int deltaY)
        {
            if (!IsRunning || deltaX == 0 && deltaY == 0)
            {
                return;
            }

            Point2D playerPosition = World.PlayerPosition;
            Point2D targetPosition = playerPosition + new Point2D(deltaX, deltaY);

            if (!World.Contains(targetPosition))
            {
                return;
            }

            Tile targetTile = World.GetTile(targetPosition.X, targetPosition.Y);
            bool moved = targetTile.Id is 0 or 3;

            if (targetTile.Id is 2 or 5)
            {
                Point2D boxPosition = targetPosition + new Point2D(deltaX, deltaY);
                moved = TryMoveBox(targetPosition, boxPosition, targetTile.Id == 5);
            }

            if (!moved)
            {
                return;
            }

            World.PlayerPosition = targetPosition;
            World.Moves += 1;
            PlayerMoved?.Invoke(this, new PlayerMovementEventArgs(targetPosition));
        }

        public bool GetCompletion()
        {
            for (int y = 0; y < World.Size.Height; y += 1)
            {
                for (int x = 0; x < World.Size.Width; x += 1)
                {
                    if (World.GetTile(x, y).Id == 3)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool TryMoveBox(Point2D targetPosition, Point2D boxPosition, bool targetIsGoal)
        {
            if (!World.Contains(boxPosition))
            {
                return false;
            }

            int destinationId = World.GetTile(boxPosition.X, boxPosition.Y).Id;

            if (destinationId is not 0 and not 3)
            {
                return false;
            }

            int boxId = 2;

            if (destinationId == 3)
            {
                boxId = 5;
            }

            int targetId = 0;

            if (targetIsGoal)
            {
                targetId = 3;
            }

            World.Tiles[boxPosition.X, boxPosition.Y] = Tiles.ById(boxId);
            World.Tiles[targetPosition.X, targetPosition.Y] = Tiles.ById(targetId);
            return true;
        }
    }
}

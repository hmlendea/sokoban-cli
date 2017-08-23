using SokobanCLI.Models;

namespace SokobanCLI.GameLogic.Managers.Interfaces
{
    public interface IGameManager
    {
        World World { get; }

        bool IsRunning { get; }

        void Start(int level);

        void Stop();

        void MovePlayer(int deltaX, int deltaY);

        bool GetCompletion();
    }
}

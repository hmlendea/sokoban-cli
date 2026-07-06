using SokobanCLI.GameLogic.Events;

namespace SokobanCLI.UnitTests.GameLogic.Managers
{
    public interface IPlayerMovedSpy
    {
        void OnPlayerMoved(object sender, PlayerMovementEventArgs e);
    }
}

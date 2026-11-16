using SokobanCLI.Input.Events;

namespace SokobanCLI.Input
{
    /// <summary>
    /// Input manager contract.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>Occurs when a keyboard key was pressed.</summary>
        event ConsoleKeyEventHandler KeyboardKeyPressed;

        /// <summary>Updates the content.</summary>
        void Update();
    }
}

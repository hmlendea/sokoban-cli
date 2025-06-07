using System;

namespace SokobanCLI.Input.Events
{
    /// <summary>
    /// Keyboard key event handler.
    /// </summary>
    public delegate void ConsoleKeyEventHandler(object sender, ConsoleKeyEventArgs e);

    /// <summary>
    /// Keyboard key event arguments.
    /// </summary>
    /// <param name="key">Key.</param>
    public class ConsoleKeyEventArgs(ConsoleKey key)
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public ConsoleKey Key { get; private set; } = key;
    }
}

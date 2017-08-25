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
    public class ConsoleKeyEventArgs
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public ConsoleKey Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleKeyEventArgs"/> class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="keyState">Key state.</param>
        public ConsoleKeyEventArgs(ConsoleKey key)
        {
            Key = key;
        }
    }
}

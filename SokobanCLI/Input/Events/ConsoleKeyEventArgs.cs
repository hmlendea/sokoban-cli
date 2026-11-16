using System;

namespace SokobanCLI.Input.Events
{
    /// <summary>
    /// Keyboard key event arguments.
    /// </summary>
    /// <param name="key">Key.</param>
    public sealed class ConsoleKeyEventArgs(ConsoleKey key)
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public ConsoleKey Key { get; private set; } = key;
    }
}

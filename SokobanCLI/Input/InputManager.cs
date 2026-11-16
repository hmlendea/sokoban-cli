using System;
using System.Threading;

using SokobanCLI.Input.Events;

namespace SokobanCLI.Input
{
    /// <summary>
    /// Input manager.
    /// </summary>
    public sealed class InputManager : IInputManager
    {
        private static readonly Lock SyncRoot = new();

        private static volatile InputManager instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static InputManager Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (SyncRoot)
                    {
                        instance ??= new InputManager();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Occurs when a keyboard key was pressed.
        /// </summary>
        public event ConsoleKeyEventHandler KeyboardKeyPressed;

        /// <summary>
        /// Updates the content.
        /// </summary>
        public void Update()
        {
            if (!Console.KeyAvailable)
            {
                return;
            }

            ExposePressedKey();
        }

        private void ExposePressedKey()
        {
            ConsoleKey key = Console.ReadKey(true).Key;

            KeyboardKeyPressed?.Invoke(this, new ConsoleKeyEventArgs(key));
        }
    }
}

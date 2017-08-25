using System;

using SokobanCLI.Input.Events;

namespace SokobanCLI.Input
{
    /// <summary>
    /// Input manager.
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Occurs when a keyboard key was pressed.
        /// </summary>
        public event ConsoleKeyEventHandler KeyboardKeyPressed;
        
        static volatile InputManager instance;
        static object syncRoot = new object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new InputManager();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        public void Update()
        {
            if (!Console.KeyAvailable)
            {
                return;
            }

            CheckKeyboardKeyPressed();
        }

        void CheckKeyboardKeyPressed()
        {
            ConsoleKey key = Console.ReadKey().Key;

            KeyboardKeyPressed?.Invoke(this, new ConsoleKeyEventArgs(key));
        }
    }
}

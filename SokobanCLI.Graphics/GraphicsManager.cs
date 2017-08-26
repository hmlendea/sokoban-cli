using System;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    /// <summary>
    /// Graphics Manager.
    /// </summary>
    public class GraphicsManager
    {
        static volatile GraphicsManager instance;
        static object syncRoot = new object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static GraphicsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new GraphicsManager();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the sprite batch.
        /// </summary>
        /// <value>The sprite batch.</value>
        public AsciiSpriteBatch SpriteBatch { get; set; }

        /// <summary>
        /// Gets the resolution.
        /// </summary>
        /// <value>The resolution.</value>
        public Size2D Resolution => new Size2D(Console.WindowWidth, Console.WindowHeight);
    }
}

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

        AsciiSpriteBatch drawnData;

        public void LoadContent()
        {
            drawnData = new AsciiSpriteBatch();
            drawnData.LoadContent();
        }

        public void UnloadContent()
        {
            SpriteBatch.UnloadContent();
            drawnData.UnloadContent();
        }

        public void Update(float gameTime)
        {
        }

        public void Draw()
        {
            for (int y = 0; y < SpriteBatch.Size.Height; y++)
            {
                for (int x = 0; x < SpriteBatch.Size.Width; x++)
                {
                    if (SpriteBatch.CharArray[x, y] != ' ')
                    {
                        char a = SpriteBatch.CharArray[x, y];
                        char b = drawnData.CharArray[x, y];

                        var z = 1;
                    }

                    if (drawnData.CharArray[x, y] == SpriteBatch.CharArray[x, y] &&
                        drawnData.BackgroundColourArray[x, y] == SpriteBatch.BackgroundColourArray[x, y] &&
                        drawnData.ForegroundColourArray[x, y] == SpriteBatch.ForegroundColourArray[x, y])
                    {
                        continue;
                    }

                    Console.SetCursorPosition(x, y);
                    Console.BackgroundColor = SpriteBatch.BackgroundColourArray[x, y];
                    Console.ForegroundColor = SpriteBatch.ForegroundColourArray[x, y];

                    Console.Write(SpriteBatch.CharArray[x, y]);

                    drawnData.CharArray[x, y] = SpriteBatch.CharArray[x, y];
                    drawnData.BackgroundColourArray[x, y] = SpriteBatch.BackgroundColourArray[x, y];
                    drawnData.ForegroundColourArray[x, y] = SpriteBatch.ForegroundColourArray[x, y];

                }
            }

            Console.SetCursorPosition(0, 0);
        }

        public void Clear(ConsoleColor colour)
        {
            Console.BackgroundColor = colour;
            Console.ForegroundColor = colour;
            Console.Clear();

            for (int y = 0; y < drawnData.Size.Height; y++)
            {
                for (int x = 0; x < drawnData.Size.Width; x++)
                {
                    drawnData.CharArray[x, y] = ' ';
                    drawnData.BackgroundColourArray[x, y] = colour;
                    drawnData.ForegroundColourArray[x, y] = colour;

                    SpriteBatch.CharArray[x, y] = ' ';
                    SpriteBatch.BackgroundColourArray[x, y] = ConsoleColor.Black;
                    SpriteBatch.ForegroundColourArray[x, y] = ConsoleColor.White;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using SokobanCLI.Graphics.Enumerations;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    /// <summary>
    /// Sprite batch.
    /// </summary>
    public class AsciiSpriteBatch
    {
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        public Size2D Size { get; private set; }

        /// <summary>
        /// Gets the char array.
        /// </summary>
        /// <value>The char array.</value>
        public char[,] CharArray { get; private set; }

        /// <summary>
        /// Gets the background colour array.
        /// </summary>
        /// <value>The background colour array.</value>
        public ConsoleColor[,] BackgroundColourArray { get; private set; }

        /// <summary>
        /// Gets the foreground colour array.
        /// </summary>
        /// <value>The foreground colour array.</value>
        public ConsoleColor[,] ForegroundColourArray { get; private set; }

        /// <summary>
        /// DEBUG: Gets a preview of the screen.
        /// </summary>
        /// <value>The screen preview.</value>
        public string ScreenPreview
        {
            get
            {
                string screen = string.Empty;

                for (int y = 0; y < Size.Height; y++)
                {
                    for (int x = 0; x < Size.Width; x++)
                    {
                        screen += CharArray[x, y];
                    }

                    screen += Environment.NewLine;
                }

                return screen;
            }
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public void LoadContent()
        {
            Size = GraphicsManager.Instance.Resolution;

            CharArray = new char[Size.Width, Size.Height];
            BackgroundColourArray = new ConsoleColor[Size.Width, Size.Height];
            ForegroundColourArray = new ConsoleColor[Size.Width, Size.Height];

            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    CharArray[x, y] = ' ';
                    BackgroundColourArray[x, y] = ConsoleColor.Black;
                    ForegroundColourArray[x, y] = ConsoleColor.White;
                }
            }
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public virtual void UnloadContent()
        {

        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        public virtual void Update()
        {
            if (GraphicsManager.Instance.Resolution == Size)
            {
                return;
            }

            LoadContent();
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        public virtual void Draw(AsciiSprite sprite)
        {
            List<string> lines = sprite.Text.Split('\n').ToList();
            string[,] spriteData = new string[sprite.Size.Width, sprite.Size.Height];

            int firstRow = 0;

            switch (sprite.HorizontalAlignment)
            {
                case HorizontalAlignment.Centre:
                    firstRow = (sprite.Size.Height - lines.Count) / 2;
                    break;

                case HorizontalAlignment.Bottom:
                    firstRow = sprite.Size.Height - lines.Count;
                    break;
            }

            int lastRow = firstRow + lines.Count;

            for (int y = firstRow; y < lastRow; y++)
            {
                int row = y - firstRow;
                int firstCol = 0;

                switch (sprite.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        firstCol = (sprite.Size.Width - lines[row].Length) / 2;
                        break;

                    case VerticalAlignment.Right:
                        firstCol = sprite.Size.Width - lines[row].Length;
                        break;
                }

                int lastCol = firstCol + lines[row].Length;

                for (int x = firstCol; x < lastCol; x++)
                {
                    int col = x - firstCol;
                    int destX = sprite.Location.X + x;
                    int destY = sprite.Location.Y + y;

                    CharArray[destX, destY] = lines[row][col];
                    BackgroundColourArray[destX, destY] = sprite.BackgroundColour;
                    ForegroundColourArray[destX, destY] = sprite.ForegroundColour;
                }
            }
        }
    }
}

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
        public Size2D Size { get; private set; }

        public char[,] CharArray { get; private set; }

        public ConsoleColor[,] BackgroundColourArray { get; private set; }

        public ConsoleColor[,] ForegroundColourArray { get; private set; }

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
                }
            }
        }
    }
}

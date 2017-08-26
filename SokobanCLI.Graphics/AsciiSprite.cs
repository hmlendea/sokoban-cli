using System;

using SokobanCLI.Graphics.Enumerations;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    public class AsciiSprite
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public Point2D Location { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public Size2D Size { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment.
        /// </summary>
        /// <value>The horizontal alignment.</value>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        /// <value>The vertical alignment.</value>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the background colour.
        /// </summary>
        /// <value>The background colour.</value>
        public ConsoleColor BackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the foreground colour.
        /// </summary>
        /// <value>The foreground colour.</value>
        public ConsoleColor ForegroundColour { get; set; }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public void LoadContent()
        {
            Text = string.Empty;
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public void UnloadContent()
        {
            Text = string.Empty;
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public void Update(float gameTime)
        {
            /*
            List<string> lines = Text.Split('\n').ToList();
            int maxWidth = 0;

            foreach(string line in lines)
            {
                if (line.Length > maxWidth)
                {
                    maxWidth = line.Length;
                }
            }

            Size = new Size2D(maxWidth, lines.Count);
            */
        }

        /// <summary>
        /// Draw content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public void Draw(AsciiSpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this);
        }
    }
}

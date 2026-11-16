using System;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    /// <summary>
    /// Sprite contract.
    /// </summary>
    public interface IAsciiSprite
    {
        /// <summary>Gets or sets the location.</summary>
        Point2D Location { get; set; }

        /// <summary>Gets or sets the size.</summary>
        Size2D Size { get; set; }

        /// <summary>Gets or sets the text.</summary>
        string Text { get; set; }

        /// <summary>Gets or sets the horizontal alignment.</summary>
        HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>Gets or sets the vertical alignment.</summary>
        VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>Gets or sets the background colour.</summary>
        ConsoleColor BackgroundColour { get; set; }

        /// <summary>Gets or sets the foreground colour.</summary>
        ConsoleColor ForegroundColour { get; set; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Unloads the content.</summary>
        void UnloadContent();

        /// <summary>Updates the content.</summary>
        /// <param name="gameTime">Game time.</param>
        void Update(float gameTime);

        /// <summary>Draws the content.</summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        void Draw(AsciiSpriteBatch spriteBatch);
    }
}

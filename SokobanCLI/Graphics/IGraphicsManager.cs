using System;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    /// <summary>
    /// Graphics manager contract.
    /// </summary>
    public interface IGraphicsManager
    {
        /// <summary>Gets or sets the sprite batch.</summary>
        AsciiSpriteBatch SpriteBatch { get; set; }

        /// <summary>Gets the resolution.</summary>
        Size2D Resolution { get; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Unloads the content.</summary>
        void UnloadContent();

        /// <summary>Updates the content.</summary>
        /// <param name="gameTime">Game time.</param>
        void Update(float gameTime);

        /// <summary>Draws the content.</summary>
        void Draw();

        /// <summary>Clears the console screen.</summary>
        /// <param name="colour">Fill colour.</param>
        void Clear(ConsoleColor colour);
    }
}

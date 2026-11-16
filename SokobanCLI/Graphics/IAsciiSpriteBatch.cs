using System;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    /// <summary>
    /// Sprite batch contract.
    /// </summary>
    public interface IAsciiSpriteBatch
    {
        /// <summary>Gets the size.</summary>
        Size2D Size { get; }

        /// <summary>Gets the character array.</summary>
        char[,] CharArray { get; }

        /// <summary>Gets the background colour array.</summary>
        ConsoleColor[,] BackgroundColourArray { get; }

        /// <summary>Gets the foreground colour array.</summary>
        ConsoleColor[,] ForegroundColourArray { get; }

        /// <summary>Gets a preview of the screen.</summary>
        string ScreenPreview { get; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Unloads the content.</summary>
        void UnloadContent();

        /// <summary>Updates the content.</summary>
        void Update();

        /// <summary>Draws the content.</summary>
        /// <param name="sprite">Sprite.</param>
        void Draw(AsciiSprite sprite);
    }
}

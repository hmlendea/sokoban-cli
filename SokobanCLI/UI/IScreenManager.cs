using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UI
{
    /// <summary>
    /// Screen manager contract.
    /// </summary>
    public interface IScreenManager
    {
        /// <summary>Gets the size.</summary>
        Size2D Size { get; }

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

        /// <summary>Changes the screen.</summary>
        /// <param name="screenName">Screen name.</param>
        void ChangeScreens(string screenName);

        /// <summary>Changes the screen.</summary>
        /// <param name="screenName">Screen name.</param>
        /// <param name="screenArgs">Screen arguments.</param>
        void ChangeScreens(string screenName, string[] screenArgs);
    }
}

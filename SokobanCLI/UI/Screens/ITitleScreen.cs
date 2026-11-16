using SokobanCLI.Graphics;

namespace SokobanCLI.UI.Screens
{
    /// <summary>
    /// Title screen contract.
    /// </summary>
    public interface ITitleScreen
    {
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

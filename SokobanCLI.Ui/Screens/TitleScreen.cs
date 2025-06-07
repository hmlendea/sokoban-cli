using SokobanCLI.Graphics;

namespace SokobanCLI.Ui.Screens
{
    /// <summary>
    /// Title screen.
    /// </summary>
    public class TitleScreen : MenuScreen
    {
        /// <summary>
        /// Loads the content.
        /// </summary>
        public override void LoadContent() => base.LoadContent();

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public override void UnloadContent() => base.UnloadContent();

        /// <summary>
        /// Update the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(float gameTime) => base.Update(gameTime);

        /// <summary>
        /// Draw the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public override void Draw(AsciiSpriteBatch spriteBatch) => base.Draw(spriteBatch);
    }
}

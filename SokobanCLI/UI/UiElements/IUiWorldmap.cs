using SokobanCLI.GameLogic.Managers;
using SokobanCLI.Graphics;

namespace SokobanCLI.UI.UiElements
{
    /// <summary>
    /// World map UI element contract.
    /// </summary>
    public interface IUiWorldmap
    {
        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Draws the content.</summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        void Draw(AsciiSpriteBatch spriteBatch);

        /// <summary>Associates the game manager.</summary>
        /// <param name="game">Game manager.</param>
        void AssociateGameManager(IGameManager game);
    }
}

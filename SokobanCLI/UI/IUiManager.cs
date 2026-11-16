using System.Collections.Generic;

using SokobanCLI.Graphics;
using SokobanCLI.UI.UiElements;

namespace SokobanCLI.UI
{
    /// <summary>
    /// User-interface manager contract.
    /// </summary>
    public interface IUiManager
    {
        /// <summary>Gets or sets the GUI elements.</summary>
        List<UiElement> UiElements { get; set; }

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

        /// <summary>Focuses the input on the element with the specified identifier.</summary>
        /// <param name="id">Element identifier.</param>
        void FocusElement(string id);

        /// <summary>Focuses the input on the specified element.</summary>
        /// <param name="element">Element.</param>
        void FocusElement(UiElement element);
    }
}

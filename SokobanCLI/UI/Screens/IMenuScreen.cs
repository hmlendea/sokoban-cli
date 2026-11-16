using System.Collections.Generic;

using SokobanCLI.Graphics;
using SokobanCLI.UI.UiElements;

namespace SokobanCLI.UI.Screens
{
    /// <summary>
    /// Menu screen contract.
    /// </summary>
    public interface IMenuScreen
    {
        /// <summary>Gets or sets the axis.</summary>
        string Axis { get; set; }

        /// <summary>Gets or sets the spacing.</summary>
        int Spacing { get; set; }

        /// <summary>Gets or sets the links.</summary>
        List<UiMenuLink> Links { get; set; }

        /// <summary>Gets or sets the actions.</summary>
        List<UiMenuAction> Actions { get; set; }

        /// <summary>Gets all the items.</summary>
        List<UiMenuItem> Items { get; }

        /// <summary>Gets the item number.</summary>
        int ItemNumber { get; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Updates the content.</summary>
        /// <param name="gameTime">Game time.</param>
        void Update(float gameTime);

        /// <summary>Draws the content.</summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        void Draw(AsciiSpriteBatch spriteBatch);
    }
}

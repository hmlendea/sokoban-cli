using System;
using System.Collections.Generic;

using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UI.UiElements
{
    /// <summary>
    /// GUI element contract.
    /// </summary>
    public interface IUiElement
    {
        /// <summary>Gets or sets the identifier.</summary>
        string Id { get; set; }

        /// <summary>Gets or sets the location.</summary>
        Point2D Location { get; set; }

        /// <summary>Gets or sets the size.</summary>
        Size2D Size { get; set; }

        /// <summary>Gets the screen area covered by this element.</summary>
        Rectangle2D ClientRectangle { get; }

        /// <summary>Gets or sets the opacity.</summary>
        float Opacity { get; set; }

        /// <summary>Gets or sets the background colour.</summary>
        ConsoleColor BackgroundColour { get; set; }

        /// <summary>Gets or sets the foreground colour.</summary>
        ConsoleColor ForegroundColour { get; set; }

        /// <summary>Gets or sets a value indicating whether this element is enabled.</summary>
        bool IsEnabled { get; set; }

        /// <summary>Gets or sets a value indicating whether this element is visible.</summary>
        bool IsVisible { get; set; }

        /// <summary>Gets or sets a value indicating whether this element is hovered.</summary>
        bool IsHovered { get; set; }

        /// <summary>Gets or sets a value indicating whether this element has input focus.</summary>
        bool HasInputFocus { get; set; }

        /// <summary>Gets or sets the children GUI elements.</summary>
        List<UiElement> Children { get; }

        /// <summary>Gets a value indicating whether this element is disposed.</summary>
        bool IsDisposed { get; }

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

        /// <summary>Shows this element.</summary>
        void Show();

        /// <summary>Hides this element.</summary>
        void Hide();
    }
}

using SokobanCLI.Graphics;

namespace SokobanCLI.UI.UiElements
{
    /// <summary>
    /// Text UI element contract.
    /// </summary>
    public interface IUiText
    {
        /// <summary>Gets or sets the text.</summary>
        string Text { get; set; }

        /// <summary>Gets or sets the vertical alignment of the text.</summary>
        VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>Gets or sets the horizontal alignment of the text.</summary>
        HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>Gets or sets the margins.</summary>
        int Margins { get; set; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Draws the content.</summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        void Draw(AsciiSpriteBatch spriteBatch);
    }
}

using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Enumerations;

namespace SokobanCLI.Ui.UiElements
{
    /// <summary>
    /// Text UI element.
    /// </summary>
    public class UiText : UiElement
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the text.
        /// </summary>
        /// <value>The vertical alignment.</value>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the text.
        /// </summary>
        /// <value>The horizontal alignment.</value>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the margins.
        /// </summary>
        /// <value>The margins.</value>
        public int Margins { get; set; }

        AsciiSprite text;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiText"/> class.
        /// </summary>
        public UiText()
        {
            Text = string.Empty;

            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Centre;
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public override void LoadContent()
        {
            text = new AsciiSprite();

            base.LoadContent();
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public override void Draw(AsciiSpriteBatch spriteBatch)
            => text.Draw(spriteBatch);

        /// <summary>
        /// Sets the children properties.
        /// </summary>
        protected override void SetChildrenProperties()
        {
            base.SetChildrenProperties();

            text.Location = Location;
            text.Size = Size;
            text.Text = Text;
            text.HorizontalAlignment = HorizontalAlignment;
            text.VerticalAlignment = VerticalAlignment;
            text.BackgroundColour = BackgroundColour;
            text.ForegroundColour = ForegroundColour;
        }
    }
}

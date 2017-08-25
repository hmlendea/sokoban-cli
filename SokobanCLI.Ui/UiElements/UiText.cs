using System;

using SokobanCLI.Graphics.Enumerations;
using SokobanCLI.Graphics.Geometry;

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
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GuiText"/> class.
        /// </summary>
        public UiText()
        {
            ForegroundColour = ConsoleColor.White;
            BackgroundColour = ConsoleColor.Black;

            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Centre;
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        public override void Draw()
        {
            int cursorX = Location.X;
            int cursorY = Location.Y;

            switch(VerticalAlignment)
            {
                case VerticalAlignment.Left:
                    cursorX = 0;
                    break;

                case VerticalAlignment.Center:
                    cursorX = Location.X + (Size.Width - Text.Length) / 2;
                    break;

                case VerticalAlignment.Right:
                    cursorX = Location.X + Size.Width - Text.Length;
                    break;
            }

            Console.SetCursorPosition(cursorX, cursorY);
            Console.BackgroundColor = BackgroundColour;
            Console.ForegroundColor = ForegroundColour;

            // TODO: Align it based on the properties
            // TOOD: Take the Margins property into consideration
            Console.Write(Text);
        }
    }
}

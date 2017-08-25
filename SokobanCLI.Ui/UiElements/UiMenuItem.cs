using System;

using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Input.Events;

namespace SokobanCLI.Ui.UiElements
{
    /// <summary>
    /// Menu item GUI element.
    /// </summary>
    public class UiMenuItem : UiElement
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the highlight colour.
        /// </summary>
        /// <value>The highlight colour.</value>
        public ConsoleColor HighlightColour { get; set; }

        // TODO: Maybe implement my own handler and args
        /// <summary>
        /// Occurs when activated.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// The text UI element.
        /// </summary>
        protected UiText text;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiMenuItem"/> class.
        /// </summary>
        public UiMenuItem()
        {
            ForegroundColour = ConsoleColor.White;
            HighlightColour = ConsoleColor.Yellow;

            Size = new Size2D(48, 1);
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public override void LoadContent()
        {
            text = new UiText();
            
            Children.Add(text);

            base.LoadContent();
        }

        protected override void SetChildrenProperties()
        {
            base.SetChildrenProperties();

            text.Text = Text;
            text.Location = Location;
            text.Size = Size;

            if (InputFocus)
            {
                text.ForegroundColour = HighlightColour;
            }
            else
            {
                text.ForegroundColour = ForegroundColour;
            }
        }

        protected override void OnKeyPressed(object sender, ConsoleKeyEventArgs e)
        {
            base.OnKeyPressed(sender, e);

            if (e.Key == ConsoleKey.Enter || e.Key == ConsoleKey.E)
            {
                OnActivated(this, null);
            }
        }

        /// <summary>
        /// Fired by the Activated event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnActivated(object sender, EventArgs e)
        {
            Activated?.Invoke(this, null);
        }
    }
}

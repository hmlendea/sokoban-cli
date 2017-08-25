using System;

namespace SokobanCLI.Ui.UiElements
{
    /// <summary>
    /// Menu action UI element.
    /// </summary>
    public class UiMenuAction : UiMenuItem
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public string ActionId { get; set; }

        /// <summary>
        /// Fired by the Activated event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected override void OnActivated(object sender, EventArgs e)
        {
            base.OnActivated(sender, e);

            switch (ActionId)
            {
                case "Exit":
                    // TODO: Close the game somehow...
                    break;
            }
        }
    }
}

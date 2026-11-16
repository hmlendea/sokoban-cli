using System;

namespace SokobanCLI.UI.UiElements
{
    /// <summary>
    /// Menu item GUI element contract.
    /// </summary>
    public interface IUiMenuItem
    {
        /// <summary>Gets or sets the text.</summary>
        string Text { get; set; }

        /// <summary>Gets or sets the highlight colour.</summary>
        ConsoleColor HighlightColour { get; set; }

        /// <summary>Loads the content.</summary>
        void LoadContent();
    }
}

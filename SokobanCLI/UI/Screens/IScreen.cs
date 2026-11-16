using System;

using SokobanCLI.Graphics;
using SokobanCLI.UI.UiElements;

namespace SokobanCLI.UI.Screens
{
    /// <summary>
    /// Screen contract.
    /// </summary>
    public interface IScreen
    {
        /// <summary>Gets or sets the identifier.</summary>
        string Id { get; set; }

        /// <summary>Gets or sets the XML path.</summary>
        string XmlPath { get; set; }

        /// <summary>Gets or sets the type.</summary>
        Type Type { get; set; }

        /// <summary>Gets or sets the background colour.</summary>
        ConsoleColor BackgroundColour { get; set; }

        /// <summary>Gets or sets the foreground colour.</summary>
        ConsoleColor ForegroundColour { get; set; }

        /// <summary>Gets or sets the highlight colour.</summary>
        ConsoleColor HighlightColour { get; set; }

        /// <summary>Gets or sets the screen arguments.</summary>
        string[] ScreenArgs { get; set; }

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
    }
}

using SokobanCLI.UI.UiElements;

namespace SokobanCLI.UI.Screens
{
    /// <summary>
    /// Game screen contract.
    /// </summary>
    public interface IGameScreen
    {
        /// <summary>Gets or sets the worldmap.</summary>
        UiWorldmap Worldmap { get; set; }

        /// <summary>Gets or sets the game details label.</summary>
        UiText GameDetailsLabel { get; set; }

        /// <summary>Gets or sets the game details.</summary>
        UiText GameDetails { get; set; }

        /// <summary>Gets or sets the game controls label.</summary>
        UiText GameControlsLabel { get; set; }

        /// <summary>Gets or sets the game controls.</summary>
        UiText GameControls { get; set; }

        /// <summary>Loads the content.</summary>
        void LoadContent();

        /// <summary>Updates the content.</summary>
        /// <param name="gameTime">Game time.</param>
        void Update(float gameTime);
    }
}

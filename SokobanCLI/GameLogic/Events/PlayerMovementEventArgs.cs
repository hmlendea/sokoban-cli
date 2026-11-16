using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.GameLogic.Events
{
    /// <summary>
    /// Player movement event arguments.
    /// </summary>
    /// <param name="playerLocation">Key.</param>
    public sealed class PlayerMovementEventArgs(Point2D playerLocation)
    {
        /// <summary>
        /// Gets the player location.
        /// </summary>
        /// <value>The player location.</value>
        public Point2D PlayerLocation { get; private set; } = playerLocation;
    }
}

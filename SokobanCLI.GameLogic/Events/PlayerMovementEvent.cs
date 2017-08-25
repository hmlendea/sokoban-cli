using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.GameLogic.Events
{
    /// <summary>
    /// Player movement event handler.
    /// </summary>
    public delegate void PlayerMovementEventHandler(object sender, PlayerMovementEventArgs e);

    /// <summary>
    /// Player movement event arguments.
    /// </summary>
    public class PlayerMovementEventArgs
    {
        /// <summary>
        /// Gets the player location.
        /// </summary>
        /// <value>The player location.</value>
        public Point2D PlayerLocation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerMovementEventArgs"/> class.
        /// </summary>
        /// <param name="playerLocation">Key.</param>
        public PlayerMovementEventArgs(Point2D playerLocation)
        {
            PlayerLocation = playerLocation;
        }
    }
}

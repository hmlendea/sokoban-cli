using System;

using SokobanCLI.GameLogic.Events;
using SokobanCLI.GameLogic.Managers;
using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Input.Events;
using SokobanCLI.Ui.UiElements;

namespace SokobanCLI.Ui.Screens
{
    /// <summary>
    /// Game screen.
    /// </summary>
    public class GameScreen : Screen
    {
        /// <summary>
        /// Gets or sets the worldmap.
        /// </summary>
        /// <value>The worldmap.</value>
        public UiWorldmap Worldmap { get; set; }

        /// <summary>
        /// Gets or sets the game details label.
        /// </summary>
        /// <value>The game details label.</value>
        public UiText GameDetailsLabel { get; set; }

        /// <summary>
        /// Gets or sets the game details.
        /// </summary>
        /// <value>The game details.</value>
        public UiText GameDetails { get; set; }

        /// <summary>
        /// Gets or sets the game controls label.
        /// </summary>
        /// <value>The game controls label.</value>
        public UiText GameControlsLabel { get; set; }

        /// <summary>
        /// Gets or sets the game controls.
        /// </summary>
        /// <value>The game controls.</value>
        public UiText GameControls { get; set; }

        IGameManager game;

        /// <summary>
        /// Loads the content.
        /// </summary>
        public override void LoadContent()
        {
            game = new GameManager();
            int level = 0;

            if (ScreenArgs is not null && ScreenArgs.Length >= 1)
            {
                level = int.Parse(ScreenArgs[0]);
            }

            game.Start(level);

            UiManager.Instance.UiElements.Add(Worldmap);
            UiManager.Instance.UiElements.Add(GameDetailsLabel);
            UiManager.Instance.UiElements.Add(GameDetails);
            UiManager.Instance.UiElements.Add(GameControlsLabel);
            UiManager.Instance.UiElements.Add(GameControls);

            Worldmap.AssociateGameManager(ref game);

            SetChildrenProperties();

            base.LoadContent();
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(float gameTime)
        {
            base.Update(gameTime);

            SetChildrenProperties();

            if (game.GetCompletion())
            {
                game.Stop();

                // TODO: Save progress
                // TODO: What if this is the last level?
                // TODO: Switch to a transition screen before starting
                game.Start(game.World.Level + 1);
            }
        }

        /// <summary>
        /// Registers the events.
        /// </summary>
        protected override void RegisterEvents()
        {
            base.RegisterEvents();

            game.PlayerMoved += OnGamePlayerMoved;
        }

        /// <summary>
        /// Unregisters the events.
        /// </summary>
        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();

            game.PlayerMoved -= OnGamePlayerMoved;
        }

        protected override void OnKeyPressed(object sender, ConsoleKeyEventArgs e)
        {
            switch (e.Key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    game.MovePlayer(-1, 0);
                    break;

                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    game.MovePlayer(0, -1);
                    break;

                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    game.MovePlayer(1, 0);
                    break;

                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    game.MovePlayer(0, 1);
                    break;

                case ConsoleKey.R:
                    game.Start(game.World.Level);
                    break;

                case ConsoleKey.Escape:
                    ScreenManager.Instance.ChangeScreens(nameof(TitleScreen));
                    break;
            }

            base.OnKeyPressed(sender, e);
        }

        void SetChildrenProperties()
        {
            GameDetailsLabel.Location = new Point2D(0, Worldmap.ClientRectangle.Bottom + 1);
            GameDetailsLabel.Size = new Size2D(GameDetailsLabel.Text.Length, 1);

            GameDetails.Location = new Point2D(0, GameDetailsLabel.ClientRectangle.Bottom);
            GameDetails.Size = new Size2D(GraphicsManager.Instance.Resolution.Width, 3);

            GameControlsLabel.Location = new Point2D(0, GameDetails.ClientRectangle.Bottom + 1);
            GameControlsLabel.Size = new Size2D(GameControlsLabel.Text.Length, 1);

            GameControls.Location = new Point2D(0, GameControlsLabel.ClientRectangle.Bottom);
            GameControls.Size = new Size2D(GraphicsManager.Instance.Resolution.Width, 1);

            GameDetails.Text = $"Level: {game.World.Level}" + Environment.NewLine +
                               $"Player coordinates: {game.World.PlayerPosition.X},{game.World.PlayerPosition.Y}" + Environment.NewLine +
                               $"Moves: {game.World.Moves}";
        }

        void OnGamePlayerMoved(object sender, PlayerMovementEventArgs e)
            => ScreenManager.Instance.Draw(GraphicsManager.Instance.SpriteBatch);
    }
}

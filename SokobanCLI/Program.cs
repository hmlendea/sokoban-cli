using System;

using SokobanCLI.GameLogic.Managers;
using SokobanCLI.Graphics;
using SokobanCLI.Input;
using SokobanCLI.UI;

namespace SokobanCLI
{
    /// <summary>
    /// Application entry point and composition root.
    /// </summary>
    public static class Program
    {
        private static AsciiSpriteBatch spriteBatch;

        private static IGameManager game;

        private static float gameTime;

        private static void Main()
        {
            Console.CursorVisible = false;

            game = new GameManager();

            LoadContent();
            GraphicsManager.Instance.Clear(ConsoleColor.Black);

            while (true)
            {
                Update();
                Draw();
            }
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        private static void LoadContent()
        {
            spriteBatch = new AsciiSpriteBatch();
            spriteBatch.LoadContent();

            GraphicsManager.Instance.LoadContent();
            GraphicsManager.Instance.SpriteBatch = spriteBatch;

            ScreenManager.Instance.LoadContent();
            InputManager.Instance.KeyboardKeyPressed += (sender, e) => ScreenManager.Instance.Draw(spriteBatch);
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        private static void Update()
        {
            spriteBatch.Update();

            InputManager.Instance.Update();
            ScreenManager.Instance.Update(gameTime);
            GraphicsManager.Instance.Update(gameTime);

            gameTime += 1;
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        private static void Draw()
        {
            ScreenManager.Instance.Draw(spriteBatch);
            GraphicsManager.Instance.Draw();
        }
    }
}

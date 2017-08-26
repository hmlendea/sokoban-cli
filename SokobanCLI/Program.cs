using System;

using SokobanCLI.GameLogic.Managers;
using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Graphics;
using SokobanCLI.Input;
using SokobanCLI.Ui;

namespace SokobanCLI
{
    class Program
    {
        static AsciiSpriteBatch spriteBatch;

        static IGameManager game;

        static float gameTime;

        static void Main()
        {
            Console.CursorVisible = false;

            game = new GameManager();

            LoadContent();
            GraphicsManager.Instance.Clear(ConsoleColor.Black);

            // TODO: Have an end to this
            while (true)
            {
                Update();
                Draw();
            }

            UnloadContent();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        static void LoadContent()
        {
            spriteBatch = new AsciiSpriteBatch();
            spriteBatch.LoadContent();

            GraphicsManager.Instance.LoadContent();
            GraphicsManager.Instance.SpriteBatch = spriteBatch;

            ScreenManager.Instance.LoadContent();
            InputManager.Instance.KeyboardKeyPressed += delegate { ScreenManager.Instance.Draw(spriteBatch); };
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        static void UnloadContent()
        {
            ScreenManager.Instance.UnloadContent();
            GraphicsManager.Instance.UnloadContent();
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        static void Update()
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
        static void Draw()
        {
            ScreenManager.Instance.Draw(spriteBatch);
            GraphicsManager.Instance.Draw();
        }
    }
}

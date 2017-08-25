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
            Console.Clear();

            game = new GameManager();
            
            LoadContent();

            // TODO: Have an end to this
            while (true)
            {
                Update();
                Draw();
            }

            UnloadContent();
        }

        static void LoadContent()
        {
            spriteBatch = new AsciiSpriteBatch();
            spriteBatch.LoadContent();

            GraphicsManager.Instance.SpriteBatch = spriteBatch;
            ScreenManager.Instance.LoadContent();
            InputManager.Instance.KeyboardKeyPressed += delegate { ScreenManager.Instance.Draw(spriteBatch); };
        }

        static void UnloadContent()
        {
            ScreenManager.Instance.UnloadContent();
        }

        static void Update()
        {
            spriteBatch.Update();

            InputManager.Instance.Update();
            ScreenManager.Instance.Update(gameTime);

            gameTime += 1;
        }

        static void Draw()
        {
            ScreenManager.Instance.Draw(spriteBatch);
            
            for (int y = 0; y < spriteBatch.Size.Height; y++)
            {
                for (int x = 0; x <spriteBatch.Size.Width; x++)
                {
                    Console.SetCursorPosition(x, y);
                    Console.BackgroundColor = spriteBatch.BackgroundColourArray[x, y];
                    Console.ForegroundColor = spriteBatch.ForegroundColourArray[x, y];

                    Console.Write(spriteBatch.CharArray[x, y]);
                }
            }
        }
    }
}

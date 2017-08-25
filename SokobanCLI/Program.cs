using System;

using SokobanCLI.GameLogic.Managers;
using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Input;
using SokobanCLI.Ui;

namespace SokobanCLI
{
    class Program
    {
        static IGameManager game;
        
        static float gameTime;

        static void Main()
        {
            Console.CursorVisible = false;
            Console.Clear();

            game = new GameManager();
            
            LoadContent();
            Draw();

            // TODO: Have an end to this
            while (true)
            {
                Update();
                //Draw();
            }

            UnloadContent();
        }

        static void LoadContent()
        {
            ScreenManager.Instance.LoadContent();
            InputManager.Instance.KeyboardKeyPressed += delegate { ScreenManager.Instance.Draw(); };
        }

        static void UnloadContent()
        {
            ScreenManager.Instance.UnloadContent();
        }

        static void Update()
        {
            InputManager.Instance.Update();
            ScreenManager.Instance.Update(gameTime);

            gameTime += 1;
        }

        static void Draw()
        {
            ScreenManager.Instance.Draw();
        }
    }
}

using System;
using System.IO;

using SokobanCLI.GameLogic.Managers;
using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Input;
using SokobanCLI.Ui;

namespace SokobanCLI
{
    class Program
    {
        static ScreenType currentScreenType;

        static IGameManager game;
        
        static float gameTime;

        static void Main()
        {
            Console.CursorVisible = false;
            Console.Clear();

            game = new GameManager();
            
            
            LoadContent();

            while (true)
            {
                Update();
                Draw();
            }

            UnloadContent();

            Console.ReadKey();
            
            
            MainMenu();

            //MainMenu mainMenu = new MainMenu();
            //mainMenu.Show();
        }

        static void LoadContent()
        {
            ScreenManager.Instance.LoadContent();
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

        static void MainMenu()
        {
            SwitchScreen(ScreenType.MainMenu);

            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (File.Exists("progress.sav"))
                        Start(int.Parse(File.ReadAllText("progress.sav")));
                    else
                        Start(0);
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Start(0);
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                case ConsoleKey.Escape:
                    Console.ResetColor();
                    Console.Clear();
                    Environment.Exit(0);
                    break;

                default:
                    MainMenu();
                    break;
            }
        }

        static void Start(int level)
        {
            game.Start(level);

            SwitchScreen(ScreenType.LevelStarting);

            Console.ReadKey();
            Console.Clear();

            while (game.IsRunning)
            {
                DrawMenu();

                CheckInput();
                CheckCompletion();
            }
        }

        static void DrawMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Game details");
            Console.ResetColor();
            Console.WriteLine("Level: " + game.World.Level);
            Console.WriteLine("Player Coordinates: " + game.World.PlayerPosition.X.ToString().PadLeft(2) + "," + game.World.PlayerPosition.Y.ToString().PadLeft(2));
            Console.WriteLine("Moves: " + game.World.Moves);
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Controls");
            Console.ResetColor();
            Console.WriteLine("WASD and Arrows: Movement");
            Console.WriteLine("R: Restart level");
            Console.WriteLine("ESC: Exit");
        }

        static void CheckInput()
        {
            ConsoleKeyInfo key = Console.ReadKey();

            switch (key.Key)
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
                    Start(game.World.Level);
                    break;

                case ConsoleKey.Escape:
                    MainMenu();
                    break;
            }
        }

        static void CheckCompletion()
        {
            if (game.GetCompletion())
            {
                game.Stop();

                Console.ReadKey();

                if (File.Exists("Levels/" + (game.World.Level + 1) + ".lvl"))
                {
                    SaveProgress(game.World.Level + 1);
                    Start(game.World.Level + 1);
                }
                else
                {
                    SwitchScreen(ScreenType.GameFinished);
                    Console.ReadKey();

                    SaveProgress(0);
                    SwitchScreen(ScreenType.MainMenu);
                    MainMenu();
                }
            }
        }

        static void SaveProgress(int level)
        {
            File.WriteAllText("progress.sav", level.ToString());
        }

        static void SwitchScreen(ScreenType screenType)
        {
            currentScreenType = screenType;
            ScreenDrawer.Draw(currentScreenType);
        }
    }
}

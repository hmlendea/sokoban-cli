using System;
using System.IO;

using SokobanCLI.UI;

namespace SokobanCLI
{
    class Program
    {
        static bool isRunning;
        static World world;
        static ScreenType currentScreenType;

        static void Main()
        {
            Console.Clear();

            MainMenu();

            //MainMenu mainMenu = new MainMenu();
            //mainMenu.Show();
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
            isRunning = true;
            world = new World(level);

            SwitchScreen(ScreenType.LevelStarting);

            Console.ReadKey();
            Console.Clear();

            while (isRunning)
            {
                world.DrawLevel(0, 0);
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
            Console.WriteLine("Level: " + world.Level);
            Console.WriteLine("Player Coordinates: " + world.PlayerPosX.ToString().PadLeft(2) + "," + world.PlayerPosY.ToString().PadLeft(2));
            Console.WriteLine("Moves: " + world.Moves);
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
                    world.MovePlayer(world.PlayerPosX - 1, world.PlayerPosY);
                    break;

                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    world.MovePlayer(world.PlayerPosX, world.PlayerPosY - 1);
                    break;

                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    world.MovePlayer(world.PlayerPosX + 1, world.PlayerPosY);
                    break;

                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    world.MovePlayer(world.PlayerPosX, world.PlayerPosY + 1);
                    break;

                case ConsoleKey.R:
                    Start(world.Level);
                    break;

                case ConsoleKey.Escape:
                    MainMenu();
                    break;
            }
        }

        static void CheckCompletion()
        {
            if (world.Completed)
            {
                isRunning = false;

                Console.ReadKey();

                if (File.Exists("Levels/" + (world.Level + 1) + ".lvl"))
                {
                    SaveProgress(world.Level + 1);
                    Start(world.Level + 1);
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

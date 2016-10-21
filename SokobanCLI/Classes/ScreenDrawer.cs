using System;

namespace SokobanCLI
{
    public enum ScreenType
    {
        MainMenu,
        LevelStarting,
        LevelCompleted,
        GameFinished
    }

    public static class ScreenDrawer
    {
        public static void Draw(ScreenType screenType)
        {
            Console.ResetColor();

            switch (screenType)
            {
                case ScreenType.MainMenu:
                    DrawMainMenu();
                    break;
                case ScreenType.LevelStarting:
                    DrawLevelStarting();
                    break;
                case ScreenType.LevelCompleted:
                    DrawLevelCompleted();
                    break;
                case ScreenType.GameFinished:
                    DrawGameFinished();
                    break;
            }
        }

        private static void DrawMainMenu()
        {
            Console.Clear();

            Console.Write("Welcome to ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("SokobanCLI");
            Console.ResetColor();
            Console.WriteLine(" by Mlendea Horatiu!");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Main Menu");
            Console.ResetColor();
            Console.WriteLine("1) Continue");
            Console.WriteLine("2) New Game");
            Console.WriteLine("3) Exit");
        }

        private static void DrawLevelStarting()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();

            string message = "Press any key to start the level ...";

            for (int i = 0; i < (Console.BufferHeight - 1) / 2; i++)
                Console.WriteLine();

            for (int j = 0; j < (Console.BufferWidth - message.Length) / 2; j++)
                Console.Write(" ");
            
            Console.Write(message);
        }

        private static void DrawLevelCompleted()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();

            for (int i = 0; i < (Console.BufferHeight - 2) / 2; i++)
                Console.WriteLine();

            Console.WriteLine("Congratulations, you completed the level!");
            Console.WriteLine("Progress saved...");
        }

        private static void DrawGameFinished()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();

            Console.WriteLine("Congratulations, you completet the game!");
            Console.WriteLine("Press any key to go back to the Main Menu...");

        }
    }
}


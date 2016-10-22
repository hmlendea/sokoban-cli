using System;

namespace SokobanCLI.UI
{
    public class MainMenu : Menu
    {
        public MainMenu()
        {
            Name = "Main Menu";

            MenuHelpList.Add("Continue last game");
            MenuHelpList.Add("Start a new game");
        }

        public override void HandleCommand()
        {
            switch (CurrentKey)
            {
                case ConsoleKey.UpArrow:
                    if (ActiveItem > 0)
                        ActiveItem -= 1;
                    break;

                case ConsoleKey.DownArrow:
                    if (ActiveItem < TotalItems - 1)
                        ActiveItem += 1;
                    break;

                case ConsoleKey.Enter:
                    switch (ActiveItem)
                    {
                        case 0:
                            Console.WriteLine("aaaa");
                            break;

                        case 1:
                            Console.WriteLine("bbbb\nb");
                            break;
                    }
                    break;
            }
        }
    }
}

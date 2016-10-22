using System;
using System.Collections.Generic;

using SokobanCLI.Utils;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SokobanCLI.UI
{
    public class Menu
    {
        public string Name { get; set; }

        public int ActiveItem { get; set; }

        public int TotalItems
        {
            get { return MenuHelpList.Count; }
        }

        public ConsoleColor ActiveItemColor { get; set; }

        public ConsoleColor TitleColor { get; set; }

        protected ConsoleKey CurrentKey { get; private set; }

        public ConsoleKey ExitKey { get; set; }

        public List<string> MenuHelpList { get; set; }

        public Menu()
        {
            ActiveItem = 0;

            ActiveItemColor = ConsoleColor.Cyan;
            TitleColor = ConsoleColor.Green;

            ExitKey = ConsoleKey.Escape;
            MenuHelpList = new List<string>();
        }

        public Menu(string name)
            : this()
        {
            Name = name;
        }

        public virtual string Input(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public bool InputPermission(string prompt)
        {
            Console.Write(prompt);
            Console.Write(" (y/N) ");

            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey();

                switch (c.Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine();
                        return true;

                    case ConsoleKey.N:
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return false;

                    default:
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        break;
                }
            }
        }

        public virtual void HandleCommand()
        {

        }

        public void Show()
        {
            while (CurrentKey != ExitKey)
            {
                Console.Clear();

                Write("-===< ", ConsoleColor.Yellow);
                Write(Name, TitleColor);
                Write(" >===-", ConsoleColor.Yellow);
                WriteLine();

                for (int i = 0; i < MenuHelpList.Count; i++)
                    if (ActiveItem == i)
                        WriteLine("► " + MenuHelpList[i] + " ◄", ActiveItemColor);
                    else
                        WriteLine("  " + MenuHelpList[i] + "  ");
                
                //Console.WriteLine("Use '" + keyExit + "' to exit this menu");

                CurrentKey = Console.ReadKey().Key;

                HandleCommand();
            }
        }

        public void Write(string text)
        {
            Console.Write(text);
        }

        public void Write(string text, ConsoleColor foreColor)
        {
            ConsoleColor foreColorOld = Console.ForegroundColor;

            Console.ForegroundColor = foreColor;

            Console.Write(text);

            Console.ForegroundColor = foreColorOld;
        }

        public void Write(string text, ConsoleColor foreColor, ConsoleColor backColor)
        {
            ConsoleColor foreColorOld = Console.ForegroundColor;
            ConsoleColor backColorOld = Console.BackgroundColor;

            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;

            Console.Write(text);

            Console.ForegroundColor = foreColorOld;
            Console.BackgroundColor = backColorOld;
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteLine(string text, ConsoleColor foreColor)
        {
            Write(text + Environment.NewLine, foreColor);
        }

        public void WriteLine(string text, ConsoleColor foreColor, ConsoleColor backColor)
        {
            Write(text + Environment.NewLine, foreColor, backColor);
        }
    }
}

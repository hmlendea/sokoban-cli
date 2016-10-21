using System;
using System.Collections.Generic;

using SokobanCLI.Utils;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SokobanCLI.UI
{
    public class Menu
    {
        string name;
        int activeItem;
        ConsoleColor activeItemColor;
        ConsoleColor titleColor;
        ConsoleKey key;
        ConsoleKey keyExit;
        List<string> menuHelpList;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int ActiveItem
        {
            get { return activeItem; }
            set { activeItem = value; }
        }

        public int TotalItems
        {
            get { return menuHelpList.Count; }
        }

        public ConsoleColor ActiveItemColor
        {
            get { return activeItemColor; }
            set { activeItemColor = value; }
        }

        public ConsoleColor TitleColor
        {
            get { return titleColor; }
            set { titleColor = value; }
        }

        protected ConsoleKey CurrentKey
        {
            get { return key; }
        }

        public ConsoleKey ExitKey
        {
            get { return keyExit; }
            set { keyExit = value; }
        }

        public List<string> MenuHelpList
        {
            get { return menuHelpList; }
            set { menuHelpList = value; }
        }

        public Menu()
        {
            activeItem = 0;

            activeItemColor = ConsoleColor.Cyan;
            titleColor = ConsoleColor.Green;

            keyExit = ConsoleKey.Escape;
            menuHelpList = new List<string>();
        }

        public Menu(string name)
            : this()
        {
            this.name = name;
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
            while (key != keyExit)
            {
                Console.Clear();

                Write("-===< ", ConsoleColor.Yellow);
                Write(name, titleColor);
                Write(" >===-", ConsoleColor.Yellow);
                WriteLine();

                for (int i = 0; i < menuHelpList.Count; i++)
                    if (activeItem == i)
                        WriteLine("► " + menuHelpList[i] + " ◄", activeItemColor);
                    else
                        WriteLine("  " + menuHelpList[i] + "  ");
                
                //Console.WriteLine("Use '" + keyExit + "' to exit this menu");

                key = Console.ReadKey().Key;

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

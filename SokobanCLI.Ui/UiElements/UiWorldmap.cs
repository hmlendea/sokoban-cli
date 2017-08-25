using System;

using SokobanCLI.GameLogic.Managers.Interfaces;
using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Ui.UiElements
{
    /// <summary>
    /// World map UI element.
    /// </summary>
    public class UiWorldmap : UiElement
    {
        IGameManager game;

        UiText text;

        public override void LoadContent()
        {
            text = new UiText
            {
                Size = new Size2D(14, 14)
            };

            Children.Add(text);

            base.LoadContent();
        }

        /// <summary>
        /// Draw the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public override void Draw(AsciiSpriteBatch spriteBatch)
        {
            text.Text = string.Empty;
            
            for (int x = 0; x < game.World.Size.Width; x++)
            {
                for (int y = 0; y < game.World.Size.Height; y++)
                {
                    if (x == game.World.PlayerPosition.X && y == game.World.PlayerPosition.Y)
                    {
                        // ConsoleColor.DarkGreen;

                        if (game.World.Tiles[game.World.PlayerPosition.X, game.World.PlayerPosition.Y].Id == 3)
                        {
                            text.Text += '☻';
                        }
                        else
                        {
                            text.Text += '☺';
                        }
                    }
                    else
                    {
                       // game.World.Tiles[x, y].Colour;

                        if (game.World.Tiles[x, y].Id == 1)
                        {
                            text.Text += GetWallShape(x, y);
                        }
                        else
                        {
                            text.Text += game.World.Tiles[x, y].Character;
                        }
                    }
                }

                text.Text += Environment.NewLine;
            }

            // Console.ResetColor();

            // TODO: Draw
            base.Draw(spriteBatch);
        }

        // TODO: Handle this better
        /// <summary>
        /// Associates the game manager.
        /// </summary>
        /// <param name="game">Game.</param>
        public void AssociateGameManager(ref IGameManager game)
        {
            this.game = game;
        }

        char GetWallShape(int x, int y)
        {
            char c = '?';

            bool n, w, s, e;

            if (x > 0)
            {
                n = (game.World.Tiles[x - 1, y].Id == 1);
            }
            else
            {
                n = false;
            }

            if (y > 0)
            {
                w = (game.World.Tiles[x, y - 1].Id == 1);
            }
            else
            {
                w = false;
            }

            if (y < Size.Height - 1)
            {
                e = (game.World.Tiles[x, y + 1].Id == 1);
            }
            else
            {
                e = false;
            }

            if (x < Size.Width - 1)
            {
                s = (game.World.Tiles[x + 1, y].Id == 1);
            }
            else
            {
                s = false;
            }

            if (n && w && s && e)
            {
                c = '╬';
            }
            else if (n && w && s)
            {
                c = '╣';
            }
            else if (n && w && e)
            {
                c = '╩';
            }
            else if (n && s && e)
            {
                c = '╠';
            }
            else if (n && w)
            {
                c = '╝';
            }
            else if (n && s)
            {
                c = '║';
            }
            else if (n && e)
            {
                c = '╚';
            }
            else if (n)
            {
                c = '║';
            }
            else if (w && s && e)
            {
                c = '╦';
            }
            else if (w && s)
            {
                c = '╗';
            }
            else if (w && e)
            {
                c = '═';
            }
            else if (w)
            {
                c = '═';
            }
            else if (s && e)
            {
                c = '╔';
            }
            else if (s)
            {
                c = '║';
            }
            else if (e)
            {
                c = '═';
            }

            return c;
        }
    }
}

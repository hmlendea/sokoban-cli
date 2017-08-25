using System.Collections.Generic;
using System.Linq;

using SokobanCLI.Graphics.Enumerations;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.Graphics
{
    public class AsciiSprite
    {
        public Point2D Location { get; set; }

        public Size2D Size { get; set; }

        public string Text { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public void LoadContent()
        {
            Text = string.Empty;
        }

        public void UnloadContent()
        {
            Text = string.Empty;
        }

        public void Update(float gameTime)
        {
            /*
            List<string> lines = Text.Split('\n').ToList();
            int maxWidth = 0;

            foreach(string line in lines)
            {
                if (line.Length > maxWidth)
                {
                    maxWidth = line.Length;
                }
            }

            Size = new Size2D(maxWidth, lines.Count);
            */
        }

        public void Draw(AsciiSpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this);
        }
    }
}

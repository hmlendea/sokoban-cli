using System;
using System.IO;
using System.Xml.Serialization;

using SokobanCLI.Input;
using SokobanCLI.Input.Events;

namespace SokobanCLI.Ui.Screens
{
    public class Screen
    {
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the xml path.
        /// </summary>
        /// <value>The xml path.</value>
        public string XmlPath { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [XmlIgnore]
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the background colour.
        /// </summary>
        /// <value>The background colour.</value>
        public ConsoleColor BackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour.
        /// </summary>
        /// <value>The background colour.</value>
        public ConsoleColor ForegroundColour { get; set; }

        /// <summary>
        /// Gets or sets the highlight colour.
        /// </summary>
        /// <value>The highlight colour.</value>
        public ConsoleColor HighlightColour { get; set; }

        /// <summary>
        /// Gets or sets the screen arguments.
        /// </summary>
        /// <value>The screen arguments.</value>
        public string[] ScreenArgs { get; set; }

        /// <summary>
        /// Occurs when the a kew is pressed while this <see cref="Screen"/> has input focus.
        /// </summary>
        public event ConsoleKeyEventHandler KeyPressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen"/> class.
        /// </summary>
        public Screen()
        {
            Type = GetType();
            Id = Guid.NewGuid().ToString();

            XmlPath = Path.Combine("Screens", $"{Type.Name}.xml");

            BackgroundColour = ConsoleColor.Black;
            ForegroundColour = ConsoleColor.White;
            HighlightColour = ConsoleColor.Yellow;
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public virtual void LoadContent()
        {
            UiManager.Instance.LoadContent();

            RegisterEvents();
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public virtual void UnloadContent()
        {
            UiManager.Instance.UnloadContent();

            UnregisterEvents();
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public virtual void Update(float gameTime)
        {
            UiManager.Instance.Update(gameTime);
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        public virtual void Draw()
        {
            Console.BackgroundColor = BackgroundColour;
            Console.Clear();

            UiManager.Instance.Draw();
        }

        protected virtual void RegisterEvents()
        {
            InputManager.Instance.KeyboardKeyPressed += OnKeyPressed;
        }

        protected virtual void UnregisterEvents()
        {
            InputManager.Instance.KeyboardKeyPressed -= OnKeyPressed;
        }

        protected virtual void OnKeyPressed(object sender, ConsoleKeyEventArgs e)
        {
            KeyPressed?.Invoke(sender, e);
        }
    }
}

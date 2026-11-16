using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SokobanCLI.Graphics;
using SokobanCLI.UI.UiElements;

namespace SokobanCLI.UI
{
    /// <summary>
    /// GUI manager.
    /// </summary>
    public sealed class UiManager : IUiManager
    {
        private static volatile UiManager instance;
        private static readonly Lock syncRoot = new();

        /// <summary>
        /// Gets or sets the GUI elements.
        /// </summary>
        /// <value>The GUI elements.</value>
        public List<UiElement> UiElements { get; set; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static UiManager Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (syncRoot)
                    {
                        instance ??= new UiManager();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiManager"/> class.
        /// </summary>
        public UiManager() => UiElements = [];

        /// <summary>
        /// Loads the content.
        /// </summary>
        public void LoadContent() => UiElements.ToList().ForEach(w => w.LoadContent());

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public void UnloadContent()
        {
            UiElements.ForEach(w => w.UnloadContent());
            UiElements.Clear();
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public void Update(float gameTime)
        {
            UiElements.RemoveAll(e => e.IsDisposed);
            UiElements.Where(e => e.IsEnabled).ToList().ForEach(e => e.Update(gameTime));
        }

        /// <summary>
        /// Draws the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public void Draw(AsciiSpriteBatch spriteBatch)
            => UiElements.Where(e => e.IsVisible).ToList().ForEach(e => e.Draw(spriteBatch));

        /// <summary>
        /// Focuses the input on the element with the specified identifier.
        /// </summary>
        /// <param name="id">Element identifier.</param>
        public void FocusElement(string id)
        {
            UiElements.ForEach(e => e.HasInputFocus = false);
            UiElement element = UiElements.FirstOrDefault(e => e.Id == id);
            if (element is not null)
            {
                element.HasInputFocus = true;
            }
        }

        /// <summary>
        /// Focuses the input on the specified element.
        /// </summary>
        /// <param name="element">Element.</param>
        public void FocusElement(UiElement element) => FocusElement(element.Id);
    }
}

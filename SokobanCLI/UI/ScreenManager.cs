using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

using NuciDAL.IO;
using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Ui.Screens;

namespace SokobanCLI.Ui
{
    /// <summary>
    /// Screen manager.
    /// </summary>
    public class ScreenManager
    {
        static volatile ScreenManager instance;
        static readonly Lock syncRoot = new();

        Screen currentScreen, newScreen;

        readonly XmlFileObject<Screen> xmlScreenManager;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ScreenManager Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (syncRoot)
                    {
                        if (instance is null)
                        {
                            XmlFileObject<ScreenManager> xmlManager = new();
                            instance = xmlManager.Read(Path.Combine("Screens", $"{nameof(ScreenManager)}.xml"));
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        [XmlIgnore]
        public Size2D Size { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenManager"/> class.
        /// </summary>
        public ScreenManager()
        {
            currentScreen = new TitleScreen();

            xmlScreenManager = new XmlFileObject<Screen>
            {
                Type = currentScreen.Type
            };

            currentScreen = xmlScreenManager.Read(currentScreen.XmlPath);
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public void LoadContent()
        {
            Size = GraphicsManager.Instance.Resolution;
            currentScreen.LoadContent();
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public void UnloadContent() => currentScreen.UnloadContent();

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public void Update(float gameTime)
        {
            Size = GraphicsManager.Instance.Resolution;

            currentScreen.Update(gameTime);
        }

        /// <summary>
        /// Draw the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public void Draw(AsciiSpriteBatch spriteBatch) => currentScreen.Draw(spriteBatch);

        /// <summary>
        /// Changes the screen.
        /// </summary>
        /// <param name="screenName">Screen name.</param>
        public void ChangeScreens(string screenName) => ChangeScreens(screenName, null);

        /// <summary>
        /// Changes the screen.
        /// </summary>
        /// <param name="screenName">Screen name.</param>
        /// <param name="screenArgs">Screen arguments.</param>
        public void ChangeScreens(string screenName, string[] screenArgs)
        {
            GraphicsManager.Instance.Clear(currentScreen.BackgroundColour);

            newScreen = (Screen)Activator.CreateInstance(Type.GetType($"{typeof(Screen).Namespace}.{screenName}"));

            xmlScreenManager.Type = newScreen.Type;

            if (File.Exists(currentScreen.XmlPath))
            {
                newScreen = xmlScreenManager.Read(newScreen.XmlPath);
            }

            newScreen.ScreenArgs = screenArgs;

            currentScreen.UnloadContent();
            currentScreen = newScreen;
            currentScreen.LoadContent();
        }
    }
}

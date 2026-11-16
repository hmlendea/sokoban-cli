using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

using NuciDAL.IO;

using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.UI.Screens;

namespace SokobanCLI.UI
{
    /// <summary>
    /// Screen manager.
    /// </summary>
    public sealed class ScreenManager : IScreenManager
    {
        private static readonly Lock SyncRoot = new();

        private static readonly string[] NoScreenArgs = [];

        private static volatile ScreenManager instance;

        private readonly XmlFileObject<Screen> screenReader;

        private Screen currentScreen;

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
                    lock (SyncRoot)
                    {
                        instance ??= new ScreenManager();
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

            screenReader = new XmlFileObject<Screen>
            {
                Type = currentScreen.Type
            };

            currentScreen = screenReader.Read(currentScreen.XmlPath);
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
        public void ChangeScreens(string screenName) => ChangeScreens(screenName, NoScreenArgs);

        /// <summary>
        /// Changes the screen.
        /// </summary>
        /// <param name="screenName">Screen name.</param>
        /// <param name="screenArgs">Screen arguments.</param>
        public void ChangeScreens(string screenName, string[] screenArgs)
        {
            GraphicsManager.Instance.Clear(currentScreen.BackgroundColour);

            Screen newScreen = CreateScreen(screenName, screenArgs);

            currentScreen.UnloadContent();
            currentScreen = newScreen;
            currentScreen.LoadContent();
        }

        private Screen CreateScreen(string screenName, string[] screenArgs)
        {
            Type screenType = Type.GetType($"{typeof(Screen).Namespace}.{screenName}")!;
            Screen screen = (Activator.CreateInstance(screenType) as Screen)!;

            screenReader.Type = screen.Type;

            if (File.Exists(screen.XmlPath))
            {
                screen = screenReader.Read(screen.XmlPath);
            }

            screen.ScreenArgs = screenArgs;

            return screen;
        }
    }
}

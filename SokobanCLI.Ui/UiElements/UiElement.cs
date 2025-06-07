using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

using SokobanCLI.Graphics;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Input;
using SokobanCLI.Input.Events;

namespace SokobanCLI.Ui.UiElements
{
    /// <summary>
    /// GUI Element.
    /// </summary>
    public class UiElement : IComponent, IDisposable
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the location of this <see cref="UiElement"/>.
        /// </summary>
        /// <value>The location.</value>
        public Point2D Location { get; set; }

        /// <summary>
        /// Gets the size of this <see cref="UiElement"/>.
        /// </summary>
        /// <value>The size.</value>
        public Size2D Size { get; set; }

        /// <summary>
        /// Gets the screen area covered by this <see cref="UiElement"/>.
        /// </summary>
        /// <value>The screen area.</value>
        public Rectangle2D ClientRectangle => new(Location, Size);

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        /// <value>The opacity.</value>
        public float Opacity
        {
            get => _opacity;
            set => _opacity = Math.Max(0.0f, Math.Min(value, 1.0f));
        }

        /// <summary>
        /// Gets or sets the background colour.
        /// </summary>
        /// <value>The background colour.</value>
        public ConsoleColor BackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the foreground colour.
        /// </summary>
        /// <value>The foreground colour.</value>
        public ConsoleColor ForegroundColour { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UiElement"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UiElement"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UiElement"/> is hovered.
        /// </summary>
        /// <value><c>true</c> if hovered; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool Hovered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UiElement"/> has input focus.
        /// </summary>
        /// <value><c>true</c> if it has input focus; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool InputFocus { get; set; }

        /// <summary>
        /// Gets or sets the children GUI elements.
        /// </summary>
        /// <value>The children.</value>
        [XmlIgnore]
        public List<UiElement> Children { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UiElement"/> is destroyed.
        /// </summary>
        /// <value><c>true</c> if destroyed; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>The site.</value>
        [XmlIgnore]
        public ISite Site { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="UiElement"/> can raise events.
        /// </summary>
        /// <value><c>true</c> if can raise events; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        protected virtual bool CanRaiseEvents => true;

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <value>The container.</value>
        [XmlIgnore]
        public IContainer Container => Site?.Container;

        /// <summary>
        /// Gets a value indicating whether this <see cref="UiElement"/> is in design mode.
        /// </summary>
        /// <value><c>true</c> if in design mode; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        protected bool DesignMode => Site?.DesignMode ?? false;

        /// <summary>
        /// Occurs when the BackgroundColour property was changed.
        /// </summary>
        public event EventHandler BackgroundColourChanged;

        /// <summary>
        /// Occurs when a key is pressed while this <see cref="UiElement"/> has input focus.
        /// </summary>
        public event ConsoleKeyEventHandler KeyPressed;

        /// <summary>
        /// Occurs when this <see cref="UiElement"/> was disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Occurs when the ForegroundColour property was changed.
        /// </summary>
        public event EventHandler ForegroundColourChanged;

        /// <summary>
        /// Occurs when the Location property value changes.
        /// </summary>
        public event EventHandler LocationChanged;

        /// <summary>
        /// Occurs when the Size property value changes.
        /// </summary>
        public event EventHandler SizeChanged;

        ConsoleColor _oldBackgroundColour;
        ConsoleColor _oldForegroundColour;
        Point2D _oldLocation;
        Size2D _oldSize;
        float _opacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiElement"/> class.
        /// </summary>
        public UiElement()
        {
            Enabled = true;
            Visible = true;
            Opacity = 1.0f;

            BackgroundColour = ConsoleColor.Black;
            ForegroundColour = ConsoleColor.White;

            Id = Guid.NewGuid().ToString();

            Children = [];
        }

        ~UiElement() => Dispose();

        /// <summary>
        /// Loads the content.
        /// </summary>
        public virtual void LoadContent()
        {
            SetChildrenProperties();

            Children.ForEach(x => x.LoadContent());

            RegisterEvents();

            IsDisposed = false;
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        public virtual void UnloadContent()
        {
            UnregisterEvents();

            Children.ForEach(x => x.UnloadContent());
        }

        /// <summary>
        /// Update the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public virtual void Update(float gameTime)
        {
            Children.RemoveAll(w => w.IsDisposed);

            RaiseEvents();
            SetChildrenProperties();

            Children.Where(e => e.Enabled).ToList().ForEach(e => e.Update(gameTime));
        }

        /// <summary>
        /// Draw the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        public virtual void Draw(AsciiSpriteBatch spriteBatch)
            => Children
                .Where(e => e.Visible)
                .ToList()
                .ForEach(e => e.Draw(spriteBatch));

        /// <summary>
        /// Disposes of this <see cref="UiElement"/>.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            Children.ForEach(c => c.Dispose());
        }

        /// <summary>
        /// Disposes of this <see cref="UiElement"/>.
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            IsDisposed = true;

            lock (this)
            {
                if (Site != null && Site.Container != null)
                {
                    Site.Container.Remove(this);
                }

                UnloadContent();

                OnDisposed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Shows this GUI element.
        /// </summary>
        public virtual void Show()
        {
            Enabled = true;
            Visible = true;
        }

        /// <summary>
        /// Hide this GUI element.
        /// </summary>
        public virtual void Hide()
        {
            Enabled = false;
            Visible = false;
        }

        /// <summary>
        /// Registers the events.
        /// </summary>
        protected virtual void RegisterEvents()
            => InputManager.Instance.KeyboardKeyPressed += OnInputManagerKeyboardKeyPressed;

        /// <summary>
        /// Unregisters the events.
        /// </summary>
        protected virtual void UnregisterEvents()
            => InputManager.Instance.KeyboardKeyPressed -= OnInputManagerKeyboardKeyPressed;

        /// <summary>
        /// Raises the events.
        /// </summary>
        protected virtual void RaiseEvents()
        {
            if (!CanRaiseEvents)
            {
                return;
            }

            if (_oldBackgroundColour != BackgroundColour)
            {
                OnBackgroundColourChanged(this, null);
            }

            if (_oldForegroundColour != ForegroundColour)
            {
                OnForegroundColourChanged(this, null);
            }

            if (_oldLocation != Location)
            {
                OnLocationChanged(this, null);
            }

            if (_oldSize != Size)
            {
                OnSizeChanged(this, null);
            }
        }

        /// <summary>
        /// Sets the children properties.
        /// </summary>
        protected virtual void SetChildrenProperties()
        {
            _oldBackgroundColour = BackgroundColour;
            _oldForegroundColour = ForegroundColour;

            _oldLocation = Location;
            _oldSize = Size;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <returns>The service.</returns>
        /// <param name="service">Service.</param>
        protected virtual object GetService(Type service) => Site?.GetService(service);

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="UiElement"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="UiElement"/>.</returns>
        public override string ToString()
        {
            if (Site is null)
            {
                return GetType().FullName;
            }

            return $"{Site.Name} [{GetType().FullName}]";
        }

        /// <summary>
        /// Raised by the BackgroundColourChanged event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnBackgroundColourChanged(object sender, EventArgs e)
            => BackgroundColourChanged?.Invoke(sender, e);

        /// <summary>
        /// Fired by the Disposed event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnDisposed(object sender, EventArgs e)
            => Disposed?.Invoke(sender, e);

        /// <summary>
        /// Raised by the ForegroundColourChanged event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnForegroundColourChanged(object sender, EventArgs e)
            => ForegroundColourChanged?.Invoke(sender, e);

        /// <summary>
        /// Raised by the LocationChanged event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnLocationChanged(object sender, EventArgs e)
            => LocationChanged?.Invoke(this, e);

        /// <summary>
        /// Raised by the SizeChanged event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnSizeChanged(object sender, EventArgs e)
            => SizeChanged?.Invoke(this, e);

        /// <summary>
        /// Raised by the KeyPressed event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnKeyPressed(object sender, ConsoleKeyEventArgs e)
            => KeyPressed?.Invoke(sender, e);

        void OnInputManagerKeyboardKeyPressed(object sender, ConsoleKeyEventArgs e)
        {
            if (!Enabled || !Visible || !CanRaiseEvents || !InputFocus)
            {
                return;
            }

            OnKeyPressed(sender, e);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Ui.UiElements;

namespace SokobanCLI.Ui.Screens
{
    /// <summary>
    /// Menu screen.
    /// </summary>
    public class MenuScreen : Screen
    {
        /// <summary>
        /// Gets or sets the axis.
        /// </summary>
        /// <value>The axis.</value>
        public string Axis { get; set; }

        /// <summary>
        /// Gets or sets the spacing.
        /// </summary>
        /// <value>The spacing.</value>
        public int Spacing { get; set; }

        /// <summary>
        /// Gets or sets the links.
        /// </summary>
        /// <value>The links.</value>
        [XmlElement("Link")]
        public List<UiMenuLink> Links { get; set; }

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>The actions.</value>
        [XmlElement("Action")]
        public List<UiMenuAction> Actions { get; set; }

        /// <summary>
        /// Gets all the items.
        /// </summary>
        /// <value>The items.</value>
        [XmlIgnore]
        public List<UiMenuItem> Items => Links.Select(x => (UiMenuItem)x).Concat(
                                         Actions.Select(x => (UiMenuItem)x)).ToList();

        /// <summary>
        /// Gets the item number.
        /// </summary>
        /// <value>The item number.</value>
        [XmlIgnore]
        public int ItemNumber { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen"/> class.
        /// </summary>
        public MenuScreen()
        {
            Id = string.Empty;
            ItemNumber = 0;
            Axis = "Y";
            Spacing = 1;

            Links = new List<UiMenuLink>();
            Actions = new List<UiMenuAction>();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public override void LoadContent()
        {
            UiManager.Instance.UiElements.AddRange(Links);
            UiManager.Instance.UiElements.AddRange(Actions);

            base.LoadContent();

            AlignMenuItems();
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public override void Update(float gameTime)
        {
            int newSelectedItemIndex = ItemNumber;

            if (newSelectedItemIndex < 0)
            {
                newSelectedItemIndex = 0;
            }
            else if (newSelectedItemIndex > Items.Count - 1 && Items.Count > 0)
            {
                newSelectedItemIndex = Items.Count - 1;
            }

            UiManager.Instance.FocusElement(Items[newSelectedItemIndex]);

            ItemNumber = newSelectedItemIndex;

            base.Update(gameTime);
        }
        
        void AlignMenuItems()
        {
            Size2D dimensions = Size2D.Empty;

            Items.ForEach(item => dimensions += new Size2D(item.Size.Width + Spacing / 2,
                                                           item.Size.Height + Spacing / 2));

            dimensions = new Size2D((ScreenManager.Instance.Size.Width - dimensions.Width) / 2,
                                    (ScreenManager.Instance.Size.Height - dimensions.Height) / 2);

            foreach (UiMenuItem item in Items)
            {
                if ("Xx".Contains(Axis))
                {
                    item.Location = new Point2D(dimensions.Width, (ScreenManager.Instance.Size.Height - item.Size.Height) / 2);
                }
                else if ("Yy".Contains(Axis))
                {
                    item.Location = new Point2D((ScreenManager.Instance.Size.Width - item.Size.Width) / 2, dimensions.Height);
                }

                dimensions += new Size2D(item.Size.Width + Spacing / 2,
                                         item.Size.Height + Spacing / 2);
            }
        }
    }
}

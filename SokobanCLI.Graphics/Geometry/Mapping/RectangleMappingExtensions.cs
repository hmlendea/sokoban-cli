using SystemRectangle = System.Drawing.Rectangle;

namespace SokobanCLI.Graphics.Geometry.Mapping
{
    public static class RectangleMappingExtensions
    {
        // >>> TO NARIVIA

        /// <summary>
        /// Convers a <see cref="SystemRectangle"/> into to a <see cref="Rectangle2D"/>.
        /// </summary>
        /// <param name="source">Source <see cref="SystemRectangle"/>.</param>
        /// <returns>The <see cref="Rectangle2D"/>.</returns>
        public static Rectangle2D ToRectangle2D(this SystemRectangle source)
        => new(source.X, source.Y, source.Width, source.Height);

        // >>> TO SYSTEM

        /// <summary>
        /// Convers a <see cref="Rectangle2D"/> into to a <see cref="SystemRectangle"/>.
        /// </summary>
        /// <param name="source">Source <see cref="Rectangle2D"/>.</param>
        /// <returns>The <see cref="SystemRectangle"/>.</returns>
        public static SystemRectangle ToSystemRectangle(this Rectangle2D source)
        => new(source.X, source.Y, source.Width, source.Height);
    }
}

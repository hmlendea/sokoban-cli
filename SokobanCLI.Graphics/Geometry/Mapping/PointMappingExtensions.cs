using SystemPoint = System.Drawing.Point;

namespace SokobanCLI.Graphics.Geometry.Mapping
{
    public static class PointMappingExtensions
    {
        // >>> TO NARIVIA

        /// <summary>
        /// Convers a <see cref="SystemPoint"/> into to a <see cref="Point2D"/>.
        /// </summary>
        /// <param name="source">Source <see cref="SystemPoint"/>.</param>
        /// <returns>The <see cref="Point2D"/>.</returns>
        public static Point2D ToPoint2D(this SystemPoint source)
        => new(source.X, source.Y);

        // >>> TO SYSTEM

        /// <summary>
        /// Convers a <see cref="Point2D"/> into to a <see cref="SystemPoint"/>.
        /// </summary>
        /// <param name="source">Source <see cref="Point2D"/>.</param>
        /// <returns>The <see cref="SystemPoint"/>.</returns>
        public static SystemPoint ToSystemPoint(this Point2D source)
        => new(source.X, source.Y);
    }
}

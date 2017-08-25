using SystemSize = System.Drawing.Size;

namespace SokobanCLI.Graphics.Geometry.Mapping
{
    public static class SizeMappingExtensions
    {
        // >>> TO NARIVIA

        /// <summary>
        /// Convers a <see cref="SystemSize"/> into to a <see cref="Size2D"/>.
        /// </summary>
        /// <param name="source">Source <see cref="SystemSize"/>.</param>
        /// <returns>The <see cref="Size2D"/>.</returns>
        public static Size2D ToSize2D(this SystemSize source)
        => new Size2D(source.Width, source.Height);

        // >>> TO SYSTEM

        /// <summary>
        /// Convers a <see cref="Size2D"/> into to a <see cref="SystemSize"/>.
        /// </summary>
        /// <param name="source">Source <see cref="Size2D"/>.</param>
        /// <returns>The <see cref="SystemSize"/>.</returns>
        public static SystemSize ToSystemPoint(this Size2D source)
        => new SystemSize(source.Width, source.Height);
    }
}

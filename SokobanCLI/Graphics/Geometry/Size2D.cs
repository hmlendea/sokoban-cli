using System;

namespace SokobanCLI.Graphics.Geometry
{
    /// <summary>
    /// 2D size structure.
    /// </summary>
    public struct Size2D : IEquatable<Size2D>
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Size2D"/> is zero.
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// Gets the size of zero.
        /// </summary>
        /// <value>The size of zero.</value>
        public static Size2D Empty => new(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Size2D"/> structure.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public Size2D(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Size2D"/> structure.
        /// </summary>
        /// <param name="point">Point.</param>
        public Size2D(Point2D point)
        {
            Width = point.X;
            Height = point.Y;
        }

        /// <summary>
        /// Determines whether this <see cref="Size2D"/> is equal to another <see cref="Size2D"/>.
        /// </summary>
        /// <param name="other">The <see cref="Size2D"/> to compare with this <see cref="Size2D"/>.</param>
        /// <returns><c>true</c> if both sizes hold the same values; otherwise, <c>false</c>.</returns>
        public bool Equals(Size2D other)
            => Width == other.Width && Height == other.Height;

        /// <summary>
        /// Determines whether this <see cref="Size2D"/> is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Size2D"/>.</param>
        /// <returns><c>true</c> if the object is a <see cref="Size2D"/> holding the same values;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
            => obj is Size2D other && Equals(other);

        /// <summary>
        /// Adds the values of a <see cref="Size2D"/> to those of another <see cref="Size2D"/>,
        /// yielding a new <see cref="Size2D"/>.
        /// </summary>
        /// <param name="source">The first <see cref="Size2D"/> to add.</param>
        /// <param name="other">The second <see cref="Size2D"/> to add.</param>
        /// <returns>The <see cref="Size2D"/> that is the sum of both operands.</returns>
        public static Size2D operator +(Size2D source, Size2D other)
            => new(source.Width + other.Width, source.Height + other.Height);

        /// <summary>
        /// Subtracts the values of a <see cref="Size2D"/> from those of another <see cref="Size2D"/>,
        /// yielding a new <see cref="Size2D"/>.
        /// </summary>
        /// <param name="source">The <see cref="Size2D"/> to subtract from.</param>
        /// <param name="other">The <see cref="Size2D"/> to subtract.</param>
        /// <returns>The <see cref="Size2D"/> that is the difference of both operands.</returns>
        public static Size2D operator -(Size2D source, Size2D other)
            => new(source.Width - other.Width, source.Height - other.Height);

        /// <summary>
        /// Determines whether both <see cref="Size2D"/> instances hold the same values.
        /// </summary>
        /// <param name="source">The first <see cref="Size2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Size2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Size2D source, Size2D other)
            => source.Equals(other);

        /// <summary>
        /// Determines whether both <see cref="Size2D"/> instances hold different values.
        /// </summary>
        /// <param name="source">The first <see cref="Size2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Size2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Size2D source, Size2D other)
            => !source.Equals(other);

        /// <summary>
        /// Serves as a hash function for a <see cref="Size2D"/> instance.
        /// </summary>
        /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
        public override int GetHashCode() => HashCode.Combine(Width, Height);
    }
}

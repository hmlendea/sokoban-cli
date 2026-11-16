using System;

namespace SokobanCLI.Graphics.Geometry
{
    /// <summary>
    /// 2D scale structure.
    /// </summary>
    public struct Scale2D : IEquatable<Scale2D>
    {
        /// <summary>
        /// Gets or sets the horizontal scale.
        /// </summary>
        /// <value>The horizontal scale.</value>
        public float Horizontal { get; set; }

        /// <summary>
        /// Gets or sets the vertical scale.
        /// </summary>
        /// <value>The vertical scale.</value>
        public float Vertical { get; set; }

        /// <summary>
        /// Gets a value indicating whether the values of this <see cref="Scale2D"/> are zero.
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => Horizontal == 0 && Vertical == 0;

        /// <summary>
        /// Gets a scale of zero.
        /// </summary>
        /// <value>The scale of zero.</value>
        public static Scale2D Empty => new(0, 0);

        /// <summary>
        /// Gets a scale of one.
        /// </summary>
        /// <value>The scale of one.</value>
        public static Scale2D One => new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale2D"/> structure.
        /// </summary>
        /// <param name="horizontal">Horizontal scale.</param>
        /// <param name="vertical">Vertical scale.</param>
        public Scale2D(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale2D"/> structure.
        /// </summary>
        /// <param name="point">Point.</param>
        public Scale2D(Point2D point)
        {
            Horizontal = point.X;
            Vertical = point.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale2D"/> structure.
        /// </summary>
        /// <param name="size">Size.</param>
        public Scale2D(Size2D size)
        {
            Horizontal = size.Width;
            Vertical = size.Height;
        }

        /// <summary>
        /// Determines whether this <see cref="Scale2D"/> is equal to another <see cref="Scale2D"/>.
        /// </summary>
        /// <param name="other">The <see cref="Scale2D"/> to compare with this <see cref="Scale2D"/>.</param>
        /// <returns><c>true</c> if both scales hold the same values; otherwise, <c>false</c>.</returns>
        public bool Equals(Scale2D other)
            => Horizontal.Equals(other.Horizontal) && Vertical.Equals(other.Vertical);

        /// <summary>
        /// Determines whether this <see cref="Scale2D"/> is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Scale2D"/>.</param>
        /// <returns><c>true</c> if the object is a <see cref="Scale2D"/> holding the same values;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
            => obj is Scale2D other && Equals(other);

        /// <summary>
        /// Adds the values of a <see cref="Scale2D"/> to those of another <see cref="Scale2D"/>,
        /// yielding a new <see cref="Scale2D"/>.
        /// </summary>
        /// <param name="source">The first <see cref="Scale2D"/> to add.</param>
        /// <param name="other">The second <see cref="Scale2D"/> to add.</param>
        /// <returns>The <see cref="Scale2D"/> that is the sum of both operands.</returns>
        public static Scale2D operator +(Scale2D source, Scale2D other)
            => new(source.Horizontal + other.Horizontal, source.Vertical + other.Vertical);

        /// <summary>
        /// Subtracts the values of a <see cref="Scale2D"/> from those of another <see cref="Scale2D"/>,
        /// yielding a new <see cref="Scale2D"/>.
        /// </summary>
        /// <param name="source">The <see cref="Scale2D"/> to subtract from.</param>
        /// <param name="other">The <see cref="Scale2D"/> to subtract.</param>
        /// <returns>The <see cref="Scale2D"/> that is the difference of both operands.</returns>
        public static Scale2D operator -(Scale2D source, Scale2D other)
            => new(source.Horizontal - other.Horizontal, source.Vertical - other.Vertical);

        /// <summary>
        /// Determines whether both <see cref="Scale2D"/> instances hold the same values.
        /// </summary>
        /// <param name="source">The first <see cref="Scale2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Scale2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Scale2D source, Scale2D other)
            => source.Equals(other);

        /// <summary>
        /// Determines whether both <see cref="Scale2D"/> instances hold different values.
        /// </summary>
        /// <param name="source">The first <see cref="Scale2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Scale2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Scale2D source, Scale2D other)
            => !source.Equals(other);

        /// <summary>
        /// Serves as a hash function for a <see cref="Scale2D"/> instance.
        /// </summary>
        /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
        public override int GetHashCode() => HashCode.Combine(Horizontal, Vertical);
    }
}

using System;

namespace SokobanCLI.Graphics.Geometry
{
    /// <summary>
    /// 2D orthogonal coordinate pair.
    /// </summary>
    public struct Point2D : IEquatable<Point2D>
    {
        /// <summary>
        /// Gets or sets the X-axis coordinate.
        /// </summary>
        /// <value>The X-axis coordinate.</value>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis coordinate.
        /// </summary>
        /// <value>The Y-axis coordinate.</value>
        public int Y { get; set; }

        /// <summary>
        /// Gets a value indicating whether both coordinates are zero.
        /// </summary>
        /// <value><c>true</c> if the coordinates are zero; otherwise, <c>false</c>.</value>
        public bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Gets a <see cref="Point2D"/> whose coordinates are zero.
        /// </summary>
        /// <value>The zero point.</value>
        public static Point2D Empty => new(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> structure.
        /// </summary>
        /// <param name="x">The X-axis coordinate.</param>
        /// <param name="y">The Y-axis coordinate.</param>
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2D"/> structure.
        /// </summary>
        /// <param name="size">Size.</param>
        public Point2D(Size2D size)
        {
            X = size.Width;
            Y = size.Height;
        }

        /// <summary>
        /// Determines whether this <see cref="Point2D"/> is equal to another <see cref="Point2D"/>.
        /// </summary>
        /// <param name="other">The <see cref="Point2D"/> to compare with this <see cref="Point2D"/>.</param>
        /// <returns><c>true</c> if both points hold the same coordinates; otherwise, <c>false</c>.</returns>
        public bool Equals(Point2D other)
            => X == other.X && Y == other.Y;

        /// <summary>
        /// Determines whether this <see cref="Point2D"/> is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="Point2D"/>.</param>
        /// <returns><c>true</c> if the object is a <see cref="Point2D"/> holding the same coordinates;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
            => obj is Point2D other && Equals(other);

        /// <summary>
        /// Adds the coordinates of a <see cref="Point2D"/> to those of another <see cref="Point2D"/>,
        /// yielding a new <see cref="Point2D"/>.
        /// </summary>
        /// <param name="source">The first <see cref="Point2D"/> to add.</param>
        /// <param name="other">The second <see cref="Point2D"/> to add.</param>
        /// <returns>The <see cref="Point2D"/> whose coordinates are the sum of both operands.</returns>
        public static Point2D operator +(Point2D source, Point2D other)
            => new(source.X + other.X, source.Y + other.Y);

        /// <summary>
        /// Subtracts the coordinates of a <see cref="Point2D"/> from those of another <see cref="Point2D"/>,
        /// yielding a new <see cref="Point2D"/>.
        /// </summary>
        /// <param name="source">The <see cref="Point2D"/> to subtract from.</param>
        /// <param name="other">The <see cref="Point2D"/> to subtract.</param>
        /// <returns>The <see cref="Point2D"/> whose coordinates are the difference of both operands.</returns>
        public static Point2D operator -(Point2D source, Point2D other)
            => new(source.X - other.X, source.Y - other.Y);

        /// <summary>
        /// Determines whether both <see cref="Point2D"/> instances hold the same coordinates.
        /// </summary>
        /// <param name="source">The first <see cref="Point2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Point2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Point2D source, Point2D other)
            => source.Equals(other);

        /// <summary>
        /// Determines whether both <see cref="Point2D"/> instances hold different coordinates.
        /// </summary>
        /// <param name="source">The first <see cref="Point2D"/> to compare.</param>
        /// <param name="other">The second <see cref="Point2D"/> to compare.</param>
        /// <returns><c>true</c> if both operands differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Point2D source, Point2D other)
            => !source.Equals(other);

        /// <summary>
        /// Serves as a hash function for a <see cref="Point2D"/> instance.
        /// </summary>
        /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}

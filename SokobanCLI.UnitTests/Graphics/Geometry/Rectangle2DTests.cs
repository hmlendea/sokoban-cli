using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UnitTests.Graphics.Geometry
{
    [TestFixture]
    public class Rectangle2DTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenZeroValues_WhenConstructingWithXYWidthHeight_ThenIsEmpty()
        {
            Rectangle2D rect = new(0, 0, 0, 0);

            Assert.That(rect.IsEmpty);
        }

        [Test]
        public void GivenNonZeroValues_WhenConstructing_ThenIsNotEmpty()
        {
            Rectangle2D rect = new(1, 2, 10, 5);

            Assert.That(rect.IsEmpty, Is.False);
        }

        [Test]
        public void GivenXYWidthHeight_WhenConstructing_ThenFieldsAreSet()
        {
            Rectangle2D rect = new(1, 2, 10, 5);

            Assert.That(rect.X, Is.EqualTo(1));
            Assert.That(rect.Y, Is.EqualTo(2));
            Assert.That(rect.Width, Is.EqualTo(10));
            Assert.That(rect.Height, Is.EqualTo(5));
        }

        [Test]
        public void GivenPointAndSize_WhenConstructing_ThenFieldsAreSet()
        {
            Point2D location = new(3, 4);
            Size2D size = new(12, 8);
            Rectangle2D rect = new(location, size);

            Assert.That(rect.X, Is.EqualTo(3));
            Assert.That(rect.Y, Is.EqualTo(4));
            Assert.That(rect.Width, Is.EqualTo(12));
            Assert.That(rect.Height, Is.EqualTo(8));
        }

        // ── Static properties ────────────────────────────────────

        [Test]
        public void GivenEmptyProperty_WhenAccessed_ThenReturnsZeroRectangle()
        {
            Rectangle2D empty = Rectangle2D.Empty;

            Assert.That(empty.X, Is.EqualTo(0));
            Assert.That(empty.Y, Is.EqualTo(0));
            Assert.That(empty.Width, Is.EqualTo(0));
            Assert.That(empty.Height, Is.EqualTo(0));
        }

        // ── Computed properties ──────────────────────────────────

        [Test]
        public void GivenRectangle_WhenAccessingLeft_ThenReturnsX()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Left, Is.EqualTo(3));
        }

        [Test]
        public void GivenRectangle_WhenAccessingTop_ThenReturnsY()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Top, Is.EqualTo(2));
        }

        [Test]
        public void GivenRectangle_WhenAccessingRight_ThenReturnsXPlusWidth()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Right, Is.EqualTo(13));
        }

        [Test]
        public void GivenRectangle_WhenAccessingBottom_ThenReturnsYPlusHeight()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Bottom, Is.EqualTo(7));
        }

        [Test]
        public void GivenRectangle_WhenAccessingLocation_ThenReturnsPointWithXAndY()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Location, Is.EqualTo(new Point2D(3, 2)));
        }

        [Test]
        public void GivenRectangle_WhenAccessingSize_ThenReturnsSizeWithWidthAndHeight()
        {
            Rectangle2D rect = new(3, 2, 10, 5);

            Assert.That(rect.Size, Is.EqualTo(new Size2D(10, 5)));
        }

        // ── Equals ──────────────────────────────────────────────

        [Test]
        public void GivenTwoRectanglesWithSameValues_WhenCallingEquals_ThenReturnsTrue()
        {
            Rectangle2D a = new(1, 2, 10, 5);
            Rectangle2D b = new(1, 2, 10, 5);

            Assert.That(a.Equals(b));
        }

        [Test]
        public void GivenTwoRectanglesWithDifferentValues_WhenCallingEquals_ThenReturnsFalse()
        {
            Rectangle2D a = new(1, 2, 10, 5);
            Rectangle2D b = new(3, 4, 8, 6);

            Assert.That(a.Equals(b), Is.False);
        }

        // ── Contains ────────────────────────────────────────────

        [Test]
        public void GivenCoordinatesInsideRectangle_WhenCallingContainsXY_ThenReturnsTrue()
        {
            Rectangle2D rect = new(0, 0, 10, 10);

            Assert.That(rect.Contains(5, 5));
        }

        [Test]
        public void GivenCoordinatesOnBorder_WhenCallingContainsXY_ThenReturnsTrue()
        {
            Rectangle2D rect = new(0, 0, 10, 10);

            Assert.That(rect.Contains(0, 0));
            Assert.That(rect.Contains(10, 10));
        }

        [Test]
        public void GivenCoordinatesOutsideRectangle_WhenCallingContainsXY_ThenReturnsFalse()
        {
            Rectangle2D rect = new(0, 0, 10, 10);

            Assert.That(rect.Contains(11, 5), Is.False);
            Assert.That(rect.Contains(5, 11), Is.False);
        }

        [Test]
        public void GivenPointInsideRectangle_WhenCallingContainsPoint_ThenReturnsTrue()
        {
            Rectangle2D rect = new(0, 0, 10, 10);
            Point2D point = new(5, 5);

            Assert.That(rect.Contains(point));
        }

        [Test]
        public void GivenPointOutsideRectangle_WhenCallingContainsPoint_ThenReturnsFalse()
        {
            Rectangle2D rect = new(0, 0, 10, 10);
            Point2D point = new(15, 5);

            Assert.That(rect.Contains(point), Is.False);
        }

        [Test]
        public void GivenRectangleFullyInside_WhenCallingContainsRectangle_ThenReturnsTrue()
        {
            Rectangle2D outer = new(0, 0, 20, 20);
            Rectangle2D inner = new(2, 2, 5, 5);

            Assert.That(outer.Contains(inner));
        }

        [Test]
        public void GivenRectanglePartiallyOutside_WhenCallingContainsRectangle_ThenReturnsFalse()
        {
            Rectangle2D outer = new(0, 0, 10, 10);
            Rectangle2D inner = new(5, 5, 10, 10);

            Assert.That(outer.Contains(inner), Is.False);
        }
    }
}

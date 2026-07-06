using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UnitTests.Graphics.Geometry
{
    [TestFixture]
    public class Size2DTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenZeroDimensions_WhenConstructing_ThenIsEmpty()
        {
            Size2D size = new(0, 0);

            Assert.That(size.IsEmpty);
        }

        [Test]
        public void GivenNonZeroDimensions_WhenConstructing_ThenIsNotEmpty()
        {
            Size2D size = new(10, 5);

            Assert.That(size.IsEmpty, Is.False);
        }

        [Test]
        public void GivenWidthAndHeight_WhenConstructing_ThenDimensionsAreSet()
        {
            Size2D size = new(10, 5);

            Assert.That(size.Width, Is.EqualTo(10));
            Assert.That(size.Height, Is.EqualTo(5));
        }

        [Test]
        public void GivenPoint2D_WhenConstructing_ThenDimensionsMatchXAndY()
        {
            Point2D point = new(7, 3);
            Size2D size = new(point);

            Assert.That(size.Width, Is.EqualTo(7));
            Assert.That(size.Height, Is.EqualTo(3));
        }

        // ── Empty ────────────────────────────────────────────────

        [Test]
        public void GivenEmptyProperty_WhenAccessed_ThenReturnsZeroSize()
        {
            Size2D empty = Size2D.Empty;

            Assert.That(empty.Width, Is.EqualTo(0));
            Assert.That(empty.Height, Is.EqualTo(0));
        }

        // ── Equals ──────────────────────────────────────────────

        [Test]
        public void GivenTwoSizesWithSameDimensions_WhenCallingEquals_ThenReturnsTrue()
        {
            Size2D a = new(10, 5);
            Size2D b = new(10, 5);

            Assert.That(a.Equals(b));
        }

        [Test]
        public void GivenTwoSizesWithDifferentDimensions_WhenCallingEquals_ThenReturnsFalse()
        {
            Size2D a = new(10, 5);
            Size2D b = new(2, 3);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void GivenTwoEqualSizes_WhenComparingWithEqualOperator_ThenReturnsTrue()
        {
            Size2D a = new(10, 5);
            Size2D b = new(10, 5);

            Assert.That(a == b);
        }

        [Test]
        public void GivenTwoDifferentSizes_WhenComparingWithNotEqualOperator_ThenReturnsTrue()
        {
            Size2D a = new(10, 5);
            Size2D b = new(2, 3);

            Assert.That(a != b);
        }

        // ── Operators ────────────────────────────────────────────

        [Test]
        public void GivenTwoSizes_WhenAdding_ThenReturnsSumSize()
        {
            Size2D a = new(3, 4);
            Size2D b = new(5, 6);

            Size2D result = a + b;

            Assert.That(result.Width, Is.EqualTo(8));
            Assert.That(result.Height, Is.EqualTo(10));
        }

        [Test]
        public void GivenTwoSizes_WhenSubtracting_ThenReturnsDifferenceSize()
        {
            Size2D a = new(10, 8);
            Size2D b = new(3, 5);

            Size2D result = a - b;

            Assert.That(result.Width, Is.EqualTo(7));
            Assert.That(result.Height, Is.EqualTo(3));
        }
    }
}

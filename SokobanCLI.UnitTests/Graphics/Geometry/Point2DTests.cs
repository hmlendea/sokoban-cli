using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UnitTests.Graphics.Geometry
{
    [TestFixture]
    public class Point2DTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenZeroCoordinates_WhenConstructing_ThenIsEmpty()
        {
            Point2D point = new(0, 0);

            Assert.That(point.IsEmpty);
        }

        [Test]
        public void GivenNonZeroCoordinates_WhenConstructing_ThenIsNotEmpty()
        {
            Point2D point = new(3, 7);

            Assert.That(point.IsEmpty, Is.False);
        }

        [Test]
        public void GivenXAndY_WhenConstructing_ThenCoordinatesAreSet()
        {
            Point2D point = new(5, 9);

            Assert.That(point.X, Is.EqualTo(5));
            Assert.That(point.Y, Is.EqualTo(9));
        }

        [Test]
        public void GivenSize2D_WhenConstructing_ThenCoordinatesMatchWidthAndHeight()
        {
            Size2D size = new(4, 8);
            Point2D point = new(size);

            Assert.That(point.X, Is.EqualTo(4));
            Assert.That(point.Y, Is.EqualTo(8));
        }

        // ── Empty ────────────────────────────────────────────────

        [Test]
        public void GivenEmptyProperty_WhenAccessed_ThenReturnsZeroPoint()
        {
            Point2D empty = Point2D.Empty;

            Assert.That(empty.X, Is.EqualTo(0));
            Assert.That(empty.Y, Is.EqualTo(0));
        }

        // ── Equals ──────────────────────────────────────────────

        [Test]
        public void GivenTwoPointsWithSameCoordinates_WhenCallingEquals_ThenReturnsTrue()
        {
            Point2D a = new(3, 7);
            Point2D b = new(3, 7);

            Assert.That(a.Equals(b));
        }

        [Test]
        public void GivenTwoPointsWithDifferentCoordinates_WhenCallingEquals_ThenReturnsFalse()
        {
            Point2D a = new(3, 7);
            Point2D b = new(1, 2);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void GivenTwoEqualPoints_WhenComparingWithEqualOperator_ThenReturnsTrue()
        {
            Point2D a = new(3, 7);
            Point2D b = new(3, 7);

            Assert.That(a == b);
        }

        [Test]
        public void GivenTwoDifferentPoints_WhenComparingWithNotEqualOperator_ThenReturnsTrue()
        {
            Point2D a = new(3, 7);
            Point2D b = new(1, 2);

            Assert.That(a != b);
        }

        // ── Operators ────────────────────────────────────────────

        [Test]
        public void GivenTwoPoints_WhenAdding_ThenReturnsSumPoint()
        {
            Point2D a = new(2, 3);
            Point2D b = new(4, 5);

            Point2D result = a + b;

            Assert.That(result.X, Is.EqualTo(6));
            Assert.That(result.Y, Is.EqualTo(8));
        }

        [Test]
        public void GivenTwoPoints_WhenSubtracting_ThenReturnsDifferencePoint()
        {
            Point2D a = new(6, 8);
            Point2D b = new(2, 3);

            Point2D result = a - b;

            Assert.That(result.X, Is.EqualTo(4));
            Assert.That(result.Y, Is.EqualTo(5));
        }
    }
}

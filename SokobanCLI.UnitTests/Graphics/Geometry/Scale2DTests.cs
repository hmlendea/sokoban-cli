using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UnitTests.Graphics.Geometry
{
    [TestFixture]
    public class Scale2DTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenZeroValues_WhenConstructing_ThenIsEmpty()
        {
            Scale2D scale = new(0f, 0f);

            Assert.That(scale.IsEmpty);
        }

        [Test]
        public void GivenNonZeroValues_WhenConstructing_ThenIsNotEmpty()
        {
            Scale2D scale = new(1f, 2f);

            Assert.That(scale.IsEmpty, Is.False);
        }

        [Test]
        public void GivenHorizontalAndVertical_WhenConstructing_ThenValuesAreSet()
        {
            Scale2D scale = new(1.5f, 2.5f);

            Assert.That(scale.Horizontal, Is.EqualTo(1.5f));
            Assert.That(scale.Vertical, Is.EqualTo(2.5f));
        }

        [Test]
        public void GivenPoint2D_WhenConstructing_ThenValuesMatchXAndY()
        {
            Point2D point = new(3, 4);
            Scale2D scale = new(point);

            Assert.That(scale.Horizontal, Is.EqualTo(3f));
            Assert.That(scale.Vertical, Is.EqualTo(4f));
        }

        [Test]
        public void GivenSize2D_WhenConstructing_ThenValuesMatchWidthAndHeight()
        {
            Size2D size = new(5, 7);
            Scale2D scale = new(size);

            Assert.That(scale.Horizontal, Is.EqualTo(5f));
            Assert.That(scale.Vertical, Is.EqualTo(7f));
        }

        // ── Static properties ────────────────────────────────────

        [Test]
        public void GivenEmptyProperty_WhenAccessed_ThenReturnsZeroScale()
        {
            Scale2D empty = Scale2D.Empty;

            Assert.That(empty.Horizontal, Is.EqualTo(0f));
            Assert.That(empty.Vertical, Is.EqualTo(0f));
        }

        [Test]
        public void GivenOneProperty_WhenAccessed_ThenReturnsOneScale()
        {
            Scale2D one = Scale2D.One;

            Assert.That(one.Horizontal, Is.EqualTo(1f));
            Assert.That(one.Vertical, Is.EqualTo(1f));
        }

        // ── Equals ──────────────────────────────────────────────

        [Test]
        public void GivenTwoScalesWithSameValues_WhenCallingEquals_ThenReturnsTrue()
        {
            Scale2D a = new(1.5f, 2.5f);
            Scale2D b = new(1.5f, 2.5f);

            Assert.That(a.Equals(b));
        }

        [Test]
        public void GivenTwoScalesWithDifferentValues_WhenCallingEquals_ThenReturnsFalse()
        {
            Scale2D a = new(1.5f, 2.5f);
            Scale2D b = new(3f, 4f);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void GivenTwoEqualScales_WhenComparingWithEqualOperator_ThenReturnsTrue()
        {
            Scale2D a = new(1.5f, 2.5f);
            Scale2D b = new(1.5f, 2.5f);

            Assert.That(a == b);
        }

        [Test]
        public void GivenTwoDifferentScales_WhenComparingWithNotEqualOperator_ThenReturnsTrue()
        {
            Scale2D a = new(1.5f, 2.5f);
            Scale2D b = new(3f, 4f);

            Assert.That(a != b);
        }

        // ── Operators ────────────────────────────────────────────

        [Test]
        public void GivenTwoScales_WhenAdding_ThenReturnsSumScale()
        {
            Scale2D a = new(1f, 2f);
            Scale2D b = new(3f, 4f);

            Scale2D result = a + b;

            Assert.That(result.Horizontal, Is.EqualTo(4f));
            Assert.That(result.Vertical, Is.EqualTo(6f));
        }

        [Test]
        public void GivenTwoScales_WhenSubtracting_ThenReturnsDifferenceScale()
        {
            Scale2D a = new(5f, 8f);
            Scale2D b = new(2f, 3f);

            Scale2D result = a - b;

            Assert.That(result.Horizontal, Is.EqualTo(3f));
            Assert.That(result.Vertical, Is.EqualTo(5f));
        }
    }
}

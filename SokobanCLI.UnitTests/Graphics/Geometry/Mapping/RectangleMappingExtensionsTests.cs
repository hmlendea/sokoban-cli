using System.Drawing;

using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Graphics.Geometry.Mapping;

namespace SokobanCLI.UnitTests.Graphics.Geometry.Mapping
{
    [TestFixture]
    public class RectangleMappingExtensionsTests
    {
        // ── ToRectangle2D ────────────────────────────────────────

        [Test]
        public void GivenSystemRectangle_WhenCallingToRectangle2D_ThenReturnsMatchingRectangle2D()
        {
            Rectangle systemRect = new(1, 2, 10, 5);

            Rectangle2D result = systemRect.ToRectangle2D();

            Assert.That(result.X, Is.EqualTo(1));
            Assert.That(result.Y, Is.EqualTo(2));
            Assert.That(result.Width, Is.EqualTo(10));
            Assert.That(result.Height, Is.EqualTo(5));
        }

        [Test]
        public void GivenZeroSystemRectangle_WhenCallingToRectangle2D_ThenReturnsEmptyRectangle2D()
        {
            Rectangle systemRect = new(0, 0, 0, 0);

            Rectangle2D result = systemRect.ToRectangle2D();

            Assert.That(result.IsEmpty);
        }

        // ── ToSystemRectangle ────────────────────────────────────

        [Test]
        public void GivenRectangle2D_WhenCallingToSystemRectangle_ThenReturnsMatchingSystemRectangle()
        {
            Rectangle2D rect = new(1, 2, 10, 5);

            Rectangle result = rect.ToSystemRectangle();

            Assert.That(result.X, Is.EqualTo(1));
            Assert.That(result.Y, Is.EqualTo(2));
            Assert.That(result.Width, Is.EqualTo(10));
            Assert.That(result.Height, Is.EqualTo(5));
        }

        [Test]
        public void GivenEmptyRectangle2D_WhenCallingToSystemRectangle_ThenReturnsZeroSystemRectangle()
        {
            Rectangle2D rect = Rectangle2D.Empty;

            Rectangle result = rect.ToSystemRectangle();

            Assert.That(result.X, Is.EqualTo(0));
            Assert.That(result.Y, Is.EqualTo(0));
            Assert.That(result.Width, Is.EqualTo(0));
            Assert.That(result.Height, Is.EqualTo(0));
        }
    }
}

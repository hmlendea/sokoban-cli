using System.Drawing;

using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Graphics.Geometry.Mapping;

namespace SokobanCLI.UnitTests.Graphics.Geometry.Mapping
{
    [TestFixture]
    public class PointMappingExtensionsTests
    {
        // ── ToPoint2D ────────────────────────────────────────────

        [Test]
        public void GivenSystemPoint_WhenCallingToPoint2D_ThenReturnsMatchingPoint2D()
        {
            Point systemPoint = new(5, 9);

            Point2D result = systemPoint.ToPoint2D();

            Assert.That(result.X, Is.EqualTo(5));
            Assert.That(result.Y, Is.EqualTo(9));
        }

        [Test]
        public void GivenZeroSystemPoint_WhenCallingToPoint2D_ThenReturnsEmptyPoint2D()
        {
            Point systemPoint = new(0, 0);

            Point2D result = systemPoint.ToPoint2D();

            Assert.That(result.IsEmpty);
        }

        // ── ToSystemPoint ────────────────────────────────────────

        [Test]
        public void GivenPoint2D_WhenCallingToSystemPoint_ThenReturnsMatchingSystemPoint()
        {
            Point2D point = new(5, 9);

            Point result = point.ToSystemPoint();

            Assert.That(result.X, Is.EqualTo(5));
            Assert.That(result.Y, Is.EqualTo(9));
        }

        [Test]
        public void GivenEmptyPoint2D_WhenCallingToSystemPoint_ThenReturnsZeroSystemPoint()
        {
            Point2D point = Point2D.Empty;

            Point result = point.ToSystemPoint();

            Assert.That(result.X, Is.EqualTo(0));
            Assert.That(result.Y, Is.EqualTo(0));
        }
    }
}

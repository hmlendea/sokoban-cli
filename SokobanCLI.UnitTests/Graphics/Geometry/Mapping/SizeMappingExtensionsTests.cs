using System.Drawing;

using NUnit.Framework;

using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Graphics.Geometry.Mapping;

namespace SokobanCLI.UnitTests.Graphics.Geometry.Mapping
{
    [TestFixture]
    public class SizeMappingExtensionsTests
    {
        // ── ToSize2D ─────────────────────────────────────────────

        [Test]
        public void GivenSystemSize_WhenCallingToSize2D_ThenReturnsMatchingSize2D()
        {
            Size systemSize = new(10, 6);

            Size2D result = systemSize.ToSize2D();

            Assert.That(result.Width, Is.EqualTo(10));
            Assert.That(result.Height, Is.EqualTo(6));
        }

        [Test]
        public void GivenZeroSystemSize_WhenCallingToSize2D_ThenReturnsEmptySize2D()
        {
            Size systemSize = new(0, 0);

            Size2D result = systemSize.ToSize2D();

            Assert.That(result.IsEmpty);
        }

        // ── ToSystemPoint ────────────────────────────────────────

        [Test]
        public void GivenSize2D_WhenCallingToSystemPoint_ThenReturnsMatchingSystemSize()
        {
            Size2D size = new(10, 6);

            Size result = size.ToSystemPoint();

            Assert.That(result.Width, Is.EqualTo(10));
            Assert.That(result.Height, Is.EqualTo(6));
        }

        [Test]
        public void GivenEmptySize2D_WhenCallingToSystemPoint_ThenReturnsZeroSystemSize()
        {
            Size2D size = Size2D.Empty;

            Size result = size.ToSystemPoint();

            Assert.That(result.Width, Is.EqualTo(0));
            Assert.That(result.Height, Is.EqualTo(0));
        }
    }
}

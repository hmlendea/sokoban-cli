using NUnit.Framework;

using SokobanCLI.GameLogic.Events;
using SokobanCLI.Graphics.Geometry;

namespace SokobanCLI.UnitTests.GameLogic.Events
{
    [TestFixture]
    public class PlayerMovementEventArgsTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenPlayerLocation_WhenConstructing_ThenPlayerLocationIsSet()
        {
            Point2D location = new(5, 7);

            PlayerMovementEventArgs args = new(location);

            Assert.That(args.PlayerLocation, Is.EqualTo(location));
        }

        [Test]
        public void GivenZeroLocation_WhenConstructing_ThenPlayerLocationIsEmpty()
        {
            Point2D location = Point2D.Empty;

            PlayerMovementEventArgs args = new(location);

            Assert.That(args.PlayerLocation.IsEmpty);
        }
    }
}

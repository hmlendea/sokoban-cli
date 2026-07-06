using System;

using NUnit.Framework;

using SokobanCLI.Input.Events;

namespace SokobanCLI.UnitTests.Input.Events
{
    [TestFixture]
    public class ConsoleKeyEventArgsTests
    {
        // ── Constructor ──────────────────────────────────────────

        [Test]
        public void GivenKey_WhenConstructing_ThenKeyIsSet()
        {
            ConsoleKeyEventArgs args = new(ConsoleKey.W);

            Assert.That(args.Key, Is.EqualTo(ConsoleKey.W));
        }

        [Test]
        public void GivenDifferentKey_WhenConstructing_ThenKeyMatchesGivenValue()
        {
            ConsoleKeyEventArgs args = new(ConsoleKey.Escape);

            Assert.That(args.Key, Is.EqualTo(ConsoleKey.Escape));
        }
    }
}

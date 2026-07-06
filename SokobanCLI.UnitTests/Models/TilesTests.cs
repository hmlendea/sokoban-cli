using System;

using NUnit.Framework;

using SokobanCLI.Models;

namespace SokobanCLI.UnitTests.Models
{
    [TestFixture]
    public class TilesTests
    {
        // ── ById ──────────────────────────────────────────────────

        [Test]
        public void GivenId0_WhenCallingById_ThenReturnsEmptyTile()
        {
            Tile tile = Tiles.ById(0);

            Assert.That(tile.Id, Is.EqualTo(0));
            Assert.That(tile.Character, Is.EqualTo(' '));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.Black));
        }

        [Test]
        public void GivenId1_WhenCallingById_ThenReturnsWallTile()
        {
            Tile tile = Tiles.ById(1);

            Assert.That(tile.Id, Is.EqualTo(1));
            Assert.That(tile.Character, Is.EqualTo('█'));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.Gray));
        }

        [Test]
        public void GivenId2_WhenCallingById_ThenReturnsBoxTile()
        {
            Tile tile = Tiles.ById(2);

            Assert.That(tile.Id, Is.EqualTo(2));
            Assert.That(tile.Character, Is.EqualTo('O'));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.DarkYellow));
        }

        [Test]
        public void GivenId3_WhenCallingById_ThenReturnsGoalTile()
        {
            Tile tile = Tiles.ById(3);

            Assert.That(tile.Id, Is.EqualTo(3));
            Assert.That(tile.Character, Is.EqualTo('+'));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.DarkRed));
        }

        [Test]
        public void GivenId4_WhenCallingById_ThenReturnsEmptyTile()
        {
            Tile tile = Tiles.ById(4);

            Assert.That(tile.Id, Is.EqualTo(4));
            Assert.That(tile.Character, Is.EqualTo(' '));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.Black));
        }

        [Test]
        public void GivenId5_WhenCallingById_ThenReturnsSolvedBoxTile()
        {
            Tile tile = Tiles.ById(5);

            Assert.That(tile.Id, Is.EqualTo(5));
            Assert.That(tile.Character, Is.EqualTo('@'));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.DarkGreen));
        }

        [Test]
        public void GivenUnknownId_WhenCallingById_ThenReturnsEmptyTile()
        {
            Tile tile = Tiles.ById(99);

            Assert.That(tile.Id, Is.EqualTo(99));
            Assert.That(tile.Character, Is.EqualTo(' '));
            Assert.That(tile.Colour, Is.EqualTo(ConsoleColor.Black));
        }
    }
}

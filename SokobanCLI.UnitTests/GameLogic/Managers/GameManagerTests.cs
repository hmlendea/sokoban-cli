using Moq;

using NUnit.Framework;

using SokobanCLI.GameLogic.Events;
using SokobanCLI.GameLogic.Managers;
using SokobanCLI.Graphics.Geometry;
using SokobanCLI.Models;

namespace SokobanCLI.UnitTests.GameLogic.Managers
{
    // Level 0 layout (rows[x][y] → Tiles[x, y]):
    //   Player start: (7, 5)   Boxes: (6, 6) and (7, 7)   Goals: (6, 8) and (7, 8)
    //   Walls surround the play area (rows 5 and 8, columns 4 and 9 of the room)

    [TestFixture]
    public class GameManagerTests
    {
        GameManager game;

        [SetUp]
        public void SetUp()
        {
            game = new GameManager();
            game.PlayerMoved = delegate { };
            game.Start(0);
        }

        // ── Start ────────────────────────────────────────────────

        [Test]
        public void GivenLevel_WhenStarting_ThenIsRunningIsTrue()
        {
            Assert.That(game.IsRunning);
        }

        [Test]
        public void GivenLevel_WhenStarting_ThenWorldIsNotNull()
        {
            Assert.That(game.World, Is.Not.Null);
        }

        [Test]
        public void GivenLevel_WhenStarting_ThenMovesCountIsZero()
        {
            Assert.That(game.World.Moves, Is.EqualTo(0));
        }

        // ── Stop ─────────────────────────────────────────────────

        [Test]
        public void GivenRunningGame_WhenStopping_ThenIsRunningIsFalse()
        {
            game.Stop();

            Assert.That(game.IsRunning, Is.False);
        }

        // ── MovePlayer – no movement ─────────────────────────────

        [Test]
        public void GivenZeroDeltas_WhenMovingPlayer_ThenPlayerPositionDoesNotChange()
        {
            Point2D positionBefore = game.World.PlayerPosition;

            game.MovePlayer(0, 0);

            Assert.That(game.World.PlayerPosition, Is.EqualTo(positionBefore));
        }

        [Test]
        public void GivenZeroDeltas_WhenMovingPlayer_ThenMovesCountDoesNotChange()
        {
            game.MovePlayer(0, 0);

            Assert.That(game.World.Moves, Is.EqualTo(0));
        }

        [Test]
        public void GivenWallAhead_WhenMovingPlayer_ThenPlayerPositionDoesNotChange()
        {
            Point2D positionBefore = game.World.PlayerPosition;

            game.MovePlayer(0, -1); // tile (7, 4) is a wall

            Assert.That(game.World.PlayerPosition, Is.EqualTo(positionBefore));
        }

        [Test]
        public void GivenWallAhead_WhenMovingPlayer_ThenMovesCountDoesNotChange()
        {
            game.MovePlayer(0, -1); // tile (7, 4) is a wall

            Assert.That(game.World.Moves, Is.EqualTo(0));
        }

        // ── MovePlayer – player movement ─────────────────────────

        [Test]
        public void GivenEmptyTileAhead_WhenMovingPlayer_ThenPlayerPositionIsUpdated()
        {
            game.MovePlayer(0, 1); // tile (7, 6) is empty

            Assert.That(game.World.PlayerPosition, Is.EqualTo(new Point2D(7, 6)));
        }

        [Test]
        public void GivenEmptyTileAhead_WhenMovingPlayer_ThenMovesCountIsIncremented()
        {
            game.MovePlayer(0, 1); // tile (7, 6) is empty

            Assert.That(game.World.Moves, Is.EqualTo(1));
        }

        [Test]
        public void GivenGoalTileAhead_WhenMovingPlayer_ThenPlayerPositionIsUpdated()
        {
            game.World.Tiles[7, 6] = Tiles.ById(3); // place a goal directly ahead

            game.MovePlayer(0, 1);

            Assert.That(game.World.PlayerPosition, Is.EqualTo(new Point2D(7, 6)));
        }

        [Test]
        public void GivenGoalTileAhead_WhenMovingPlayer_ThenMovesCountIsIncremented()
        {
            game.World.Tiles[7, 6] = Tiles.ById(3); // place a goal directly ahead

            game.MovePlayer(0, 1);

            Assert.That(game.World.Moves, Is.EqualTo(1));
        }

        // ── MovePlayer – box pushing ─────────────────────────────

        [Test]
        public void GivenBoxWithEmptyTileBehind_WhenMovingPlayer_ThenBoxIsMoved()
        {
            game.World.Tiles[7, 6] = Tiles.ById(2); // box ahead of player
            game.World.Tiles[7, 7] = Tiles.ById(0); // empty tile behind box

            game.MovePlayer(0, 1);

            Assert.That(game.World.Tiles[7, 6].Id, Is.EqualTo(0)); // vacated
            Assert.That(game.World.Tiles[7, 7].Id, Is.EqualTo(2)); // box moved here
        }

        [Test]
        public void GivenBoxWithEmptyTileBehind_WhenMovingPlayer_ThenPlayerPositionIsUpdated()
        {
            game.World.Tiles[7, 6] = Tiles.ById(2);
            game.World.Tiles[7, 7] = Tiles.ById(0);

            game.MovePlayer(0, 1);

            Assert.That(game.World.PlayerPosition, Is.EqualTo(new Point2D(7, 6)));
        }

        [Test]
        public void GivenBoxWithGoalBehind_WhenMovingPlayer_ThenBoxBecomesBoxOnGoal()
        {
            // Move player from (7,5) to (7,6) first
            game.MovePlayer(0, 1);
            // Now push box at (7,7) toward goal at (7,8)
            game.MovePlayer(0, 1);

            Assert.That(game.World.Tiles[7, 8].Id, Is.EqualTo(5)); // boxOnGoal
            Assert.That(game.World.Tiles[7, 7].Id, Is.EqualTo(0)); // vacated
        }

        [Test]
        public void GivenBoxWithWallBehind_WhenMovingPlayer_ThenPlayerPositionDoesNotChange()
        {
            game.World.Tiles[7, 6] = Tiles.ById(2); // box ahead
            game.World.Tiles[7, 7] = Tiles.ById(1); // wall behind box

            Point2D positionBefore = game.World.PlayerPosition;

            game.MovePlayer(0, 1);

            Assert.That(game.World.PlayerPosition, Is.EqualTo(positionBefore));
        }

        [Test]
        public void GivenBoxOnGoalWithEmptyTileBehind_WhenMovingPlayer_ThenBoxIsMovedToEmptyTile()
        {
            game.World.Tiles[7, 6] = Tiles.ById(5); // boxOnGoal ahead
            game.World.Tiles[7, 7] = Tiles.ById(0); // empty behind

            game.MovePlayer(0, 1);

            Assert.That(game.World.Tiles[7, 7].Id, Is.EqualTo(2)); // box moved to empty
        }

        [Test]
        public void GivenBoxOnGoalWithEmptyTileBehind_WhenMovingPlayer_ThenGoalRemainsAtOriginalPosition()
        {
            game.World.Tiles[7, 6] = Tiles.ById(5); // boxOnGoal ahead
            game.World.Tiles[7, 7] = Tiles.ById(0); // empty behind

            game.MovePlayer(0, 1);

            Assert.That(game.World.Tiles[7, 6].Id, Is.EqualTo(3)); // goal remains
        }

        [Test]
        public void GivenBoxOnGoalWithGoalBehind_WhenMovingPlayer_ThenBoxBecomesBoxOnGoal()
        {
            game.World.Tiles[7, 6] = Tiles.ById(5); // boxOnGoal ahead
            game.World.Tiles[7, 7] = Tiles.ById(3); // goal behind

            game.MovePlayer(0, 1);

            Assert.That(game.World.Tiles[7, 7].Id, Is.EqualTo(5)); // boxOnGoal pushed to goal
            Assert.That(game.World.Tiles[7, 6].Id, Is.EqualTo(3)); // original goal restored
        }

        [Test]
        public void GivenBoxOnGoalWithWallBehind_WhenMovingPlayer_ThenPlayerPositionDoesNotChange()
        {
            game.World.Tiles[7, 6] = Tiles.ById(5); // boxOnGoal ahead
            game.World.Tiles[7, 7] = Tiles.ById(1); // wall behind

            Point2D positionBefore = game.World.PlayerPosition;

            game.MovePlayer(0, 1);

            Assert.That(game.World.PlayerPosition, Is.EqualTo(positionBefore));
        }

        // ── MovePlayer – event ───────────────────────────────────

        [Test]
        public void GivenEmptyTileAhead_WhenMovingPlayer_ThenPlayerMovedEventIsFired()
        {
            Mock<IPlayerMovedSpy> spy = new();
            game.PlayerMoved += spy.Object.OnPlayerMoved;

            game.MovePlayer(0, 1); // tile (7, 6) is empty

            spy.Verify(
                s => s.OnPlayerMoved(
                    game,
                    It.Is<PlayerMovementEventArgs>(a =>
                        a.PlayerLocation.Equals(new Point2D(7, 6)))),
                Times.Once);
        }

        // ── GetCompletion ────────────────────────────────────────

        [Test]
        public void GivenRemainingGoalTiles_WhenGettingCompletion_ThenReturnsFalse()
        {
            bool result = game.GetCompletion();

            Assert.That(result, Is.False);
        }

        [Test]
        public void GivenNoRemainingGoalTiles_WhenGettingCompletion_ThenReturnsTrue()
        {
            // Replace both uncovered goals in level 0 with boxOnGoal
            game.World.Tiles[6, 8] = Tiles.ById(5);
            game.World.Tiles[7, 8] = Tiles.ById(5);

            bool result = game.GetCompletion();

            Assert.That(result);
        }
    }
}

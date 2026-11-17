using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class ExitPathTests
    {
        static readonly Vector2Int[] L = { new(0, 0), new(1, 0), new(0, 1) };
        static readonly Vector2Int Board = new(5, 6);

        [Test]
        public void ExitPath_ShouldMeasureLengthAndDoorSpan_WhenLeavingLeft()
        {
            var path = new ExitPath(L, new Vector2Int(0, 2), Vector2Int.left, Board);

            Assert.That(path.Side, Is.EqualTo(WallSide.Left));
            Assert.That(path.Axis, Is.Zero);
            Assert.That(path.Length, Is.EqualTo(2));
            Assert.That(path.CrossMin, Is.EqualTo(2));
            Assert.That(path.CrossMax, Is.EqualTo(3));
            Assert.That(path.Width, Is.EqualTo(2));
            Assert.That(path.Edge, Is.Zero);
            Assert.That(path.DoorCentre(0.3f), Is.EqualTo(new Vector3(0f, 0.3f, 3f)));
        }

        [Test]
        public void ExitPath_ShouldUseTheFarEdge_WhenLeavingTop()
        {
            var path = new ExitPath(L, new Vector2Int(3, 4), Vector2Int.up, Board);

            Assert.That(path.Side, Is.EqualTo(WallSide.Top));
            Assert.That(path.Axis, Is.EqualTo(1));
            Assert.That(path.Length, Is.EqualTo(2));
            Assert.That(path.CrossMin, Is.EqualTo(3));
            Assert.That(path.CrossMax, Is.EqualTo(4));
            Assert.That(path.Edge, Is.EqualTo(6));
            Assert.That(path.DoorCentre(0f), Is.EqualTo(new Vector3(4f, 0f, 6f)));
        }
    }
}

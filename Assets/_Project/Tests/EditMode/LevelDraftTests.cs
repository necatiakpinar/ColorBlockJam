using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Editor.LevelEditing;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelDraftTests
    {
        static List<Vector2Int> Cells(params int[] xy)
        {
            var cells = new List<Vector2Int>();
            for (int i = 0; i < xy.Length; i += 2)
                cells.Add(new Vector2Int(xy[i], xy[i + 1]));
            return cells;
        }

        static LevelDraft Empty(int width = 5, int height = 5) => new() { Width = width, Height = height };

        [Test]
        public void Draft_ShouldRoundTripALevel_WhenReadAndWritten()
        {
            LevelData level = new LevelDataBuilder(4, 5).Timer(90).Difficulty(LevelDifficulty.Hard)
                .Block(Red, 0, 0, 1, 0).Door(Left, 0, 2, Red).Build();

            LevelData back = LevelDraft.From(level).ToLevel();

            Assert.That(JsonUtility.ToJson(back), Is.EqualTo(JsonUtility.ToJson(level)));
        }

        [Test]
        public void AddBlock_ShouldRefuse_WhenItOverlapsLeavesTheBoardOrIsInPieces()
        {
            LevelDraft draft = Empty();
            Assert.That(draft.TryAddBlock(Red, Cells(0, 0, 1, 0)), Is.True);

            Assert.That(draft.TryAddBlock(Blue, Cells(1, 0, 2, 0)), Is.False, "Overlap");
            Assert.That(draft.TryAddBlock(Blue, Cells(4, 0, 5, 0)), Is.False, "Off the board");
            Assert.That(draft.TryAddBlock(Blue, Cells(3, 3, 4, 4)), Is.False, "Not connected");
            Assert.That(draft.Blocks.Count, Is.EqualTo(1));
        }

        [Test]
        public void MoveBlock_ShouldMoveOrRefuse_WhenTheWayIsFreeOrNot()
        {
            LevelDraft draft = Empty();
            draft.TryAddBlock(Red, Cells(0, 0));
            draft.TryAddBlock(Blue, Cells(2, 0));

            Assert.That(draft.TryMoveBlock(0, Vector2Int.right), Is.True);
            Assert.That(draft.TryMoveBlock(0, Vector2Int.right), Is.False, "Blue is there.");
            Assert.That(draft.TryMoveBlock(0, Vector2Int.down), Is.False, "Off the board.");
            Assert.That(draft.Blocks[0].Cells, Is.EqualTo(Cells(1, 0)));
        }

        [Test]
        public void RotateBlock_ShouldTurnAQuarterAndStayOnTheBoard_WhenThereIsRoom()
        {
            LevelDraft draft = Empty();
            draft.TryAddBlock(Red, Cells(0, 0, 1, 0, 2, 0));

            Assert.That(draft.TryRotateBlock(0), Is.True);

            var cells = new HashSet<Vector2Int>(draft.Blocks[0].Cells);
            Assert.That(cells, Is.EquivalentTo(Cells(0, 0, 0, 1, 0, 2)));
        }

        [Test]
        public void Doors_ShouldStayOnTheirWallAndApart_WhenAddedOrResized()
        {
            LevelDraft draft = Empty(4, 3);

            Assert.That(draft.TryAddDoor(Bottom, 0, 2, Red), Is.True);
            Assert.That(draft.TryAddDoor(Bottom, 1, 1, Blue), Is.False, "Overlaps the red door.");
            Assert.That(draft.TryAddDoor(Left, 2, 2, Blue), Is.False, "Runs past the left wall (3 rows).");
            Assert.That(draft.TryAddDoor(Bottom, 2, 2, Blue), Is.True);
            Assert.That(draft.TrySetDoorSpan(0, 0, 3), Is.False, "Would cover the blue door.");
            Assert.That(draft.TrySetDoorSpan(0, 0, 1), Is.True);
            Assert.That(draft.DoorAt(Bottom, 1), Is.EqualTo(-1));
        }

        [Test]
        public void Resize_ShouldRemoveWhatNoLongerFitsAndSayWhat_WhenShrunk()
        {
            LevelDraft draft = Empty(5, 5);
            draft.TryAddBlock(Red, Cells(4, 4));
            draft.TryAddBlock(Blue, Cells(0, 0));
            draft.TryAddDoor(Top, 3, 2, Red);
            draft.TryAddDoor(Left, 0, 1, Blue);

            (int Blocks, int Doors) preview = draft.WouldLoseOnResize(4, 4);
            (int Blocks, int Doors) removed = draft.Resize(4, 4);

            Assert.That(preview, Is.EqualTo((1, 1)));
            Assert.That(removed, Is.EqualTo((1, 1)));
            Assert.That(draft.Blocks.Count, Is.EqualTo(1));
            Assert.That(draft.Doors.Count, Is.EqualTo(1));
        }
    }
}

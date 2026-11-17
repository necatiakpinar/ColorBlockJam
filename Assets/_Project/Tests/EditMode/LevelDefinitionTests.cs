using NUnit.Framework;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The level asset keeps every field through Unity serialization, the path the editor and game share.</summary>
    public sealed class LevelDefinitionTests
    {
        [Test]
        public void LevelData_ShouldSurviveSerialization_WhenCopiedThroughTheAsset()
        {
            var source = ScriptableObject.CreateInstance<LevelDefinition>();
            var copy = ScriptableObject.CreateInstance<LevelDefinition>();
            source.SetData(new LevelDataBuilder(5, 6).Timer(90)
                .Block(Yellow, 0, 0, 1, 0, 1, 1)
                .Block(Cyan, 4, 5)
                .Door(Right, 2, 3, Cyan)
                .Build());

            EditorUtility.CopySerialized(source, copy);
            LevelData level = copy.Data;

            Assert.That((level.Width, level.Height, level.TimerSeconds), Is.EqualTo((5, 6, 90)));
            Assert.That(level.Blocks.Count, Is.EqualTo(2));
            Assert.That(level.Blocks[0].Color, Is.EqualTo(Yellow));
            Assert.That(level.Blocks[0].Cells, Is.EqualTo(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) }));
            Assert.That(level.Doors.Count, Is.EqualTo(1));
            Assert.That((level.Doors[0].Side, level.Doors[0].Start, level.Doors[0].Length, level.Doors[0].Color),
                Is.EqualTo((Right, 2, 3, Cyan)));

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(copy);
        }

        [Test]
        public void WallLength_ShouldFollowTheWallAxis_WhenBoardIsNotSquare()
        {
            LevelData level = new LevelDataBuilder(5, 7).Build();

            Assert.That(level.WallLength(Bottom), Is.EqualTo(5));
            Assert.That(level.WallLength(Top), Is.EqualTo(5));
            Assert.That(level.WallLength(Left), Is.EqualTo(7));
            Assert.That(level.WallLength(Right), Is.EqualTo(7));
        }
    }
}

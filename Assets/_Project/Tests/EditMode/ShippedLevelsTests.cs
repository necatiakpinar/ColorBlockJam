using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Levels;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>
    /// Every level in the shipped catalog is valid and can be finished. Solvability is checked with the real
    /// <see cref="BoardState"/> rules: blocks are taken out one at a time, each one searching every cell it can
    /// slide to with the others in place. Clearing a level that way proves it solvable.
    /// </summary>
    public sealed class ShippedLevelsTests
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";
        const string RulesPath = "Assets/_Project/Configs/LevelRulesConfig.asset";
        static readonly Vector2Int[] Steps = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

        static IReadOnlyList<LevelDefinition> Levels() =>
            AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath).Levels;

        [Test]
        public void Catalog_ShouldShipFiveLevels_WhenBuilt()
        {
            Assert.That(Levels().Count, Is.EqualTo(5));
        }

        [Test]
        public void ShippedLevels_ShouldPassValidationWithoutIssues_WhenChecked()
        {
            var validator = new LevelDefinitionValidator(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath));
            for (int i = 0; i < Levels().Count; i++)
                Assert.That(validator.Validate(Levels()[i].Data), Is.Empty, $"Level {i + 1}");
        }

        [Test]
        public void ShippedLevels_ShouldBeSolvable_WhenBlocksLeaveOneByOne()
        {
            for (int i = 0; i < Levels().Count; i++)
                Assert.That(CanClear(new BoardState(Levels()[i].Data)), Is.True, $"Level {i + 1}");
        }

        [Test]
        public void ShippedLevels_ShouldGetHarder_WhenTheLevelNumberRises()
        {
            int previous = 0;
            for (int i = 0; i < Levels().Count; i++)
            {
                int blocks = Levels()[i].Data.Blocks.Count;
                Assert.That(blocks, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1} has fewer blocks than the one before.");
                previous = blocks;
            }
        }

        static bool CanClear(BoardState board)
        {
            while (board.RemainingBlocks > 0)
            {
                bool progress = false;
                for (int id = 0; id < board.Blocks.Count; id++)
                    if (!board.Blocks[id].HasExited && TryTakeOut(board, id, new HashSet<Vector2Int>()))
                        progress = true;
                if (!progress)
                    return false;
            }

            return true;
        }

        /// <summary>Slides the block through every free cell it can reach; leaves it out if any reaches its door.</summary>
        static bool TryTakeOut(BoardState board, int id, HashSet<Vector2Int> visited)
        {
            visited.Add(board.Blocks[id].Offset);
            foreach (Vector2Int step in Steps)
            {
                StepResult result = board.CanStep(id, step);
                if (result == StepResult.Exited)
                {
                    board.TryStep(id, step);
                    return true;
                }

                if (result != StepResult.Moved || visited.Contains(board.Blocks[id].Offset + step))
                    continue;
                board.TryStep(id, step);
                if (TryTakeOut(board, id, visited))
                    return true;
                board.TryStep(id, -step);
            }

            return false;
        }
    }
}

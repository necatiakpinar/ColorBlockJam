using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Solving;
using UnityEditor;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>
    /// Every level in the shipped catalog is valid, can be finished (proved by <see cref="LevelSolver"/> on the real
    /// board rules) and ramps up: more blocks, a higher solver score, and from level 3 on blocks that must be moved
    /// out of the way first (T-32, level-data.md §3).
    /// </summary>
    public sealed class ShippedLevelsTests
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";
        const string RulesPath = "Assets/_Project/Configs/LevelRulesConfig.asset";

        static IReadOnlyList<LevelDefinition> Levels() =>
            AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath).Levels;

        [Test]
        public void Catalog_ShouldShipEightLevels_WhenBuilt()
        {
            Assert.That(Levels().Count, Is.EqualTo(8));
        }

        [Test]
        public void ShippedLevels_ShouldIncludeArrowBlocks_WhenLevelsSevenAndEightArePlayed()
        {
            // Levels 7 and 8 show the arrow blocker (T-35): 2 and 3 of their blocks move along one axis only.
            int[] expected = { 2, 3 };
            for (int k = 0; k < expected.Length; k++)
            {
                int arrows = 0;
                foreach (BlockData block in Levels()[6 + k].Data.Blocks)
                    if (block.Blockers.Arrow.IsActive)
                        arrows++;
                Assert.That(arrows, Is.EqualTo(expected[k]), $"Level {7 + k}");
            }
        }

        [Test]
        public void ShippedLevels_ShouldIncludeFrozenBlocks_WhenLevelSixIsPlayed()
        {
            // Level 6 shows the ice blocker (T-34): two of its blocks wait for others to leave.
            int frozen = 0;
            foreach (BlockData block in Levels()[5].Data.Blocks)
                if (block.Blockers.Ice.IsActive)
                    frozen++;

            Assert.That(frozen, Is.EqualTo(2));
        }

        [Test]
        public void ShippedLevels_ShouldPassValidationWithoutIssues_WhenChecked()
        {
            var validator = new LevelDefinitionValidator(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath));
            for (int i = 0; i < Levels().Count; i++)
                Assert.That(validator.Validate(Levels()[i].Data), Is.Empty, $"Level {i + 1}");
        }

        [Test]
        public void ShippedLevels_ShouldNeverGetEasierInDifficulty_WhenTheLevelNumberRises()
        {
            LevelDifficulty previous = LevelDifficulty.Normal;
            for (int i = 0; i < Levels().Count; i++)
            {
                LevelDifficulty difficulty = Levels()[i].Data.Difficulty;
                Assert.That(difficulty, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1}");
                previous = difficulty;
            }

            Assert.That(previous, Is.EqualTo(LevelDifficulty.VeryHard), "The last level is the hardest kind.");
        }

        [Test]
        public void ShippedLevels_ShouldPlayAsHardAsTheirDifficultySays_WhenSolved()
        {
            var rules = AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath);
            var solver = new LevelSolver(rules.SolverStateCap);
            var generator = new LevelGenerator(new LevelDefinitionValidator(rules), rules.SolverStateCap, new BlockShapeLibrary());
            for (int i = 0; i < Levels().Count; i++)
            {
                LevelData level = Levels()[i].Data;
                SolveResult result = solver.Solve(level);
                (int min, int max) = generator.ScoreFor(level.Difficulty);
                Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable), $"Level {i + 1}");
                Assert.That(result.Score, Is.InRange(min, max), $"Level {i + 1} is labelled {level.Difficulty}");
            }
        }

        [Test]
        public void ShippedLevels_ShouldScoreHigher_WhenTheLevelNumberRises()
        {
            var solver = new LevelSolver(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath).SolverStateCap);
            int previous = 0;
            for (int i = 0; i < Levels().Count; i++)
            {
                int score = solver.Solve(Levels()[i].Data).Score;
                Assert.That(score, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1} plays easier than the one before.");
                previous = score;
            }
        }

        [Test]
        public void ShippedLevels_ShouldNeedRoomMadeFirst_WhenFromLevelThreeOn()
        {
            var solver = new LevelSolver(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath).SolverStateCap);
            for (int i = 2; i < Levels().Count; i++)
                Assert.That(solver.Solve(Levels()[i].Data).Moves, Is.GreaterThanOrEqualTo(1), $"Level {i + 1}");
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
    }
}

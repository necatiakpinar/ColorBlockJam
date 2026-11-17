using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Game.Levels.Solving;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>
    /// Every level in the shipped catalog is valid and can be finished (proved by <see cref="LevelSolver"/> on the
    /// real board rules). Levels 1–8 ramp up gently: more blocks, a higher solver score, and from level 3 on blocks
    /// that must be moved out of the way first. From level 9 they come in groups of six with a breather and a peak
    /// that needs more room-making moves than the last group's (T-32, T-49, DA-40, level-data.md §3).
    /// </summary>
    public sealed class ShippedLevelsTests
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";
        const string RulesPath = "Assets/_Project/Configs/LevelRulesConfig.asset";
        const string PackConfigPath = "Assets/_Project/Configs/LevelPackConfig.asset";

        const int RampLevels = 8;
        const int GroupSize = 6;
        const int FinalPeakMoves = 8;

        static IReadOnlyList<LevelDefinition> Levels() =>
            AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath).Levels;

        static int[] MovesOfEveryLevel()
        {
            var solver = new LevelSolver(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath).SolverStateCap);
            var moves = new int[Levels().Count];
            for (int i = 0; i < moves.Length; i++)
                moves[i] = solver.Solve(Levels()[i].Data).Moves;
            return moves;
        }

        /// <summary>A Hard level after the ramp gives the player a rest between peaks.</summary>
        static bool IsBreather(int index) => index >= RampLevels && Levels()[index].Data.Difficulty == LevelDifficulty.Hard;

        [Test]
        public void BundledPack_ShouldHoldTheCatalogsLevels_WhenTheGameShips()
        {
            // The game plays levels.json; the editor rewrites it on every catalog change (level-packs.md).
            TextAsset bundled = AssetDatabase.LoadAssetAtPath<LevelPackConfig>(PackConfigPath).BundledPack;

            Assert.That(new LevelPackJson().TryRead(bundled.text, out LevelPack pack, out string error), Is.True, error);
            Assert.That(pack.Levels, Has.Count.EqualTo(Levels().Count));
            for (int i = 0; i < pack.Levels.Count; i++)
                Assert.That(JsonUtility.ToJson(pack.Levels[i]), Is.EqualTo(JsonUtility.ToJson(Levels()[i].Data)), $"Level {i + 1}");
        }

        [Test]
        public void Catalog_ShouldShipTwentyLevels_WhenBuilt()
        {
            Assert.That(Levels().Count, Is.EqualTo(20));
        }

        [Test]
        public void ShippedLevels_ShouldPeakAtTheEndOfEachGroup_WhenFromLevelNineOn()
        {
            int[] moves = MovesOfEveryLevel();
            int previousPeak = 0;
            for (int first = RampLevels; first < moves.Length; first += GroupSize)
            {
                int last = System.Math.Min(first + GroupSize, moves.Length) - 1;
                bool hasBreather = false;
                for (int i = first; i <= last; i++)
                {
                    Assert.That(moves[i], Is.LessThanOrEqualTo(moves[last]), $"Level {i + 1} is harder than its group's peak.");
                    hasBreather |= IsBreather(i);
                }

                Assert.That(hasBreather, Is.True, $"Levels {first + 1}–{last + 1} have no breather.");
                Assert.That(moves[last], Is.GreaterThan(previousPeak), $"Level {last + 1} peaks no higher than the group before.");
                previousPeak = moves[last];
            }

            Assert.That(previousPeak, Is.GreaterThanOrEqualTo(FinalPeakMoves), "The last peak is a real puzzle.");
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
        public void ShippedLevels_ShouldNeverGetEasierInDifficulty_WhenTheRampIsPlayed()
        {
            LevelDifficulty previous = LevelDifficulty.Normal;
            for (int i = 0; i < RampLevels; i++)
            {
                LevelDifficulty difficulty = Levels()[i].Data.Difficulty;
                Assert.That(difficulty, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1}");
                previous = difficulty;
            }

            Assert.That(Levels()[Levels().Count - 1].Data.Difficulty, Is.EqualTo(LevelDifficulty.VeryHard), "The last level is the hardest kind.");
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
        public void ShippedLevels_ShouldScoreHigher_WhenTheRampIsPlayed()
        {
            var solver = new LevelSolver(AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath).SolverStateCap);
            int previous = 0;
            for (int i = 0; i < RampLevels; i++)
            {
                int score = solver.Solve(Levels()[i].Data).Score;
                Assert.That(score, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1} plays easier than the one before.");
                previous = score;
            }
        }

        [Test]
        public void ShippedLevels_ShouldNeedRoomMadeFirst_WhenFromLevelThreeOnButABreather()
        {
            int[] moves = MovesOfEveryLevel();
            for (int i = 2; i < moves.Length; i++)
                if (!IsBreather(i))
                    Assert.That(moves[i], Is.GreaterThanOrEqualTo(1), $"Level {i + 1}");
        }

        [Test]
        public void ShippedLevels_ShouldGetMoreBlocks_WhenTheRampIsPlayed()
        {
            int previous = 0;
            for (int i = 0; i < RampLevels; i++)
            {
                int blocks = Levels()[i].Data.Blocks.Count;
                Assert.That(blocks, Is.GreaterThanOrEqualTo(previous), $"Level {i + 1} has fewer blocks than the one before.");
                previous = blocks;
            }
        }
    }
}

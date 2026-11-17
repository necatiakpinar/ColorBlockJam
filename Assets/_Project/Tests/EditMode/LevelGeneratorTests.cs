using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Solving;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelGeneratorTests
    {
        LevelRulesConfig _rules;
        LevelDefinitionValidator _validator;
        LevelGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            _validator = new LevelDefinitionValidator(_rules);
            _generator = new LevelGenerator(_validator, 2000, new BlockShapeLibrary());
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_rules);

        static LevelGenerationRequest Request(LevelDifficulty difficulty, int seed) =>
            new(6, 7, 4, 7, difficulty, seed, 300);

        [TestCase(LevelDifficulty.Normal)]
        [TestCase(LevelDifficulty.Hard)]
        [TestCase(LevelDifficulty.VeryHard)]
        public void Generate_ShouldGiveAValidSolvableLevelInTheDifficultysBand_WhenAsked(LevelDifficulty difficulty)
        {
            LevelGenerationResult result = _generator.Generate(Request(difficulty, 11));

            Assert.That(result.HasLevel, Is.True, result.Failure);
            Assert.That(_validator.Validate(result.Level), Is.Empty);
            Assert.That(new LevelSolver(20000).Solve(result.Level).Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.MatchesDifficulty, Is.True);
            (int min, int max) = _generator.ScoreFor(difficulty);
            Assert.That(result.Solution.Score, Is.InRange(min, max));
            Assert.That(result.Level.Difficulty, Is.EqualTo(difficulty));
        }

        [Test]
        public void Generate_ShouldRespectTheSettings_WhenAsked()
        {
            LevelData level = _generator.Generate(Request(LevelDifficulty.Hard, 5)).Level;

            Assert.That(level.Width, Is.EqualTo(6));
            Assert.That(level.Height, Is.EqualTo(7));
            Assert.That(level.Blocks.Count, Is.EqualTo(7));
            var colors = new HashSet<BlockColor>();
            foreach (BlockData block in level.Blocks)
                colors.Add(block.Color);
            Assert.That(colors.Count, Is.EqualTo(4));
            Assert.That(level.Doors.Count, Is.EqualTo(4), "One door per colour.");
            Assert.That(level.TimerSeconds, Is.EqualTo(_generator.TimerFor(7, LevelDifficulty.Hard)));
        }

        [Test]
        public void Generate_ShouldGiveTheSameLevel_WhenTheSeedRepeats()
        {
            LevelData first = _generator.Generate(Request(LevelDifficulty.Normal, 42)).Level;
            LevelData second = _generator.Generate(Request(LevelDifficulty.Normal, 42)).Level;

            Assert.That(JsonUtility.ToJson(second), Is.EqualTo(JsonUtility.ToJson(first)));
        }

        [Test]
        public void Generate_ShouldStop_WhenCancelled()
        {
            using var cancelled = new System.Threading.CancellationTokenSource();
            cancelled.Cancel();

            Assert.Throws<System.OperationCanceledException>(() => _generator.Generate(Request(LevelDifficulty.VeryHard, 1), cancelled.Token));
        }

        [Test]
        public void Generate_ShouldSaySo_WhenTheSettingsCannotWork()
        {
            LevelGenerationResult result = _generator.Generate(new LevelGenerationRequest(3, 3, 4, 20, LevelDifficulty.Normal, 1, 20));

            Assert.That(result.HasLevel, Is.False);
            Assert.That(result.Failure, Is.Not.Empty);
        }

        [Test]
        public void Generate_ShouldRefuse_WhenThereAreFewerBlocksThanColours()
        {
            Assert.That(_generator.Generate(new LevelGenerationRequest(5, 5, 4, 2, LevelDifficulty.Normal, 1, 10)).HasLevel, Is.False);
        }

        [Test]
        public void Timer_ShouldGrowWithBlocksAndDifficulty_WhenSuggested()
        {
            Assert.That(_generator.TimerFor(4, LevelDifficulty.Normal), Is.EqualTo(60));
            Assert.That(_generator.TimerFor(4, LevelDifficulty.Hard), Is.GreaterThan(60));
            Assert.That(_generator.TimerFor(4, LevelDifficulty.VeryHard), Is.GreaterThan(_generator.TimerFor(4, LevelDifficulty.Hard)));
        }
    }
}

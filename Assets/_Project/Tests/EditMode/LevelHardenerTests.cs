using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Solving;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class LevelHardenerTests
    {
        const int Cap = 5000;

        LevelRulesConfig _rules;
        LevelDefinitionValidator _validator;
        LevelHardener _hardener;
        LevelData _start;

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            _validator = new LevelDefinitionValidator(_rules);
            _hardener = new LevelHardener(_validator, Cap);
            _start = new LevelGenerator(_validator, 2000, new BlockShapeLibrary())
                .Generate(new LevelGenerationRequest(6, 7, 4, 8, LevelDifficulty.Hard, 11, 100)).Level;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_rules);

        static LevelHardeningRequest Request(bool blockers) => new(3, 12, int.MaxValue, blockers, blockers);

        [Test]
        public void Harden_ShouldNeedAtLeastAsManyMoves_WhenItFinishes()
        {
            int before = new LevelSolver(Cap).Solve(_start).Moves;

            LevelHardeningResult result = _hardener.Harden(_start, Request(true));

            Assert.That(result.Solution.Moves, Is.GreaterThanOrEqualTo(before));
            Assert.That(result.Solution.Moves, Is.GreaterThan(before), "Twelve rounds find at least one harder layout.");
            Assert.That(_validator.HasErrors(_validator.Validate(result.Level)), Is.False);
            Assert.That(new LevelSolver(Cap).Solve(result.Level).Status, Is.EqualTo(SolveStatus.Solvable));
        }

        [Test]
        public void Harden_ShouldGiveTheSameLevel_WhenTheSeedRepeats()
        {
            LevelData first = _hardener.Harden(_start, Request(true)).Level;
            LevelData second = _hardener.Harden(_start, Request(true)).Level;

            Assert.That(JsonUtility.ToJson(second), Is.EqualTo(JsonUtility.ToJson(first)));
        }

        [Test]
        public void Harden_ShouldAddNoBlockers_WhenArrowsAndIceAreOff()
        {
            LevelData level = _hardener.Harden(_start, Request(false)).Level;

            foreach (BlockData block in level.Blocks)
                Assert.That(block.Blockers.IsEmpty, Is.True);
        }

        [Test]
        public void Harden_ShouldStop_WhenTheTargetIsReached()
        {
            LevelHardeningResult result = _hardener.Harden(_start, new LevelHardeningRequest(3, 50, 0, true, true));

            Assert.That(result.Rounds, Is.Zero);
            Assert.That(JsonUtility.ToJson(result.Level), Is.EqualTo(JsonUtility.ToJson(_start)));
        }

        [Test]
        public void Harden_ShouldGiveNothing_WhenTheLevelCannotBeFinished()
        {
            LevelData stuck = new LevelDataBuilder(2, 1).Block(Red, 0, 0).Block(Blue, 1, 0)
                .Door(Right, 0, 1, Red).Door(Left, 0, 1, Blue).Build();

            Assert.That(_hardener.Harden(stuck, Request(true)), Is.Null);
        }
    }
}

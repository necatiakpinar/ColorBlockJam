using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using ColorBlockJam.Editor.LevelEditing;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Solving;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using UnityEngine.TestTools;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>
    /// The level editor's checks: the validator answers at once and the solver runs in the background, so an edit
    /// never waits for it (level-editor.md §3).
    /// </summary>
    public sealed class LevelCheckerTests
    {
        const int WaitMilliseconds = 5000;

        LevelRulesConfig _rules;
        LevelChecker _checker;

        static LevelData Solvable() => new LevelDataBuilder(4, 4).Block(Red, 1, 1).Door(Left, 1, 1, Red).Build();

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            _checker = new LevelChecker(new LevelDefinitionValidator(_rules), 20000, 0);
        }

        [TearDown]
        public void TearDown()
        {
            _checker.Dispose();
            Object.DestroyImmediate(_rules);
        }

        [Test]
        public void Check_ShouldReturnBeforeTheSolverFinishes_WhenTheLevelIsValid()
        {
            _checker.Check(Solvable());

            Assert.That(_checker.Issues, Is.Empty);
            Assert.That(_checker.IsSolving, Is.True);
            Assert.That(_checker.Solution, Is.Null);
        }

        [Test]
        public void Check_ShouldNotSolve_WhenTheLevelHasErrors()
        {
            _checker.Check(new LevelDataBuilder(4, 4).Block(Red, 1, 1).Build());

            Assert.That(_checker.Issues, Is.Not.Empty);
            Assert.That(_checker.IsSolving, Is.False);
        }

        [UnityTest]
        public IEnumerator Check_ShouldReportTheSolution_WhenTheBackgroundSolveLands()
        {
            int solved = 0;
            _checker.Solved += () => solved++;

            _checker.Check(Solvable(), now: true);
            var watch = Stopwatch.StartNew();
            while (_checker.IsSolving && watch.ElapsedMilliseconds < WaitMilliseconds)
                yield return null;

            Assert.That(_checker.Solution.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(solved, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Check_ShouldDropTheOldSolve_WhenANewCheckStarts()
        {
            int solved = 0;
            _checker.Solved += () => solved++;

            _checker.Check(Solvable(), now: true);
            _checker.Check(new LevelDataBuilder(4, 4).Block(Red, 1, 1).Block(Blue, 2, 2)
                .Door(Left, 1, 1, Red).Door(Right, 2, 1, Blue).Build(), now: true);
            var watch = Stopwatch.StartNew();
            while (_checker.IsSolving && watch.ElapsedMilliseconds < WaitMilliseconds)
                yield return null;

            Assert.That(solved, Is.EqualTo(1), "Only the newest draft reports.");
            Assert.That(_checker.Solution.Steps, Has.Count.EqualTo(2));
        }
    }
}

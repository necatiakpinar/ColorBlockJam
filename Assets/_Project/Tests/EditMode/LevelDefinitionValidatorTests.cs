using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RollicCase.Game.Configs;
using RollicCase.Game.Levels;
using RollicCase.Tests.EditMode.Helpers;
using UnityEngine;
using static RollicCase.Game.Levels.BlockColor;
using static RollicCase.Game.Levels.WallSide;

namespace RollicCase.Tests.EditMode
{
    public sealed class LevelDefinitionValidatorTests
    {
        LevelRulesConfig _rules;
        LevelDefinitionValidator _validator;

        [SetUp]
        public void SetUp()
        {
            // Defaults: board 3-10, timer 10-600 s.
            _rules = ScriptableObject.CreateInstance<LevelRulesConfig>();
            _validator = new LevelDefinitionValidator(_rules);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_rules);

        /// <summary>A small valid level the failing cases change one thing in.</summary>
        static LevelDataBuilder ValidLevel() => new LevelDataBuilder(4, 4)
            .Block(Red, 0, 0, 1, 0)
            .Door(Bottom, 0, 2, Red);

        IReadOnlyList<LevelIssue> Validate(LevelDataBuilder builder) => _validator.Validate(builder.Build());

        static void AssertSingleError(IReadOnlyList<LevelIssue> issues, string expectedFragment)
        {
            List<LevelIssue> errors = issues.Where(issue => issue.Severity == LevelIssueSeverity.Error).ToList();
            Assert.That(errors.Count, Is.EqualTo(1), string.Join("\n", issues));
            StringAssert.Contains(expectedFragment, errors[0].Message);
        }

        [Test]
        public void Validate_ShouldReportNothing_WhenLevelIsValid()
        {
            Assert.That(Validate(ValidLevel()), Is.Empty);
        }

        [TestCase(2, 4)]
        [TestCase(11, 4)]
        [TestCase(4, 2)]
        [TestCase(4, 11)]
        public void Validate_ShouldReportBoardSize_WhenOutsideTheAllowedRange(int width, int height)
        {
            var level = new LevelDataBuilder(width, height).Block(Red, 0, 0, 1, 0).Door(Bottom, 0, 2, Red);

            AssertSingleError(Validate(level), "width and height must be between 3 and 10");
        }

        [TestCase(5)]
        [TestCase(601)]
        public void Validate_ShouldReportTimer_WhenOutsideTheAllowedRange(int seconds)
        {
            AssertSingleError(Validate(ValidLevel().Timer(seconds)), "must be between 10 and 600 s");
        }

        [Test]
        public void Validate_ShouldReportMissingBlocks_WhenLevelIsEmpty()
        {
            AssertSingleError(_validator.Validate(new LevelDataBuilder(4, 4).Build()), "has no blocks");
        }

        [Test]
        public void Validate_ShouldReportEmptyBlock_WhenABlockHasNoCells()
        {
            AssertSingleError(Validate(ValidLevel().Block(Blue)), "Block 2 (Blue) has no cells");
        }

        [Test]
        public void Validate_ShouldReportMissingColour_WhenABlockColourIsUnset()
        {
            var level = new LevelDataBuilder(4, 4).Block(default, 0, 0).Door(Bottom, 0, 1, default);

            IReadOnlyList<LevelIssue> issues = Validate(level);

            Assert.That(issues.Count(issue => issue.Message.Contains("has no colour chosen")), Is.EqualTo(2));
        }

        [Test]
        public void Validate_ShouldReportMissingWall_WhenADoorSideIsUnset()
        {
            AssertSingleError(Validate(ValidLevel().Door(default, 2, 1, Red)), "Door 2 (Red, 0 wall) has no wall chosen");
        }

        [Test]
        public void Validate_ShouldReportCell_WhenBlockLeavesTheBoard()
        {
            var level = ValidLevel().Block(Blue, 3, 3, 4, 3).Door(Top, 2, 2, Blue);

            AssertSingleError(Validate(level), "Block 2 (Blue) has cell (4, 3) outside");
        }

        [Test]
        public void Validate_ShouldReportOverlap_WhenTwoBlocksShareACell()
        {
            var level = ValidLevel().Block(Blue, 1, 0, 1, 1).Door(Left, 0, 2, Blue);

            AssertSingleError(Validate(level), "Block 2 (Blue) overlaps Block 1 (Red) at (1, 0)");
        }

        [Test]
        public void Validate_ShouldReportDuplicateCell_WhenABlockListsACellTwice()
        {
            var level = new LevelDataBuilder(4, 4).Block(Red, 0, 0, 0, 0).Door(Bottom, 0, 1, Red);

            AssertSingleError(Validate(level), "lists cell (0, 0) twice");
        }

        [Test]
        public void Validate_ShouldReportSplitBlock_WhenCellsDoNotTouchByAnEdge()
        {
            var level = new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 1).Door(Bottom, 0, 2, Red);

            AssertSingleError(Validate(level), "is split into separate pieces");
        }

        [TestCase(3, 2, "spans cells 3-4")]
        [TestCase(-1, 1, "spans cells -1--1")]
        public void Validate_ShouldReportDoor_WhenItRunsPastTheWall(int start, int length, string fragment)
        {
            IReadOnlyList<LevelIssue> issues = Validate(ValidLevel().Door(Right, start, length, Red));

            AssertSingleError(issues, fragment);
            Assert.That(_validator.HasErrors(issues), Is.True);
        }

        [Test]
        public void Validate_ShouldReportOnlyTheLength_WhenAZeroLengthDoorTouchesAnotherDoor()
        {
            // Start 2 with length 0 ends at cell 1, inside door 1 (cells 0-1); only the length is wrong.
            var level = ValidLevel().Door(Bottom, 2, 0, Red);

            AssertSingleError(Validate(level), "a door spans at least one cell");
        }

        [Test]
        public void Validate_ShouldReportOverlap_WhenTwoDoorsShareWallCells()
        {
            var level = ValidLevel().Door(Bottom, 1, 2, Red);

            AssertSingleError(Validate(level), "Door 2 (Red, Bottom wall) overlaps Door 1");
        }

        [Test]
        public void Validate_ShouldReportOnlyTheLength_WhenAZeroLengthDoorComesFirst()
        {
            // Door 1 (start 1, length 0) ends at cell 0, inside door 2 (cells 0-1).
            var level = new LevelDataBuilder(4, 4)
                .Block(Red, 0, 0, 1, 0)
                .Door(Bottom, 1, 0, Red)
                .Door(Bottom, 0, 2, Red);

            AssertSingleError(Validate(level), "Door 1 (Red, Bottom wall) has length 0");
        }

        [Test]
        public void Validate_ShouldAllowDoorsOnDifferentWalls_WhenTheyShareAnIndex()
        {
            Assert.That(Validate(ValidLevel().Door(Top, 0, 2, Red)), Is.Empty);
        }

        [Test]
        public void Validate_ShouldReportTrappedBlock_WhenNoDoorOfItsColourIsWideEnough()
        {
            var level = new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0, 2, 0).Door(Bottom, 0, 2, Red);

            AssertSingleError(Validate(level), "Block 1 (Red) can never leave");
        }

        [Test]
        public void Validate_ShouldAcceptNarrowBlock_WhenADoorOnTheCrossWallFitsIt()
        {
            // Three cells wide but one cell tall: a one-cell door on the left wall lets it out.
            var level = new LevelDataBuilder(4, 4).Block(Red, 0, 0, 1, 0, 2, 0).Door(Left, 0, 1, Red);

            Assert.That(Validate(level), Is.Empty);
        }

        [Test]
        public void Validate_ShouldWarnOnly_WhenADoorHasNoBlockOfItsColour()
        {
            IReadOnlyList<LevelIssue> issues = Validate(ValidLevel().Door(Top, 0, 1, Green));

            Assert.That(issues.Count, Is.EqualTo(1));
            Assert.That(issues[0].Severity, Is.EqualTo(LevelIssueSeverity.Warning));
            Assert.That(_validator.HasErrors(issues), Is.False);
        }
    }
}

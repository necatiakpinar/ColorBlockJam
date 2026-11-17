using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Solving;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Whether a level can be finished, with the game's own rules (T-33, DA-32).</summary>
    public sealed class LevelSolverTests
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";

        readonly LevelSolver _solver = new(20000);

        /// <summary>Plays the solution on a real board: every move must be a free path and every exit real.</summary>
        static bool Replays(LevelData level, SolveResult result)
        {
            var board = new BoardState(level);
            foreach (SolveStep step in result.Steps)
            {
                // Walk the block to its target one cell at a time along a free path found by search.
                if (!WalkTo(board, step.BlockId, step.Offset, new HashSet<Vector2Int>()))
                    return false;
                if (step.Exits && board.TryStep(step.BlockId, step.ExitDirection) != StepResult.Exited)
                    return false;
            }

            return board.IsCleared;
        }

        static bool WalkTo(BoardState board, int id, Vector2Int target, HashSet<Vector2Int> seen)
        {
            Vector2Int at = board.Blocks[id].Offset;
            if (at == target)
                return true;
            seen.Add(at);
            foreach (Vector2Int step in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down })
            {
                if (seen.Contains(at + step) || board.CanStep(id, step) != StepResult.Moved)
                    continue;
                board.TryStep(id, step);
                if (WalkTo(board, id, target, seen))
                    return true;
                board.TryStep(id, -step);
            }

            return false;
        }

        [Test]
        public void Solve_ShouldFinishWithOneRound_WhenEveryBlockCanLeaveAtOnce()
        {
            LevelData level = new LevelDataBuilder(4, 4).Block(Red, 1, 1).Block(Blue, 2, 2)
                .Door(Left, 1, 1, Red).Door(Right, 2, 1, Blue).Build();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.Rounds, Is.EqualTo(1));
            Assert.That(result.Moves, Is.Zero);
            Assert.That(Replays(level, result), Is.True);
        }

        [Test]
        public void Solve_ShouldCountAWaveForEachBlockThatWaitsOnAnother_WhenOneBlocksTheOther()
        {
            // Red sits in front of blue's door; blue can only go after red has left.
            LevelData level = new LevelDataBuilder(4, 3).Block(Red, 0, 1).Block(Blue, 1, 1, 2, 1, 3, 1)
                .Door(Left, 1, 1, Blue).Door(Top, 0, 1, Red).Build();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.Rounds, Is.EqualTo(2));
            Assert.That(Replays(level, result), Is.True);
        }

        /// <summary>A 3x4 level the generator found where blocks must step aside before others can leave.</summary>
        static LevelData NeedsRoom() => new LevelDataBuilder(3, 4)
            .Block(Yellow, 2, 2, 2, 1).Block(Blue, 1, 0, 1, 1, 1, 2, 0, 1).Block(Green, 0, 3).Block(Yellow, 2, 3).Block(Blue, 0, 2)
            .Door(Bottom, 0, 1, Yellow).Door(Top, 0, 2, Blue).Door(Right, 1, 3, Green).Build();

        [Test]
        public void Solve_ShouldMakeRoomFirst_WhenBlocksMustStepAsideForOthers()
        {
            LevelData level = NeedsRoom();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.Moves, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Score, Is.EqualTo(result.Rounds + result.Moves));
            Assert.That(Replays(level, result), Is.True);
        }

        [Test]
        public void Solve_ShouldSayUnsolvable_WhenTwoBlocksStandInEachOthersWay()
        {
            // One row: red must leave right past blue, blue must leave left past red, and there is no room to pass.
            LevelData level = new LevelDataBuilder(2, 1).Block(Red, 0, 0).Block(Blue, 1, 0)
                .Door(Right, 0, 1, Red).Door(Left, 0, 1, Blue).Build();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Unsolvable));
            Assert.That(result.Steps, Is.Empty);
        }

        [Test]
        public void Solve_ShouldSayUnknown_WhenTheStateCapIsHit()
        {
            Assert.That(new LevelSolver(1).Solve(NeedsRoom()).Status, Is.EqualTo(SolveStatus.Unknown));
        }

        [Test]
        public void ShippedLevels_ShouldScoreTheirDifficulty_WhenSolved()
        {
            // Scores of the eight shipped levels (level-data.md §3).
            IReadOnlyList<LevelDefinition> levels = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath).Levels;
            int[] scores = { 1, 2, 4, 4, 8, 9, 10, 11 };
            for (int i = 0; i < scores.Length; i++)
            {
                SolveResult result = _solver.Solve(levels[i].Data);
                Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable), $"Level {i + 1}");
                Assert.That(result.Score, Is.EqualTo(scores[i]), $"Level {i + 1}");
                Assert.That(Replays(levels[i].Data, result), Is.True, $"Level {i + 1}");
            }
        }
    }
}

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
    public sealed class LevelSolverTests
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";

        readonly LevelSolver _solver = new(20000);

        static bool Replays(LevelData level, SolveResult result)
        {
            var board = new BoardState(level);
            foreach (SolveStep step in result.Steps)
            {
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
            LevelData level = new LevelDataBuilder(4, 3).Block(Red, 0, 1).Block(Blue, 1, 1, 2, 1, 3, 1)
                .Door(Left, 1, 1, Blue).Door(Top, 0, 1, Red).Build();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(result.Rounds, Is.EqualTo(2));
            Assert.That(Replays(level, result), Is.True);
        }

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
            LevelData level = new LevelDataBuilder(2, 1).Block(Red, 0, 0).Block(Blue, 1, 0)
                .Door(Right, 0, 1, Red).Door(Left, 0, 1, Blue).Build();

            SolveResult result = _solver.Solve(level);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Unsolvable));
            Assert.That(result.Steps, Is.Empty);
        }

        [Test]
        public void Solve_ShouldStop_WhenItsSearchIsCancelled()
        {
            using var cancelled = new System.Threading.CancellationTokenSource();
            cancelled.Cancel();

            Assert.Throws<System.OperationCanceledException>(() => _solver.Solve(NeedsRoom(), cancelled.Token));
        }

        [Test]
        public void Solve_ShouldSayUnknown_WhenTheLevelNeedsMoreMovesThanAllowed()
        {
            SolveResult result = _solver.Solve(NeedsRoom(), 0, System.Threading.CancellationToken.None);

            Assert.That(result.Status, Is.EqualTo(SolveStatus.Unknown));
        }

        [Test]
        public void Solve_ShouldFindTheSameSolution_WhenTheMoveLimitAllowsIt()
        {
            SolveResult limited = _solver.Solve(NeedsRoom(), 5, System.Threading.CancellationToken.None);
            SolveResult free = _solver.Solve(NeedsRoom());

            Assert.That(limited.Status, Is.EqualTo(SolveStatus.Solvable));
            Assert.That(limited.Steps, Is.EqualTo(free.Steps));
        }

        [Test]
        public void Solve_ShouldSayUnknown_WhenTheStateCapIsHit()
        {
            Assert.That(new LevelSolver(1).Solve(NeedsRoom()).Status, Is.EqualTo(SolveStatus.Unknown));
        }

        [Test]
        public void ShippedLevels_ShouldScoreTheirDifficulty_WhenSolved()
        {
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

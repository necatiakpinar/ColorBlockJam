using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Solving;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Editor.LevelEditing
{
    /// <summary>
    /// The level editor's Checks (level-editor.md §3). It validates a draft at once (cheap). It solves the draft on
    /// a worker thread once the edits pause, so dragging a block or a slider never waits for the solver. A new
    /// check cancels the search still running for the old draft.
    /// </summary>
    public sealed class LevelChecker : IDisposable
    {
        readonly LevelDefinitionValidator _validator;
        readonly int _stateCap;
        readonly int _settleMilliseconds;
        CancellationTokenSource _search;

        public LevelChecker(LevelDefinitionValidator validator, int stateCap, int settleMilliseconds)
        {
            _validator = validator;
            _stateCap = stateCap;
            _settleMilliseconds = settleMilliseconds;
        }

        /// <summary>Raised on the main thread when a solve finishes.</summary>
        public event Action Solved;

        public IReadOnlyList<LevelIssue> Issues { get; private set; } = Array.Empty<LevelIssue>();

        /// <summary>The last finished solve of the current draft; null while solving or when there are errors.</summary>
        public SolveResult Solution { get; private set; }

        public bool IsSolving { get; private set; }

        /// <summary>Checks <paramref name="level"/>; <paramref name="now"/> skips the wait for the edits to settle.</summary>
        public void Check(LevelData level, bool now = false)
        {
            Cancel();
            Issues = _validator.Validate(level);
            Solution = null;
            IsSolving = !_validator.HasErrors(Issues);
            if (!IsSolving)
                return;
            _search = new CancellationTokenSource();
            SolveAsync(level, now ? 0 : _settleMilliseconds, _search.Token).Forget();
        }

        async UniTaskVoid SolveAsync(LevelData level, int delay, CancellationToken token)
        {
            try
            {
                if (delay > 0)
                    await UniTask.Delay(delay, DelayType.Realtime, cancellationToken: token);
                var solver = new LevelSolver(_stateCap);
                SolveResult result = await UniTask.RunOnThreadPool(() => solver.Solve(level, token), cancellationToken: token);
                if (token.IsCancellationRequested)
                    return;
                Solution = result;
                IsSolving = false;
                Solved?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // A newer draft took over; its own check reports.
            }
        }

        void Cancel()
        {
            _search?.Cancel();
            _search?.Dispose();
            _search = null;
        }

        public void Dispose() => Cancel();
    }
}

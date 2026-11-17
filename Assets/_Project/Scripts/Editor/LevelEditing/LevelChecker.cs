using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Solving;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Editor.LevelEditing
{
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

        public event Action Solved;

        public IReadOnlyList<LevelIssue> Issues { get; private set; } = Array.Empty<LevelIssue>();

        public SolveResult Solution { get; private set; }

        public bool IsSolving { get; private set; }

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

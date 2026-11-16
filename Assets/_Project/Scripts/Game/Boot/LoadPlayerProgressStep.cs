using System.Threading;
using Cysharp.Threading.Tasks;
using RollicCase.Core.Boot;
using RollicCase.Core.Persistence;

namespace RollicCase.Game.Boot
{
    public sealed class LoadPlayerProgressStep : IBootStep
    {
        readonly IPlayerProgressService _playerProgress;

        public LoadPlayerProgressStep(IPlayerProgressService playerProgress)
        {
            _playerProgress = playerProgress;
        }

        public float Weight => 2f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _playerProgress.Load();
            return UniTask.CompletedTask;
        }
    }
}

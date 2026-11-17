using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Game.Boot
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

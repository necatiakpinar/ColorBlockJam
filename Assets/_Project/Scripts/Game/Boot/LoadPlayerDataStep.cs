using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Game.Boot
{
    /// <summary>Reads the player's saves before anything shows them: progress, wallet and settings.</summary>
    public sealed class LoadPlayerDataStep : IBootStep
    {
        readonly IPlayerProgressService _playerProgress;
        readonly IWalletService _wallet;
        readonly ISettingsService _settings;

        public LoadPlayerDataStep(IPlayerProgressService playerProgress, IWalletService wallet, ISettingsService settings)
        {
            _playerProgress = playerProgress;
            _wallet = wallet;
            _settings = settings;
        }

        public float Weight => 2f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _playerProgress.Load();
            _wallet.Load();
            _settings.Load();
            return UniTask.CompletedTask;
        }
    }
}

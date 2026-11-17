using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Boot
{
    /// <summary>Reads the player's saves before anything shows them: progress, wallet, settings and boosters.</summary>
    public sealed class LoadPlayerDataStep : IBootStep
    {
        readonly IPlayerProgressService _playerProgress;
        readonly IWalletService _wallet;
        readonly ISettingsService _settings;
        readonly BoosterInventory _boosters;

        public LoadPlayerDataStep(IPlayerProgressService playerProgress, IWalletService wallet, ISettingsService settings,
            BoosterInventory boosters)
        {
            _playerProgress = playerProgress;
            _wallet = wallet;
            _settings = settings;
            _boosters = boosters;
        }

        public float Weight => 2f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _playerProgress.Load();
            _wallet.Load();
            _settings.Load();
            _boosters.Load();
            return UniTask.CompletedTask;
        }
    }
}

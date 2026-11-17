using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Boot;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Lives;

namespace ColorBlockJam.Game.Boot
{
    public sealed class LoadPlayerDataStep : IBootStep
    {
        readonly IPlayerProgressService _playerProgress;
        readonly IWalletService _wallet;
        readonly ISettingsService _settings;
        readonly BoosterInventory _boosters;
        readonly LivesService _lives;

        public LoadPlayerDataStep(IPlayerProgressService playerProgress, IWalletService wallet, ISettingsService settings,
            BoosterInventory boosters, LivesService lives)
        {
            _lives = lives;
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
            _lives.Load();
            return UniTask.CompletedTask;
        }
    }
}

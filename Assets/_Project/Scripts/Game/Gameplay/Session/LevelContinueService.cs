using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Configs;

namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>
    /// Sells a lost level more time (DA-30): <see cref="GameplayConfig.ContinueSeconds"/> for
    /// <see cref="GameplayConfig.ContinueCost"/> coins. The coins are taken before the level plays on;
    /// nothing changes when the player cannot pay or the level is not lost.
    /// </summary>
    public sealed class LevelContinueService
    {
        readonly LevelSession _session;
        readonly IWalletService _wallet;
        readonly GameplayConfig _config;

        public LevelContinueService(LevelSession session, IWalletService wallet, GameplayConfig config)
        {
            _session = session;
            _wallet = wallet;
            _config = config;
        }

        public int Cost => _config.ContinueCost;
        public int ExtraSeconds => _config.ContinueSeconds;
        public bool CanAfford => _wallet.Coins >= Cost;

        public bool TryContinue()
        {
            if (_session.State != LevelState.Failed || !_wallet.TrySpend(Cost))
                return false;
            return _session.Continue(ExtraSeconds);
        }
    }
}

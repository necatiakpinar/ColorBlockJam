using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Navigation;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Game.Debugging
{
    /// <summary>
    /// Shortcuts for testing the gameplay scene by hand (debug-tools.md): win or lose now, set the timer, give
    /// coins or boosters, jump to a level. Every shortcut goes through the service that owns the change, so the game reacts
    /// exactly as it does in play (popups, rewards, saves). Used by the editor's debug window.
    /// </summary>
    public sealed class GameplayDebugCommands
    {
        readonly LevelSession _session;
        readonly LevelTimer _timer;
        readonly IWalletService _wallet;
        readonly IPlayerProgressService _progress;
        readonly INavigationService _navigation;
        readonly SignalBus _signalBus;
        readonly BoosterInventory _boosters;

        public GameplayDebugCommands(LevelSession session, LevelTimer timer, IWalletService wallet,
            IPlayerProgressService progress, INavigationService navigation, SignalBus signalBus, BoosterInventory boosters)
        {
            _boosters = boosters;
            _session = session;
            _timer = timer;
            _wallet = wallet;
            _progress = progress;
            _navigation = navigation;
            _signalBus = signalBus;
        }

        public int LevelNumber => _session.LevelNumber;
        public LevelState State => _session.State;
        public float SecondsLeft => _timer.SecondsLeft;
        public int Coins => _wallet.Coins;

        /// <summary>Wins the level as if its last block had left: the reward is paid and the win popup opens.</summary>
        public bool Win()
        {
            if (_session.State != LevelState.Playing)
                return false;
            _signalBus.Fire(new BoardClearedSignal());
            return true;
        }

        /// <summary>Runs the timer out now: the level fails and the out-of-time popup opens.</summary>
        public bool RunOutOfTime()
        {
            if (_session.State != LevelState.Playing)
                return false;
            _timer.SetSecondsLeft(0f);
            _session.Fail();
            return true;
        }

        public bool Restart() => _session.Restart();

        public void SetSecondsLeft(float seconds) => _timer.SetSecondsLeft(seconds);

        public void AddCoins(int amount) => _wallet.Add(amount);

        public bool SpendCoins(int amount) => _wallet.TrySpend(amount);

        public int BoosterCount(BoosterType type) => _boosters.Count(type);

        public void AddBooster(BoosterType type, int amount) => _boosters.Add(type, amount);

        /// <summary>Spends every <paramref name="type"/> the player owns, to see the purchase offer.</summary>
        public void ClearBooster(BoosterType type)
        {
            while (_boosters.TryUse(type))
            {
            }
        }

        /// <summary>Saves <paramref name="level"/> as the current level and reloads the scene to play it.</summary>
        public void PlayLevel(int level)
        {
            _progress.SetCurrentLevel(level);
            _navigation.GoToGameplayAsync().Forget();
        }
    }
}

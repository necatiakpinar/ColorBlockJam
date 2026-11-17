using System;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.UI;
using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Game.Gameplay
{
    public sealed class GameplayLivesFlow : IDisposable
    {
        readonly LivesService _lives;
        readonly IWalletService _wallet;
        readonly OutOfLivesPopupView _popup;
        readonly IPopupService _popups;
        readonly LevelSession _session;
        readonly INavigationService _navigation;

        public GameplayLivesFlow(LivesService lives, IWalletService wallet, OutOfLivesPopupView popup,
            IPopupService popups, LevelSession session, INavigationService navigation)
        {
            _lives = lives;
            _wallet = wallet;
            _popup = popup;
            _popups = popups;
            _session = session;
            _navigation = navigation;
            _popup.RefillClicked += OnRefillClicked;
            _popup.CloseClicked += OnCloseClicked;
        }

        public void PlayAgain()
        {
            if (_lives.Count > 0)
            {
                _session.Restart();
                return;
            }

            _session.Pause();
            _popup.SetOffer(new LivesChangedSignal(_lives.Count, _lives.Max, _lives.SecondsToNextLife()),
                _lives.RefillPrice, _wallet.Coins >= _lives.RefillPrice);
            _popups.Open(_popup);
        }

        void OnRefillClicked()
        {
            if (!_lives.TryRefill())
                return;
            _popups.Close();
            _session.Restart();
        }

        void OnCloseClicked() => _navigation.GoToHomeAsync().Forget();

        public void Dispose()
        {
            _popup.RefillClicked -= OnRefillClicked;
            _popup.CloseClicked -= OnCloseClicked;
        }
    }
}

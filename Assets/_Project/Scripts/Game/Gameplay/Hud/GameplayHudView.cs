using System;
using DG.Tweening;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    public sealed class GameplayHudView : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup _group;

        [SerializeField]
        RectTransform _panel;

        [SerializeField]
        TMP_Text _levelLabel;

        [Tooltip("Time left as mm:ss (T-15).")]
        [SerializeField]
        TMP_Text _timerLabel;

        [SerializeField]
        CoinAmountText _coins;

        [SerializeField]
        Button _restartButton;

        [SerializeField]
        Button _pauseButton;

        SignalBus _signalBus;
        GameplayConfig _config;
        IWalletService _wallet;
        Sequence _revealSequence;
        bool _holdingCoins;
        int _shownCoins;
        readonly char[] _timerChars = new char[5];

        public event Action RestartClicked;
        public event Action PauseClicked;

        [Inject]
        public void Construct(SignalBus signalBus, GameplayConfig config, IWalletService wallet)
        {
            _signalBus = signalBus;
            _config = config;
            _wallet = wallet;
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<TimerChangedSignal>(OnTimerChanged);
            _signalBus.Subscribe<CoinsChangedSignal>(OnCoinsChanged);
            _restartButton.onClick.AddListener(OnRestartClicked);
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        public string LevelLabelText => _levelLabel.text;
        public string TimerLabelText => _timerLabel.text;
        public string CoinLabelText => _coins.Text;

        public void HoldCoins(int coins)
        {
            _holdingCoins = true;
            _shownCoins = coins;
            _coins.Show(coins);
        }

        public void AddHeldCoins(int coins)
        {
            _shownCoins += coins;
            _coins.Show(_shownCoins);
        }

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _levelLabel.SetText("{0}", signal.LevelNumber);
            _holdingCoins = false;
            _coins.Show(_wallet.Coins);

            _revealSequence?.Kill();
            _revealSequence = default(FadeScaleInAnimation).Play(
                new FadeScaleInAnimation.Config(_group, _panel, _config.HudReveal));
        }

        void OnTimerChanged(TimerChangedSignal signal)
        {
            int minutes = signal.SecondsLeft / 60;
            int seconds = signal.SecondsLeft % 60;
            _timerChars[0] = (char)('0' + minutes / 10 % 10);
            _timerChars[1] = (char)('0' + minutes % 10);
            _timerChars[2] = ':';
            _timerChars[3] = (char)('0' + seconds / 10);
            _timerChars[4] = (char)('0' + seconds % 10);
            _timerLabel.SetCharArray(_timerChars, 0, _timerChars.Length);
        }

        void OnCoinsChanged(CoinsChangedSignal signal)
        {
            if (!_holdingCoins)
                _coins.Show(signal.Coins);
        }

        void OnRestartClicked() => RestartClicked?.Invoke();

        void OnPauseClicked() => PauseClicked?.Invoke();

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<TimerChangedSignal>(OnTimerChanged);
            _signalBus.Unsubscribe<CoinsChangedSignal>(OnCoinsChanged);
            _restartButton.onClick.RemoveListener(OnRestartClicked);
            _pauseButton.onClick.RemoveListener(OnPauseClicked);
            _revealSequence?.Kill();
        }
    }
}

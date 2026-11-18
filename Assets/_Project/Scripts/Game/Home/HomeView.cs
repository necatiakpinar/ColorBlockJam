using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Home
{
    public sealed class HomeView : MonoBehaviour
    {
        const string LevelLabelFormat = "Level {0}";

        [SerializeField]
        Button _levelButton;

        [SerializeField]
        TMP_Text _levelLabel;

        [Tooltip("The path's level tiles from the current level upwards.")]
        [SerializeField]
        TMP_Text[] _pathLabels;

        [Tooltip("The current level's tile on the path; it breathes while Home is shown.")]
        [SerializeField]
        RectTransform _currentLevelTile;

        [SerializeField]
        CoinAmountText _coins;

        [SerializeField]
        Button _settingsButton;

        SignalBus _signalBus;
        UiConfig _config;
        Vector2 _tileRest;
        Sequence _pulse;

        public event Action LevelButtonClicked;
        public event Action SettingsButtonClicked;

        [Inject]
        public void Construct(SignalBus signalBus, UiConfig config)
        {
            _signalBus = signalBus;
            _config = config;
            _tileRest = _currentLevelTile.anchoredPosition;
            _signalBus.Subscribe<CoinsChangedSignal>(OnCoinsChanged);
            _levelButton.onClick.AddListener(OnLevelButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        public string LevelLabelText => _levelLabel.text;
        public string CoinText => _coins.Text;

        public string PathLabelText(int index) => _pathLabels[index].text;

        public bool IsCurrentLevelPulsing => _pulse != null && _pulse.IsActive() && _pulse.IsPlaying();

        public void ShowLevel(int levelNumber)
        {
            _levelLabel.SetText(LevelLabelFormat, levelNumber);
            for (int i = 0; i < _pathLabels.Length; i++)
                _pathLabels[i].SetText("{0}", levelNumber + i);
            _pulse?.Kill();
            _pulse = default(IdlePulseAnimation).Play(
                new IdlePulseAnimation.Config(_currentLevelTile, _tileRest, _config.CurrentLevelPulse));
        }

        public void ShowCoins(int coins) => _coins.Show(coins);

        void OnCoinsChanged(CoinsChangedSignal signal) => _coins.Show(signal.Coins);

        void OnLevelButtonClicked() => LevelButtonClicked?.Invoke();

        void OnSettingsButtonClicked() => SettingsButtonClicked?.Invoke();

        void OnDestroy()
        {
            _pulse?.Kill();
            _signalBus.Unsubscribe<CoinsChangedSignal>(OnCoinsChanged);
            _levelButton.onClick.RemoveListener(OnLevelButtonClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }
    }
}

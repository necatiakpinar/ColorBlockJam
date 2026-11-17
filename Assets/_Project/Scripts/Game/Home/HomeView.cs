using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.UI;
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

        [SerializeField]
        CoinAmountText _coins;

        [SerializeField]
        Button _settingsButton;

        SignalBus _signalBus;

        public event Action LevelButtonClicked;
        public event Action SettingsButtonClicked;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<CoinsChangedSignal>(OnCoinsChanged);
            _levelButton.onClick.AddListener(OnLevelButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        public string LevelLabelText => _levelLabel.text;
        public string CoinText => _coins.Text;

        public string PathLabelText(int index) => _pathLabels[index].text;

        public void ShowLevel(int levelNumber)
        {
            _levelLabel.SetText(LevelLabelFormat, levelNumber);
            for (int i = 0; i < _pathLabels.Length; i++)
                _pathLabels[i].SetText("{0}", levelNumber + i);
        }

        public void ShowCoins(int coins) => _coins.Show(coins);

        void OnCoinsChanged(CoinsChangedSignal signal) => _coins.Show(signal.Coins);

        void OnLevelButtonClicked() => LevelButtonClicked?.Invoke();

        void OnSettingsButtonClicked() => SettingsButtonClicked?.Invoke();

        void OnDestroy()
        {
            _signalBus.Unsubscribe<CoinsChangedSignal>(OnCoinsChanged);
            _levelButton.onClick.RemoveListener(OnLevelButtonClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }
    }
}

using System;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Home
{
    /// <summary>
    /// Binds the authored Home canvas (T-23, ui.md §7) and reports taps; decides nothing itself. The level path
    /// shows the current level and the next ones; the lives, the "+" buttons, the profile and the tabs are
    /// placeholders with press feedback only (DA-9, DA-11).
    /// </summary>
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

        public event Action LevelButtonClicked;
        public event Action SettingsButtonClicked;

        [Inject]
        public void Construct()
        {
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

        void OnLevelButtonClicked() => LevelButtonClicked?.Invoke();

        void OnSettingsButtonClicked() => SettingsButtonClicked?.Invoke();

        void OnDestroy()
        {
            _levelButton.onClick.RemoveListener(OnLevelButtonClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }
    }
}

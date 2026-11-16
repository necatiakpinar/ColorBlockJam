using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace RollicCase.Game.Home
{
    /// <summary>Binds the authored Home canvas and reports button taps; decides nothing itself.</summary>
    public sealed class HomeView : MonoBehaviour
    {
        const string LevelLabelFormat = "Level {0}";

        [SerializeField]
        Button _levelButton;

        [SerializeField]
        TMP_Text _levelLabel;

        public event Action LevelButtonClicked;

        [Inject]
        public void Construct()
        {
            _levelButton.onClick.AddListener(OnLevelButtonClicked);
        }

        public string LevelLabelText => _levelLabel.text;

        public void ShowLevel(int levelNumber)
        {
            _levelLabel.SetText(LevelLabelFormat, levelNumber);
        }

        void OnLevelButtonClicked() => LevelButtonClicked?.Invoke();

        void OnDestroy()
        {
            _levelButton.onClick.RemoveListener(OnLevelButtonClicked);
        }
    }
}

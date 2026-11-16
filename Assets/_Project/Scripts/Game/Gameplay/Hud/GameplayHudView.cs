using System;
using DG.Tweening;
using RollicCase.Core.Signals;
using RollicCase.Game.Common.Animations;
using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace RollicCase.Game.Gameplay.Hud
{
    public sealed class GameplayHudView : MonoBehaviour
    {
        const string LevelLabelFormat = "Level {0}";

        [SerializeField]
        CanvasGroup _group;

        [SerializeField]
        RectTransform _panel;

        [SerializeField]
        TMP_Text _levelLabel;

        [Tooltip("Placeholder route back to Home until the pause popup owns it (T-19).")]
        [SerializeField]
        Button _homeButton;

        SignalBus _signalBus;
        GameplayConfig _config;
        Sequence _revealSequence;

        public event Action HomeButtonClicked;

        [Inject]
        public void Construct(SignalBus signalBus, GameplayConfig config)
        {
            _signalBus = signalBus;
            _config = config;
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _homeButton.onClick.AddListener(OnHomeButtonClicked);
        }

        public string LevelLabelText => _levelLabel.text;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _levelLabel.SetText(LevelLabelFormat, signal.LevelNumber);

            _revealSequence?.Kill();
            _revealSequence = default(FadeScaleInAnimation).Play(
                new FadeScaleInAnimation.Config(_group, _panel, _config.HudReveal));
        }

        void OnHomeButtonClicked() => HomeButtonClicked?.Invoke();

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _homeButton.onClick.RemoveListener(OnHomeButtonClicked);
            _revealSequence?.Kill();
        }
    }
}

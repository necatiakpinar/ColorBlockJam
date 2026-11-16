using DG.Tweening;
using RollicCase.Core.Signals;
using RollicCase.Game.Common.Animations;
using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Signals;
using TMPro;
using UnityEngine;
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

        SignalBus _signalBus;
        GameplayConfig _config;
        Sequence _revealSequence;

        [Inject]
        public void Construct(SignalBus signalBus, GameplayConfig config)
        {
            _signalBus = signalBus;
            _config = config;
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        public string LevelLabelText => _levelLabel.text;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _levelLabel.SetText(LevelLabelFormat, signal.LevelNumber);

            _revealSequence?.Kill();
            _revealSequence = default(FadeScaleInAnimation).Play(
                new FadeScaleInAnimation.Config(_group, _panel, _config.HudReveal));
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _revealSequence?.Kill();
        }
    }
}

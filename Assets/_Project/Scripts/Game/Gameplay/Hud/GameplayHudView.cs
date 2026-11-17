using System;
using DG.Tweening;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
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

        [Tooltip("Time left as mm:ss (T-15); T-18 places it in the timer holder art.")]
        [SerializeField]
        TMP_Text _timerLabel;

        [Tooltip("Placeholder route back to Home until the pause popup owns it (T-19).")]
        [SerializeField]
        Button _homeButton;

        SignalBus _signalBus;
        GameplayConfig _config;
        Sequence _revealSequence;
        // mm:ss written in place, so a tick allocates nothing on device (in the editor TMP mirrors it into a string).
        readonly char[] _timerChars = new char[5];

        public event Action HomeButtonClicked;

        [Inject]
        public void Construct(SignalBus signalBus, GameplayConfig config)
        {
            _signalBus = signalBus;
            _config = config;
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<TimerChangedSignal>(OnTimerChanged);
            _homeButton.onClick.AddListener(OnHomeButtonClicked);
        }

        public string LevelLabelText => _levelLabel.text;
        public string TimerLabelText => _timerLabel.text;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _levelLabel.SetText(LevelLabelFormat, signal.LevelNumber);

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

        void OnHomeButtonClicked() => HomeButtonClicked?.Invoke();

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<TimerChangedSignal>(OnTimerChanged);
            _homeButton.onClick.RemoveListener(OnHomeButtonClicked);
            _revealSequence?.Kill();
        }
    }
}

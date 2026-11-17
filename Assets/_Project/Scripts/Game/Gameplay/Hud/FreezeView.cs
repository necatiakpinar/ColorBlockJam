using DG.Tweening;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Common.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    /// <summary>
    /// The freeze booster on screen (DA-36): an ice frame around the screen and a meter under the timer that empties
    /// as the freeze runs out, standing still while the level is paused.
    /// </summary>
    public sealed class FreezeView : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup _frame;

        [SerializeField]
        CanvasGroup _meter;

        [SerializeField]
        Image _meterFill;

        GameplayConfig _config;
        SignalBus _signalBus;
        Sequence _frameFade;
        Sequence _meterFade;
        Sequence _drain;

        [Inject]
        public void Construct(GameplayConfig config, SignalBus signalBus)
        {
            _config = config;
            _signalBus = signalBus;
            _signalBus.Subscribe<TimerFreezeChangedSignal>(OnFreezeChanged);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        public bool IsShowing => _frame.alpha > 0f;
        public float MeterFill => _meterFill.fillAmount;

        void OnFreezeChanged(TimerFreezeChangedSignal signal)
        {
            _drain?.Kill();
            Show(signal.IsFrozen, _config.BoosterFadeDuration);
            if (!signal.IsFrozen)
                return;

            _meterFill.fillAmount = signal.SecondsLeft / signal.TotalSeconds;
            if (signal.Counting)
                _drain = default(FillAmountAnimation).Play(new FillAmountAnimation.Config(_meterFill, 0f,
                    signal.SecondsLeft, Ease.Linear));
        }

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _drain?.Kill();
            Show(false, 0f);
        }

        void Show(bool shown, float duration)
        {
            float alpha = shown ? 1f : 0f;
            if (Mathf.Approximately(_frame.alpha, alpha))
                return;
            _frameFade?.Kill();
            _meterFade?.Kill();
            _frameFade = default(CanvasFadeAnimation).Play(new CanvasFadeAnimation.Config(_frame, alpha, duration));
            _meterFade = default(CanvasFadeAnimation).Play(new CanvasFadeAnimation.Config(_meter, alpha, duration));
            if (duration > 0f)
                return;
            _frameFade.Complete();
            _meterFade.Complete();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<TimerFreezeChangedSignal>(OnFreezeChanged);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _frameFade?.Kill();
            _meterFade?.Kill();
            _drain?.Kill();
        }
    }
}

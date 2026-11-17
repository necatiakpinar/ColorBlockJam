using DG.Tweening;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Animations;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using TMPro;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    public sealed class BoosterTargetingView : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup _banner;

        [SerializeField]
        TMP_Text _title;

        [SerializeField]
        TMP_Text _hint;

        [Tooltip("Top bar parts faded while choosing; their buttons still work (they cancel the choice first).")]
        [SerializeField]
        CanvasGroup[] _faded;

        [Tooltip("A wide dark quad just under the board; its material's alpha is faded in.")]
        [SerializeField]
        Renderer _backdrop;

        BoosterConfig _boosters;
        GameplayConfig _config;
        SignalBus _signalBus;
        Material _backdropMaterial;
        Sequence _fade;

        [Inject]
        public void Construct(BoosterConfig boosters, GameplayConfig config, SignalBus signalBus)
        {
            _boosters = boosters;
            _config = config;
            _signalBus = signalBus;
            _backdropMaterial = _backdrop.material;
            _signalBus.Subscribe<BoosterTargetingChangedSignal>(OnTargetingChanged);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
        }

        public bool IsShowing => _banner.alpha > 0f;
        public string TitleText => _title.text;

        void OnTargetingChanged(BoosterTargetingChangedSignal signal)
        {
            if (signal.Active)
            {
                BoosterSettings settings = _boosters.For(signal.Type);
                _title.SetText(settings.Title);
                _hint.SetText(settings.Hint);
            }

            Fade(signal.Active, _config.BoosterFadeDuration);
        }

        void OnLevelStarted(LevelStartedSignal signal) => Fade(false, 0f);

        void Fade(bool shown, float duration)
        {
            _fade?.Kill();
            _fade = default(BoosterDimAnimation).Play(new BoosterDimAnimation.Config(_banner, _backdropMaterial, _faded,
                shown, _config.BoosterDimAlpha, duration, gameObject));
            if (duration <= 0f)
                _fade.Complete();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoosterTargetingChangedSignal>(OnTargetingChanged);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _fade?.Kill();
            Destroy(_backdropMaterial);
        }
    }
}

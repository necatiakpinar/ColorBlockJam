using DG.Tweening;
using RollicCase.Game.Common.Animations;
using RollicCase.Game.Configs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RollicCase.Game.UI
{
    /// <summary>
    /// Scales a button down while pressed and springs it back on release. Every button in the game
    /// carries one (brief §4.1, §4.2), so it reads its config asset directly instead of through the
    /// container: it has no service dependency and would otherwise need registering per button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        UiConfig _config;

        [SerializeField]
        Button _button;

        [SerializeField]
        Transform _target;

        Vector3 _restScale;
        Sequence _sequence;
        bool _isPressed;

        void Awake()
        {
            _restScale = _target.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.IsInteractable())
                return;

            _isPressed = true;
            PressFeedbackSettings settings = _config.PressFeedback;
            Play(_restScale.x * settings.PressedScale, settings.PressDuration, settings.PressEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed)
                return;

            _isPressed = false;
            PressFeedbackSettings settings = _config.PressFeedback;
            Play(_restScale.x, settings.ReleaseDuration, settings.ReleaseEase);
        }

        void Play(float endScale, float duration, Ease ease)
        {
            _sequence?.Kill();
            _sequence = default(ScaleToAnimation).Play(
                new ScaleToAnimation.Config(_target, endScale, duration, ease));
        }

        void OnDisable()
        {
            // A button hidden mid-press must come back at its authored size.
            _sequence?.Kill();
            _isPressed = false;
            _target.localScale = _restScale;
        }
    }
}

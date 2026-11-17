using System;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    /// <summary>"Level failed!" after the player gives up the extra time (T-20, DA-31): Retry, or close to Home.</summary>
    public sealed class LevelFailedPopupView : PopupView
    {
        [SerializeField]
        Button _retryButton;

        [SerializeField]
        Button _closeButton;

        public event Action RetryClicked;
        public event Action CloseClicked;

        [Inject]
        public void Construct()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        void OnRetryClicked() => RetryClicked?.Invoke();

        void OnCloseClicked() => CloseClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}

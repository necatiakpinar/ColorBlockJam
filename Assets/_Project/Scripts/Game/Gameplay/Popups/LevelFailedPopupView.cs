using System;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    public sealed class LevelFailedPopupView : PopupView
    {
        [SerializeField]
        Button _retryButton;

        [SerializeField]
        Button _homeButton;

        [SerializeField]
        Button _closeButton;

        public event Action RetryClicked;
        public event Action HomeClicked;

        [Inject]
        public void Construct()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _homeButton.onClick.AddListener(OnHomeClicked);
            _closeButton.onClick.AddListener(OnHomeClicked);
        }

        void OnRetryClicked() => RetryClicked?.Invoke();

        void OnHomeClicked() => HomeClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _homeButton.onClick.RemoveListener(OnHomeClicked);
            _closeButton.onClick.RemoveListener(OnHomeClicked);
        }
    }
}

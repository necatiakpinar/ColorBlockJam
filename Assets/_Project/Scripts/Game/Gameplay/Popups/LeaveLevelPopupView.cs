using System;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    /// <summary>
    /// Asks before Home ends a started attempt (lives.md, DA-38): leaving costs a life. Leave goes; close stays in
    /// the paused level.
    /// </summary>
    public sealed class LeaveLevelPopupView : PopupView
    {
        [SerializeField]
        Button _leaveButton;

        [SerializeField]
        Button _closeButton;

        public event Action LeaveClicked;
        public event Action CloseClicked;

        [Inject]
        public void Construct()
        {
            _leaveButton.onClick.AddListener(OnLeaveClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        void OnLeaveClicked() => LeaveClicked?.Invoke();

        void OnCloseClicked() => CloseClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}

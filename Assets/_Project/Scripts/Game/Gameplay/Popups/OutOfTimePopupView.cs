using System;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    public sealed class OutOfTimePopupView : PopupView
    {
        const string SecondsFormat = "+{0}";
        const string MessageFormat = "Get {0} seconds to keep playing!";

        [SerializeField]
        TMP_Text _secondsLabel;

        [SerializeField]
        TMP_Text _messageLabel;

        [SerializeField]
        TMP_Text _costLabel;

        [SerializeField]
        Button _continueButton;

        [SerializeField]
        Button _closeButton;

        public event Action ContinueClicked;
        public event Action CloseClicked;

        public bool CanContinue => _continueButton.interactable;

        [Inject]
        public void Construct()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void SetOffer(int extraSeconds, int cost, bool canAfford)
        {
            _secondsLabel.SetText(SecondsFormat, extraSeconds);
            _messageLabel.SetText(MessageFormat, extraSeconds);
            _costLabel.SetText("{0}", cost);
            _continueButton.interactable = canAfford;
        }

        void OnContinueClicked() => ContinueClicked?.Invoke();

        void OnCloseClicked() => CloseClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _continueButton.onClick.RemoveListener(OnContinueClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}

using System;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    /// <summary>"Level complete" with the coins the win paid (already in the wallet) and Continue (T-21, DA-12).</summary>
    public sealed class WinPopupView : PopupView
    {
        const string RewardFormat = "+{0}";

        [SerializeField]
        TMP_Text _rewardLabel;

        [SerializeField]
        Button _continueButton;

        public event Action ContinueClicked;

        public string RewardText => _rewardLabel.text;

        [Inject]
        public void Construct() => _continueButton.onClick.AddListener(OnContinueClicked);

        public void SetReward(int coins) => _rewardLabel.SetText(RewardFormat, coins);

        void OnContinueClicked() => ContinueClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}

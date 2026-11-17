using System;
using System.Collections.Generic;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    public sealed class WinPopupView : PopupView
    {
        const string RewardFormat = "+{0}";

        [SerializeField]
        TMP_Text _rewardLabel;

        [SerializeField]
        Button _continueButton;

        [Tooltip("The pile of coins above Continue; each one becomes a flying coin.")]
        [SerializeField]
        RectTransform[] _coinPile = Array.Empty<RectTransform>();

        public event Action ContinueClicked;

        public string RewardText => _rewardLabel.text;

        public IReadOnlyList<RectTransform> CoinPile => _coinPile;

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

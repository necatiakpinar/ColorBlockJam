using System;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    /// <summary>
    /// Offers one booster for coins when the player has none left (T-36, DA-36): its name, icon and what it does,
    /// and a Buy button with the price that cannot be pressed when the coins are short.
    /// </summary>
    public sealed class BoosterPurchasePopupView : PopupView
    {
        [SerializeField]
        TMP_Text _titleLabel;

        [SerializeField]
        Image _icon;

        [SerializeField]
        TMP_Text _descriptionLabel;

        [SerializeField]
        TMP_Text _priceLabel;

        [SerializeField]
        Button _buyButton;

        [SerializeField]
        Button _closeButton;

        public event Action<BoosterType> BuyClicked;
        public event Action CloseClicked;

        /// <summary>The booster on offer.</summary>
        public BoosterType Offer { get; private set; }

        public bool CanBuy => _buyButton.interactable;
        public string TitleText => _titleLabel.text;

        [Inject]
        public void Construct()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void SetOffer(BoosterSettings booster, bool canAfford)
        {
            Offer = booster.Type;
            _titleLabel.SetText(booster.Title);
            _icon.sprite = booster.Icon;
            _descriptionLabel.SetText(booster.Description);
            _priceLabel.SetText("{0}", booster.Price);
            _buyButton.interactable = canAfford;
        }

        void OnBuyClicked() => BuyClicked?.Invoke(Offer);

        void OnCloseClicked() => CloseClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _buyButton.onClick.RemoveListener(OnBuyClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}

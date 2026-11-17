using System;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.UI;

namespace ColorBlockJam.Game.Gameplay
{
    /// <summary>
    /// Routes the booster bar (T-36): a tap uses the booster through <see cref="BoosterService"/>; tapping the
    /// booster that waits for a target puts it back, tapping another one switches to it. With none left the level
    /// pauses and the purchase popup offers one; buying or closing resumes. The shop and the service own every
    /// change.
    /// </summary>
    public sealed class BoosterFlow : IDisposable
    {
        readonly BoosterBarView _bar;
        readonly BoosterService _boosters;
        readonly BoosterShop _shop;
        readonly BoosterConfig _config;
        readonly BoosterPurchasePopupView _purchase;
        readonly IPopupService _popups;
        readonly LevelSession _session;

        public BoosterFlow(BoosterBarView bar, BoosterService boosters, BoosterShop shop, BoosterConfig config,
            BoosterPurchasePopupView purchase, IPopupService popups, LevelSession session)
        {
            _bar = bar;
            _boosters = boosters;
            _shop = shop;
            _config = config;
            _purchase = purchase;
            _popups = popups;
            _session = session;
            _bar.BoosterClicked += OnBoosterClicked;
            _purchase.BuyClicked += OnBuyClicked;
            _purchase.CloseClicked += OnPurchaseClosed;
        }

        void OnBoosterClicked(BoosterType type)
        {
            if (_boosters.IsTargeting)
            {
                bool same = _boosters.Targeting == type;
                _boosters.CancelTargeting();
                if (same)
                    return;
            }

            if (_boosters.Activate(type) != BoosterUse.NeedsPurchase || !_session.Pause())
                return;
            _purchase.SetOffer(_config.For(type), _shop.CanAfford(type));
            _popups.Open(_purchase);
        }

        void OnBuyClicked(BoosterType type)
        {
            _shop.TryBuy(type);
            OnPurchaseClosed();
        }

        void OnPurchaseClosed()
        {
            _popups.Close();
            _session.Resume();
        }

        public void Dispose()
        {
            _bar.BoosterClicked -= OnBoosterClicked;
            _purchase.BuyClicked -= OnBuyClicked;
            _purchase.CloseClicked -= OnPurchaseClosed;
        }
    }
}

using ColorBlockJam.Game.Gameplay.Popups;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>
    /// The authored popup prefabs of the Gameplay scene (ui.md §3). The scope instantiates each one under its
    /// popup layer and injects it, so the scene holds no popup hierarchy of its own.
    /// </summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Gameplay Popup Config", fileName = "GameplayPopupConfig")]
    public sealed class GameplayPopupConfig : ScriptableObject
    {
        [SerializeField]
        PausePopupView _pausePopup;

        [SerializeField]
        WinPopupView _winPopup;

        [SerializeField]
        OutOfTimePopupView _outOfTimePopup;

        [SerializeField]
        LevelFailedPopupView _levelFailedPopup;

        [SerializeField]
        BoosterPurchasePopupView _boosterPurchasePopup;

        public PausePopupView PausePopup => _pausePopup;
        public WinPopupView WinPopup => _winPopup;
        public OutOfTimePopupView OutOfTimePopup => _outOfTimePopup;
        public LevelFailedPopupView LevelFailedPopup => _levelFailedPopup;
        public BoosterPurchasePopupView BoosterPurchasePopup => _boosterPurchasePopup;
    }
}

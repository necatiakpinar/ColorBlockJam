using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.UI;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
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

        [Tooltip("The lives popup, shared with Home.")]
        [SerializeField]
        OutOfLivesPopupView _outOfLivesPopup;

        [Tooltip("Asks before Home ends a started attempt, which costs a life.")]
        [SerializeField]
        LeaveLevelPopupView _leaveLevelPopup;

        public PausePopupView PausePopup => _pausePopup;
        public WinPopupView WinPopup => _winPopup;
        public OutOfTimePopupView OutOfTimePopup => _outOfTimePopup;
        public LevelFailedPopupView LevelFailedPopup => _levelFailedPopup;
        public BoosterPurchasePopupView BoosterPurchasePopup => _boosterPurchasePopup;
        public OutOfLivesPopupView OutOfLivesPopup => _outOfLivesPopup;
        public LeaveLevelPopupView LeaveLevelPopup => _leaveLevelPopup;
    }
}

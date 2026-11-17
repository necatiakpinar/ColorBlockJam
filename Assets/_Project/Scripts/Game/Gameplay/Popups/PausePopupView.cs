using System;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    public sealed class PausePopupView : SettingsPopupView
    {
        [SerializeField]
        Button _homeButton;

        public event Action HomeClicked;

        [Inject]
        public void ConstructHome() => _homeButton.onClick.AddListener(OnHomeClicked);

        void OnHomeClicked() => HomeClicked?.Invoke();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _homeButton.onClick.RemoveListener(OnHomeClicked);
        }
    }
}

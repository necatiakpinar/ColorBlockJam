using System;
using ColorBlockJam.Game.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Popups
{
    /// <summary>
    /// The settings popup as pausing opens it (T-19, DA-29): the shared settings plus a Home button. Its prefab
    /// is the gameplay variant of the <c>SettingsPopup</c> base prefab; closing it resumes.
    /// </summary>
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

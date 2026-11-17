using System;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// The settings popup (T-25, ui.md §6): vibration, sound and music switches showing the saved settings, a
    /// close button, and placeholder buttons with press feedback only (R-14). Its layout is the
    /// <c>SettingsPopup</c> base prefab; each scene uses a variant of it.
    /// </summary>
    public class SettingsPopupView : PopupView, IDisposable
    {
        [SerializeField]
        Button _closeButton;

        [SerializeField]
        SettingToggleView[] _toggles;

        SignalBus _signalBus;
        ISettingsService _settings;

        public event Action CloseClicked;
        public event Action<GameSetting> ToggleClicked;

        [Inject]
        public void Construct(SignalBus signalBus, ISettingsService settings)
        {
            _signalBus = signalBus;
            _settings = settings;
            _signalBus.Subscribe<SettingChangedSignal>(OnSettingChanged);
            _closeButton.onClick.AddListener(OnCloseClicked);
            foreach (SettingToggleView toggle in _toggles)
            {
                SettingToggleView clicked = toggle;
                toggle.Button.onClick.AddListener(() => ToggleClicked?.Invoke(clicked.Setting));
            }
        }

        /// <summary>Shows the saved switches; call before opening.</summary>
        public void Refresh()
        {
            foreach (SettingToggleView toggle in _toggles)
                toggle.Show(_settings.IsOn(toggle.Setting));
        }

        void OnSettingChanged(SettingChangedSignal signal)
        {
            foreach (SettingToggleView toggle in _toggles)
                if (toggle.Setting == signal.Setting)
                    toggle.Show(signal.IsOn);
        }

        void OnCloseClicked() => CloseClicked?.Invoke();

        /// <summary>
        /// The scope disposes the popup with its scene. Unity calls <see cref="OnDestroy"/> only on objects that were
        /// once active, and a popup never opened never was, so this is what lets go of the app-wide signal bus.
        /// </summary>
        public void Dispose() => _signalBus.Unsubscribe<SettingChangedSignal>(OnSettingChanged);

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            foreach (SettingToggleView toggle in _toggles)
                toggle.Button.onClick.RemoveAllListeners();
        }
    }
}

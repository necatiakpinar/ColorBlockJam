using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.Hud
{
    /// <summary>
    /// The HUD's booster bar (T-36, ui.md §5): shows each booster's stock and reports taps. While one waits for a
    /// target it grows and the others fade; the freeze fades while it is running.
    /// </summary>
    public sealed class BoosterBarView : MonoBehaviour
    {
        [SerializeField]
        BoosterButtonView[] _buttons;

        BoosterInventory _inventory;
        GameplayConfig _config;
        SignalBus _signalBus;
        UnityAction[] _clicks;
        bool _targeting;
        BoosterType _selected;
        bool _frozen;

        public event Action<BoosterType> BoosterClicked;

        [Inject]
        public void Construct(BoosterInventory inventory, GameplayConfig config, SignalBus signalBus)
        {
            _inventory = inventory;
            _config = config;
            _signalBus = signalBus;
            _clicks = new UnityAction[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++)
            {
                BoosterType type = _buttons[i].Type;
                _clicks[i] = () => BoosterClicked?.Invoke(type);
                _buttons[i].Button.onClick.AddListener(_clicks[i]);
            }

            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<BoosterCountChangedSignal>(OnCountChanged);
            _signalBus.Subscribe<BoosterTargetingChangedSignal>(OnTargetingChanged);
            _signalBus.Subscribe<TimerFreezeChangedSignal>(OnFreezeChanged);
        }

        public BoosterButtonView ButtonFor(BoosterType type)
        {
            foreach (BoosterButtonView button in _buttons)
                if (button.Type == type)
                    return button;
            throw new ArgumentException($"The booster bar has no {type} button.", nameof(type));
        }

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _targeting = false;
            _frozen = false;
            foreach (BoosterButtonView button in _buttons)
            {
                button.SetCount(_inventory.Count(button.Type));
                button.SetSelected(false, _config.BoosterSelectedScale, _config.BoosterFadeDuration);
            }

            RefreshFades();
        }

        void OnCountChanged(BoosterCountChangedSignal signal) => ButtonFor(signal.Type).SetCount(signal.Count);

        void OnTargetingChanged(BoosterTargetingChangedSignal signal)
        {
            _targeting = signal.Active;
            _selected = signal.Type;
            ButtonFor(signal.Type).SetSelected(signal.Active, _config.BoosterSelectedScale, _config.BoosterFadeDuration);
            RefreshFades();
        }

        void OnFreezeChanged(TimerFreezeChangedSignal signal)
        {
            _frozen = signal.IsFrozen;
            RefreshFades();
        }

        void RefreshFades()
        {
            foreach (BoosterButtonView button in _buttons)
            {
                bool faded = _targeting ? button.Type != _selected : _frozen && button.Type == BoosterType.Freeze;
                button.SetFaded(faded, _config.BoosterFadedAlpha);
            }
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<BoosterCountChangedSignal>(OnCountChanged);
            _signalBus.Unsubscribe<BoosterTargetingChangedSignal>(OnTargetingChanged);
            _signalBus.Unsubscribe<TimerFreezeChangedSignal>(OnFreezeChanged);
            for (int i = 0; i < _buttons.Length; i++)
                _buttons[i].Button.onClick.RemoveListener(_clicks[i]);
        }
    }
}

using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Lives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// The lives popup of Home and Gameplay (lives.md, DA-38): the lives held, the countdown to the next one, and
    /// a Refill button that fills every missing life for coins. The countdown follows <see cref="LivesChangedSignal"/>
    /// while the popup is open; the flow sets the offer when it opens it.
    /// </summary>
    public sealed class OutOfLivesPopupView : PopupView, IDisposable
    {
        [Tooltip("\"Out of Lives\" with none left, \"Lives\" otherwise.")]
        [SerializeField]
        TMP_Text _titleLabel;

        [SerializeField]
        TMP_Text _countLabel;

        [SerializeField]
        TMP_Text _timerLabel;

        [SerializeField]
        TMP_Text _priceLabel;

        [SerializeField]
        Button _refillButton;

        [SerializeField]
        Button _closeButton;

        readonly char[] _timer = new char[TimerPrefix.Length + 5];
        SignalBus _signalBus;

        const string TimerPrefix = "Next life in ";
        const string FullText = "Your lives are full!";
        const string OutTitle = "Out of Lives";
        const string LivesTitle = "Lives";

        public event Action RefillClicked;
        public event Action CloseClicked;

        public bool CanRefill => _refillButton.interactable;
        public string CountText => _countLabel.text;
        public string TimerText => _timerLabel.text;
        public string TitleText => _titleLabel.text;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<LivesChangedSignal>(OnLivesChanged);
            _refillButton.onClick.AddListener(OnRefillClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
            TimerPrefix.CopyTo(0, _timer, 0, TimerPrefix.Length);
        }

        /// <summary>What the popup offers when it opens.</summary>
        public void SetOffer(LivesChangedSignal lives, int price, bool canAfford)
        {
            _priceLabel.SetText("{0}", price);
            _refillButton.interactable = canAfford && !lives.IsFull;
            ShowLives(lives);
        }

        void OnLivesChanged(LivesChangedSignal signal)
        {
            if (gameObject.activeSelf)
                ShowLives(signal);
        }

        void ShowLives(LivesChangedSignal lives)
        {
            _countLabel.SetText("{0}", lives.Count);
            _titleLabel.SetText(lives.Count == 0 ? OutTitle : LivesTitle);
            if (lives.IsFull)
            {
                _refillButton.interactable = false;
                _timerLabel.SetText(FullText);
                return;
            }

            if (lives.SecondsToNextLife < 0)
            {
                _timerLabel.SetText(TimerPrefix + "--:--");
                return;
            }

            // mm:ss written in place, so a tick allocates nothing on device.
            int minutes = Math.Min(99, lives.SecondsToNextLife / 60);
            int seconds = lives.SecondsToNextLife % 60;
            int i = TimerPrefix.Length;
            _timer[i] = (char)('0' + minutes / 10);
            _timer[i + 1] = (char)('0' + minutes % 10);
            _timer[i + 2] = ':';
            _timer[i + 3] = (char)('0' + seconds / 10);
            _timer[i + 4] = (char)('0' + seconds % 10);
            _timerLabel.SetCharArray(_timer, 0, _timer.Length);
        }

        void OnRefillClicked() => RefillClicked?.Invoke();

        void OnCloseClicked() => CloseClicked?.Invoke();

        /// <summary>
        /// The scope disposes the popup with its scene. Unity calls <see cref="OnDestroy"/> only on objects that were
        /// once active, and a popup never opened never was, so this is what lets go of the app-wide signal bus.
        /// </summary>
        public void Dispose() => _signalBus.Unsubscribe<LivesChangedSignal>(OnLivesChanged);

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
            _refillButton.onClick.RemoveListener(OnRefillClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}

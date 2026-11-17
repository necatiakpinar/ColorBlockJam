using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Lives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.Home
{
    public sealed class LivesBarView : MonoBehaviour
    {
        const string FullText = "Full";

        [SerializeField]
        TMP_Text _countLabel;

        [Tooltip("\"Full\", or mm:ss to the next life.")]
        [SerializeField]
        TMP_Text _statusLabel;

        [SerializeField]
        Button _plusButton;

        readonly char[] _timer = new char[5];
        SignalBus _signalBus;

        public event Action PlusClicked;

        public string CountText => _countLabel.text;
        public string StatusText => _statusLabel.text;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<LivesChangedSignal>(Show);
            _plusButton.onClick.AddListener(OnPlusClicked);
        }

        public void Show(LivesChangedSignal lives)
        {
            _countLabel.SetText("{0}", lives.Count);
            if (lives.IsFull)
            {
                _statusLabel.SetText(FullText);
                return;
            }

            if (lives.SecondsToNextLife < 0)
            {
                _statusLabel.SetText("--:--");
                return;
            }

            int minutes = Math.Min(99, lives.SecondsToNextLife / 60);
            int seconds = lives.SecondsToNextLife % 60;
            _timer[0] = (char)('0' + minutes / 10);
            _timer[1] = (char)('0' + minutes % 10);
            _timer[2] = ':';
            _timer[3] = (char)('0' + seconds / 10);
            _timer[4] = (char)('0' + seconds % 10);
            _statusLabel.SetCharArray(_timer, 0, _timer.Length);
        }

        void OnPlusClicked() => PlusClicked?.Invoke();

        void OnDestroy()
        {
            _signalBus.Unsubscribe<LivesChangedSignal>(Show);
            _plusButton.onClick.RemoveListener(OnPlusClicked);
        }
    }
}

using ColorBlockJam.Game.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ColorBlockJam.Game.UI
{
    public sealed class ButtonTapSounds : MonoBehaviour
    {
        IAudioService _audio;
        Button[] _buttons;

        [Inject]
        public void Construct(IAudioService audio) => _audio = audio;

        void Start()
        {
            _buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < _buttons.Length; i++)
                _buttons[i].onClick.AddListener(OnTapped);
        }

        void OnTapped() => _audio.Play(AudioCue.Tap);

        void OnDestroy()
        {
            if (_buttons == null)
                return;
            for (int i = 0; i < _buttons.Length; i++)
                _buttons[i].onClick.RemoveListener(OnTapped);
        }
    }
}

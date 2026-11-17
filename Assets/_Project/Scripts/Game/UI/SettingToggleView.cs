using ColorBlockJam.Core.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
    /// <summary>
    /// One authored on/off switch for a <see cref="GameSetting"/>: the whole switch is the button, and the
    /// on and off looks are two authored children (art has only the "on" knob, asset-feedback.md A-1).
    /// </summary>
    public sealed class SettingToggleView : MonoBehaviour
    {
        [SerializeField]
        GameSetting _setting;

        [SerializeField]
        Button _button;

        [SerializeField]
        GameObject _onState;

        [SerializeField]
        GameObject _offState;

        public GameSetting Setting => _setting;
        public Button Button => _button;

        public void Show(bool isOn)
        {
            _onState.SetActive(isOn);
            _offState.SetActive(!isOn);
        }
    }
}

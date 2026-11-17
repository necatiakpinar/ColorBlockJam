using ColorBlockJam.Core.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Game.UI
{
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

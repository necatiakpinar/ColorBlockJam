using ColorBlockJam.Game.UI;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>The Home scene's popup prefabs (ui.md §3), created by its scope under the popup layer.</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Home Popup Config", fileName = "HomePopupConfig")]
    public sealed class HomePopupConfig : ScriptableObject
    {
        [Tooltip("The Home variant of the SettingsPopup base prefab.")]
        [SerializeField]
        SettingsPopupView _settingsPopup;

        public SettingsPopupView SettingsPopup => _settingsPopup;
    }
}

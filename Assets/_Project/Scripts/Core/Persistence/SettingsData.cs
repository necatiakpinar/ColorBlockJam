using System;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    /// <summary>The saved toggles. Everything is on for a new player (DA-10).</summary>
    [Serializable]
    public sealed class SettingsData
    {
        [SerializeField]
        bool _music = true;

        [SerializeField]
        bool _sound = true;

        [SerializeField]
        bool _vibration = true;

        public bool IsOn(GameSetting setting) => setting switch
        {
            GameSetting.Music => _music,
            GameSetting.Sound => _sound,
            GameSetting.Vibration => _vibration,
            _ => throw new ArgumentOutOfRangeException(nameof(setting), setting, null),
        };

        public void Set(GameSetting setting, bool isOn)
        {
            switch (setting)
            {
                case GameSetting.Music:
                    _music = isOn;
                    break;
                case GameSetting.Sound:
                    _sound = isOn;
                    break;
                case GameSetting.Vibration:
                    _vibration = isOn;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(setting), setting, null);
            }
        }
    }
}

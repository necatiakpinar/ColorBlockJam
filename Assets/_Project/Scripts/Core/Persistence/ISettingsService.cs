namespace ColorBlockJam.Core.Persistence
{
    /// <summary>The player's toggles (persistence.md). Every change is saved at once and announced with
    /// <see cref="SettingChangedSignal"/>.</summary>
    public interface ISettingsService
    {
        /// <summary>Throws before <see cref="Load"/>.</summary>
        bool IsOn(GameSetting setting);

        void Load();

        /// <summary>Saves and announces the toggle; setting the value it already has does nothing.</summary>
        void Set(GameSetting setting, bool isOn);
    }
}

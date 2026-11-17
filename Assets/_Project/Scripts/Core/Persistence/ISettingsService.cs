namespace ColorBlockJam.Core.Persistence
{
    public interface ISettingsService
    {
        bool IsOn(GameSetting setting);

        void Load();

        void Set(GameSetting setting, bool isOn);
    }
}

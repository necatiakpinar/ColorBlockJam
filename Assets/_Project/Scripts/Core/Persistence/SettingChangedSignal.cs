namespace ColorBlockJam.Core.Persistence
{
    public readonly struct SettingChangedSignal
    {
        public readonly GameSetting Setting;
        public readonly bool IsOn;

        public SettingChangedSignal(GameSetting setting, bool isOn)
        {
            Setting = setting;
            IsOn = isOn;
        }
    }
}

namespace ColorBlockJam.Core.Persistence
{
    /// <summary>A toggle changed and is already saved.</summary>
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

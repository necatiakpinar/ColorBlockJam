using System;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Signals;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class SettingsService : ISettingsService
    {
        public const string StorageKey = "settings_v1";

        readonly JsonRecord<SettingsData> _record;
        readonly SignalBus _signalBus;
        SettingsData _data;

        public SettingsService(IKeyValueStorage storage, IGameLogger logger, SignalBus signalBus)
        {
            _record = new JsonRecord<SettingsData>(StorageKey, storage, logger);
            _signalBus = signalBus;
        }

        SettingsData Data => _data ?? throw new InvalidOperationException($"{nameof(SettingsService)} read before Load().");

        public bool IsOn(GameSetting setting) => Data.IsOn(setting);

        public void Load() => _data = _record.LoadOrCreate(_ => true);

        public void Set(GameSetting setting, bool isOn)
        {
            if (Data.IsOn(setting) == isOn)
                return;
            _data.Set(setting, isOn);
            _record.Save(_data);
            _signalBus.Fire(new SettingChangedSignal(setting, isOn));
        }
    }
}

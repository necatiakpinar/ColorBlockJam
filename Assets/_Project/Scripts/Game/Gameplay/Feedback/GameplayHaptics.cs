using System;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Feedback
{
    public sealed class GameplayHaptics : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IHapticService _haptics;
        readonly GameplayConfig _config;
        readonly ISettingsService _settings;

        public GameplayHaptics(SignalBus signalBus, IHapticService haptics, GameplayConfig config, ISettingsService settings)
        {
            _signalBus = signalBus;
            _haptics = haptics;
            _config = config;
            _settings = settings;
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
        }

        void OnBlockExited(BlockExitedSignal signal)
        {
            if (_settings.IsOn(GameSetting.Vibration))
                _haptics.Play(_config.ExitHaptic);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
        }
    }
}

using System;
using ColorBlockJam.Core.Haptics;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Feedback
{
    /// <summary>
    /// Turns board events into haptic taps (DA-21): a light one when a block leaves through its door. The
    /// strength comes from <see cref="GameplayConfig"/>.
    /// </summary>
    public sealed class GameplayHaptics : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IHapticService _haptics;
        readonly GameplayConfig _config;

        public GameplayHaptics(SignalBus signalBus, IHapticService haptics, GameplayConfig config)
        {
            _signalBus = signalBus;
            _haptics = haptics;
            _config = config;
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
        }

        void OnBlockExited(BlockExitedSignal signal) => _haptics.Play(_config.ExitHaptic);

        public void Dispose()
        {
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
        }
    }
}

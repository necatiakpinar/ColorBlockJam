using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Signals;

namespace ColorBlockJam.Game.Gameplay.Feedback
{
    public sealed class GameplaySounds : IDisposable
    {
        readonly SignalBus _signalBus;
        readonly IAudioService _audio;
        readonly GameplayConfig _config;

        public GameplaySounds(SignalBus signalBus, IAudioService audio, GameplayConfig config)
        {
            _signalBus = signalBus;
            _audio = audio;
            _config = config;
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Subscribe<BoosterUsedSignal>(OnBoosterUsed);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Subscribe<TimerChangedSignal>(OnTimerChanged);
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal) => _audio.Play(AudioCue.Grab);

        void OnBlockExited(BlockExitedSignal signal) => _audio.Play(AudioCue.Exit);

        void OnBoosterUsed(BoosterUsedSignal signal) => _audio.Play(AudioCue.Booster);

        void OnLevelCompleted(LevelCompletedSignal signal) => _audio.Play(AudioCue.Win);

        void OnLevelFailed(LevelFailedSignal signal) => _audio.Play(AudioCue.Fail);

        void OnTimerChanged(TimerChangedSignal signal)
        {
            if (_config.IsLowTime(signal.SecondsLeft))
                _audio.Play(AudioCue.Tick);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Unsubscribe<BoosterUsedSignal>(OnBoosterUsed);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Unsubscribe<TimerChangedSignal>(OnTimerChanged);
        }
    }
}

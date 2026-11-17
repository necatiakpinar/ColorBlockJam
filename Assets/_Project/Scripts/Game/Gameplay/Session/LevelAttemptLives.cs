using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Lives;

namespace ColorBlockJam.Game.Gameplay.Session
{
    public sealed class LevelAttemptLives : IDisposable
    {
        readonly LivesService _lives;
        readonly SignalBus _signalBus;
        bool _started;
        bool _settled;

        public LevelAttemptLives(LivesService lives, SignalBus signalBus)
        {
            _lives = lives;
            _signalBus = signalBus;
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
        }

        public bool WouldCostLife => _started && !_settled;

        public void Abandon()
        {
            if (!_started || _settled)
                return;
            _settled = true;
            _lives.TryLoseOne();
        }

        void OnLevelStarted(LevelStartedSignal signal)
        {
            _started = false;
            _settled = false;
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal) => _started = true;

        void OnLevelCompleted(LevelCompletedSignal signal) => _settled = true;

        void OnLevelFailed(LevelFailedSignal signal) => Abandon();

        public void Dispose()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
        }
    }
}

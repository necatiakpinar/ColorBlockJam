using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Lives;

namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>
    /// Takes the life an attempt costs (lives.md, DA-38): an attempt the player has started (their first grab)
    /// costs one life when it is abandoned (given up after running out of time, restarted, or left for Home); a
    /// won attempt costs nothing. One attempt never costs more than one life.
    /// </summary>
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
        }

        /// <summary>Abandoning now would cost a life: the attempt has started and is not settled.</summary>
        public bool WouldCostLife => _started && !_settled;

        /// <summary>The attempt is being abandoned: it costs a life if it had started and was not settled yet.</summary>
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

        /// <summary>A win keeps the life.</summary>
        void OnLevelCompleted(LevelCompletedSignal signal) => _settled = true;

        public void Dispose()
        {
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        }
    }
}

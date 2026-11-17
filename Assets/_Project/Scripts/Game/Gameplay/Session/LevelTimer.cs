using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Clock;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>
    /// Counts the level's time down (T-15, DA-6). It shows the full time when a level starts and begins to
    /// count at the player's first grab of a block. It stands still while the level is paused, won or lost
    /// and while the app is in the background, and fails the level through
    /// <see cref="LevelSession.Fail"/> when it reaches zero. It ticks with <see cref="IGameClock"/> delays,
    /// one per shown second, and announces each shown second with <see cref="TimerChangedSignal"/>.
    /// </summary>
    public sealed class LevelTimer : IDisposable
    {
        readonly LevelSession _session;
        readonly IGameClock _clock;
        readonly SignalBus _signalBus;
        int _fullSeconds;
        // Seconds left when the current run began (or when it froze), and when that run began.
        float _remaining;
        float _runStart;
        bool _levelPlaying;
        bool _appPaused;
        // The attempt's first grab has happened; until then the full time waits for the player.
        bool _touched;
        int _shown = -1;
        CancellationTokenSource _run;

        public LevelTimer(LevelSession session, IGameClock clock, SignalBus signalBus)
        {
            _session = session;
            _clock = clock;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Subscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Subscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Subscribe<LevelContinuedSignal>(OnLevelContinued);
            _signalBus.Subscribe<ApplicationPausedSignal>(OnApplicationPaused);
        }

        bool IsRunning => _run != null;

        /// <summary>Seconds left, never below zero.</summary>
        public float SecondsLeft => Mathf.Max(0f, IsRunning ? _remaining - (_clock.Now - _runStart) : _remaining);

        void OnBoardLoaded(BoardLoadedSignal signal) => _fullSeconds = signal.Level.TimerSeconds;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            Freeze();
            _remaining = _fullSeconds;
            _shown = -1;
            _touched = false;
            Announce();
            SetLevelPlaying(true);
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal)
        {
            _touched = true;
            Refresh();
        }

        void OnLevelPaused(LevelPausedSignal signal) => SetLevelPlaying(false);

        void OnLevelResumed(LevelResumedSignal signal) => SetLevelPlaying(true);

        void OnLevelCompleted(LevelCompletedSignal signal) => SetLevelPlaying(false);

        void OnLevelFailed(LevelFailedSignal signal) => SetLevelPlaying(false);

        /// <summary>The bought seconds count down at once: the player was already playing.</summary>
        void OnLevelContinued(LevelContinuedSignal signal)
        {
            Freeze();
            _remaining = SecondsLeft + signal.ExtraSeconds;
            Announce();
            SetLevelPlaying(true);
        }

        void OnApplicationPaused(ApplicationPausedSignal signal)
        {
            _appPaused = signal.IsPaused;
            Refresh();
        }

        void SetLevelPlaying(bool playing)
        {
            _levelPlaying = playing;
            Refresh();
        }

        /// <summary>Runs exactly while the level plays, has been touched and the app is in front.</summary>
        void Refresh()
        {
            bool shouldRun = _levelPlaying && _touched && !_appPaused;
            if (shouldRun && !IsRunning)
            {
                _runStart = _clock.Now;
                _run = new CancellationTokenSource();
                RunAsync(_run.Token).Forget();
            }
            else if (!shouldRun)
            {
                Freeze();
            }
        }

        /// <summary>Stops counting and keeps what is left.</summary>
        void Freeze()
        {
            if (!IsRunning)
                return;
            _remaining = SecondsLeft;
            _run.Cancel();
            _run.Dispose();
            _run = null;
        }

        async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            while (true)
            {
                float left = SecondsLeft;
                if (left <= 0f)
                {
                    Expire();
                    return;
                }

                // Wake when the shown whole second drops: 2.4 s left shows 3 and drops to 2 in 0.4 s.
                float untilNextSecond = left - (Mathf.Ceil(left) - 1f);
                if (await _clock.DelayAsync(untilNextSecond, cancellation).SuppressCancellationThrow())
                    return;
                Announce();
            }
        }

        void Expire()
        {
            Freeze();
            _remaining = 0f;
            Announce();
            _session.Fail();
        }

        void Announce()
        {
            int shown = Mathf.CeilToInt(SecondsLeft);
            if (shown == _shown)
                return;
            _shown = shown;
            _signalBus.Fire(new TimerChangedSignal(shown));
        }

        public void Dispose()
        {
            Freeze();
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Unsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<LevelPausedSignal>(OnLevelPaused);
            _signalBus.Unsubscribe<LevelResumedSignal>(OnLevelResumed);
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.Unsubscribe<LevelFailedSignal>(OnLevelFailed);
            _signalBus.Unsubscribe<LevelContinuedSignal>(OnLevelContinued);
            _signalBus.Unsubscribe<ApplicationPausedSignal>(OnApplicationPaused);
        }
    }
}

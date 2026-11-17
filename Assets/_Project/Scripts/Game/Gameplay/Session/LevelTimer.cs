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
    /// <see cref="FreezeFor"/> (the freeze booster, DA-36) stops the count for a while: those seconds pass only
    /// while the level plays and the app is in front, touched or not, and are announced with
    /// <see cref="TimerFreezeChangedSignal"/>.
    /// </summary>
    public sealed class LevelTimer : IDisposable
    {
        readonly LevelSession _session;
        readonly IGameClock _clock;
        readonly SignalBus _signalBus;
        int _fullSeconds;
        // Seconds left when the current run began (or when it stopped), and when that run began.
        float _remaining;
        float _runStart;
        bool _levelPlaying;
        bool _appPaused;
        // The attempt's first grab has happened; until then the full time waits for the player.
        bool _touched;
        int _shown = -1;
        CancellationTokenSource _run;
        // The booster freeze: its length, what was left of it when its current run began, and when that was.
        float _freezeTotal;
        float _freezeLeft;
        float _freezeStart;
        CancellationTokenSource _freezeRun;

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

        /// <summary>A freeze booster is holding the count still.</summary>
        public bool IsFrozen => _freezeLeft > 0f;

        /// <summary>Seconds of freeze left, never below zero.</summary>
        public float FreezeSecondsLeft =>
            Mathf.Max(0f, _freezeRun != null ? _freezeLeft - (_clock.Now - _freezeStart) : _freezeLeft);

        /// <summary>
        /// Holds the count still for <paramref name="seconds"/> of play (DA-36). Refused while a freeze is on: a
        /// second one does not stack.
        /// </summary>
        public bool FreezeFor(float seconds)
        {
            if (IsFrozen || seconds <= 0f)
                return false;
            _freezeTotal = seconds;
            _freezeLeft = seconds;
            AnnounceFreeze(false);
            Refresh();
            return true;
        }

        /// <summary>
        /// Puts the timer at <paramref name="seconds"/> (debug tools, gameplay.md §6). It keeps counting if it was
        /// counting, so zero runs the level out of time at once.
        /// </summary>
        public void SetSecondsLeft(float seconds)
        {
            StopCounting();
            _remaining = Mathf.Max(0f, seconds);
            Announce();
            Refresh();
        }

        void OnBoardLoaded(BoardLoadedSignal signal) => _fullSeconds = signal.Level.TimerSeconds;

        void OnLevelStarted(LevelStartedSignal signal)
        {
            StopCounting();
            ClearFreeze();
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
            StopCounting();
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

        /// <summary>
        /// Counts exactly while the level plays, has been touched, the app is in front and no freeze is on; a
        /// freeze counts down while the level plays and the app is in front.
        /// </summary>
        void Refresh()
        {
            bool playing = _levelPlaying && !_appPaused;
            if (IsFrozen && playing && _freezeRun == null)
                StartFreezeRun();
            else if (_freezeRun != null && !playing)
                StopFreezeRun();

            bool shouldRun = playing && _touched && !IsFrozen;
            if (shouldRun && !IsRunning)
            {
                _runStart = _clock.Now;
                _run = new CancellationTokenSource();
                RunAsync(_run.Token).Forget();
            }
            else if (!shouldRun)
            {
                StopCounting();
            }
        }

        /// <summary>Stops counting and keeps what is left.</summary>
        void StopCounting()
        {
            if (!IsRunning)
                return;
            _remaining = SecondsLeft;
            _run.Cancel();
            _run.Dispose();
            _run = null;
        }

        void StartFreezeRun()
        {
            _freezeStart = _clock.Now;
            _freezeRun = new CancellationTokenSource();
            RunFreezeAsync(_freezeRun.Token).Forget();
            AnnounceFreeze(true);
        }

        void StopFreezeRun()
        {
            _freezeLeft = FreezeSecondsLeft;
            _freezeRun.Cancel();
            _freezeRun.Dispose();
            _freezeRun = null;
            AnnounceFreeze(false);
        }

        async UniTaskVoid RunFreezeAsync(CancellationToken cancellation)
        {
            if (await _clock.DelayAsync(_freezeLeft, cancellation).SuppressCancellationThrow())
                return;
            _freezeRun.Dispose();
            _freezeRun = null;
            _freezeLeft = 0f;
            AnnounceFreeze(false);
            Refresh();
        }

        /// <summary>A new attempt starts unfrozen.</summary>
        void ClearFreeze()
        {
            if (_freezeRun != null)
            {
                _freezeRun.Cancel();
                _freezeRun.Dispose();
                _freezeRun = null;
            }

            if (!IsFrozen)
                return;
            _freezeLeft = 0f;
            AnnounceFreeze(false);
        }

        void AnnounceFreeze(bool counting) =>
            _signalBus.Fire(new TimerFreezeChangedSignal(FreezeSecondsLeft, _freezeTotal, counting));

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
            StopCounting();
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
            StopCounting();
            _freezeRun?.Cancel();
            _freezeRun?.Dispose();
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

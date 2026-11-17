using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>
    /// Owns the attempt at the current level and the only legal ways its <see cref="LevelState"/> changes
    /// (gameplay.md §6):
    /// <code>
    /// Loading --Start--> Playing --Pause--> Paused --Resume--> Playing
    /// Playing --(board cleared)--> Won        Playing --Fail--> Failed
    /// Playing | Paused | Failed --Restart--> Playing (a fresh board from the level data)
    /// Won --Next(n)--> Playing (the next level, on a fresh board)
    /// </code>
    /// Anything else is refused: the method returns false and nothing changes or fires, so a double tap or
    /// a late timer cannot corrupt the attempt. The state is committed before its signal fires.
    /// </summary>
    public sealed class LevelSession : IDisposable
    {
        readonly ActiveBoard _board;
        readonly LevelProvider _levels;
        readonly SignalBus _signalBus;

        public LevelSession(ActiveBoard board, LevelProvider levels, SignalBus signalBus)
        {
            _board = board;
            _levels = levels;
            _signalBus = signalBus;
            _signalBus.Subscribe<BoardClearedSignal>(OnBoardCleared);
        }

        public LevelState State { get; private set; } = LevelState.Loading;

        /// <summary>The level being attempted, 1-based (DA-2); 0 before the first start.</summary>
        public int LevelNumber { get; private set; }

        /// <summary>Starts an attempt at <paramref name="levelNumber"/> on a fresh board.</summary>
        public bool Start(int levelNumber)
        {
            if (State != LevelState.Loading)
                return false;
            Begin(levelNumber);
            return true;
        }

        public bool Pause()
        {
            if (State != LevelState.Playing)
                return false;
            State = LevelState.Paused;
            _signalBus.Fire(new LevelPausedSignal());
            return true;
        }

        public bool Resume()
        {
            if (State != LevelState.Paused)
                return false;
            State = LevelState.Playing;
            _signalBus.Fire(new LevelResumedSignal());
            return true;
        }

        /// <summary>Plays the same level again from its data: nothing of the previous attempt stays (DA-15).</summary>
        public bool Restart()
        {
            if (State is not (LevelState.Playing or LevelState.Paused or LevelState.Failed))
                return false;
            Begin(LevelNumber);
            return true;
        }

        /// <summary>After a win, starts the attempt at <paramref name="levelNumber"/> (the next level).</summary>
        public bool Next(int levelNumber)
        {
            if (State != LevelState.Won)
                return false;
            Begin(levelNumber);
            return true;
        }

        public bool Fail()
        {
            if (State != LevelState.Playing)
                return false;
            State = LevelState.Failed;
            _signalBus.Fire(new LevelFailedSignal(LevelNumber));
            return true;
        }

        void Begin(int levelNumber)
        {
            LevelNumber = levelNumber;
            State = LevelState.Playing;
            _board.Load(_levels.GetLevel(levelNumber));
            _signalBus.Fire(new LevelStartedSignal(levelNumber));
        }

        void OnBoardCleared(BoardClearedSignal signal)
        {
            if (State != LevelState.Playing)
                return;
            State = LevelState.Won;
            _signalBus.Fire(new LevelCompletedSignal(LevelNumber));
        }

        public void Dispose() => _signalBus.Unsubscribe<BoardClearedSignal>(OnBoardCleared);
    }
}

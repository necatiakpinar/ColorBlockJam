using System;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Gameplay.Session
{
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

        public int LevelNumber { get; private set; }

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

        public bool Restart()
        {
            if (State is not (LevelState.Playing or LevelState.Paused or LevelState.Failed))
                return false;
            Begin(LevelNumber);
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

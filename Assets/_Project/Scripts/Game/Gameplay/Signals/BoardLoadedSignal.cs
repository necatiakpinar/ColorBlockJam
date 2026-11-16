using RollicCase.Game.Levels;

namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>
    /// A fresh board for a new attempt; views rebuild from the level as authored (block index = block id).
    /// The live board stays with <c>ActiveBoard</c>, so views cannot change it.
    /// </summary>
    public readonly struct BoardLoadedSignal
    {
        public readonly LevelData Level;

        public BoardLoadedSignal(LevelData level)
        {
            Level = level;
        }
    }
}

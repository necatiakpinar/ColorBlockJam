using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BoardLoadedSignal
    {
        public readonly LevelData Level;

        public BoardLoadedSignal(LevelData level)
        {
            Level = level;
        }
    }
}

namespace ColorBlockJam.Game.Levels
{
    public interface ILevelSource
    {
        int LevelCount { get; }

        LevelData GetLevel(int index);
    }
}

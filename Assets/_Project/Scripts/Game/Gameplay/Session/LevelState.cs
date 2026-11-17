namespace ColorBlockJam.Game.Gameplay.Session
{
    /// <summary>Where an attempt at a level stands (gameplay.md §6).</summary>
    public enum LevelState
    {
        Loading = 1000,
        Playing = 2000,
        Paused = 3000,
        Won = 4000,
        Failed = 5000,
    }
}

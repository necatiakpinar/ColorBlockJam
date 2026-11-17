namespace ColorBlockJam.Core.Persistence
{
    public interface IPlayerProgressService
    {
        /// <summary>1-based number of the level the player is on. Throws before <see cref="Load"/>.</summary>
        int CurrentLevel { get; }

        void Load();

        /// <summary>Moves the player to the next level and persists it immediately.</summary>
        void AdvanceLevel();

        /// <summary>Puts the player on <paramref name="level"/> (1 or more) and persists it immediately.</summary>
        void SetCurrentLevel(int level);
    }
}

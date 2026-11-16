namespace RollicCase.Core.Persistence
{
    public interface IPlayerProgressService
    {
        /// <summary>1-based number of the level the player is on. Throws before <see cref="Load"/>.</summary>
        int CurrentLevel { get; }

        void Load();

        /// <summary>Moves the player to the next level and persists it immediately.</summary>
        void AdvanceLevel();
    }
}

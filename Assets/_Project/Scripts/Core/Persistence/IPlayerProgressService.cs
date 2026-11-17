namespace ColorBlockJam.Core.Persistence
{
    public interface IPlayerProgressService
    {
        int CurrentLevel { get; }

        void Load();

        void AdvanceLevel();

        void SetCurrentLevel(int level);
    }
}

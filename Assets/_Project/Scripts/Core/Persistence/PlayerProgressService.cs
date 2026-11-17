using System;
using ColorBlockJam.Core.Logging;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class PlayerProgressService : IPlayerProgressService
    {
        public const string StorageKey = "player_progress_v1";

        readonly JsonRecord<PlayerProgressData> _record;
        PlayerProgressData _data;

        public PlayerProgressService(IKeyValueStorage storage, IGameLogger logger)
        {
            _record = new JsonRecord<PlayerProgressData>(StorageKey, storage, logger);
        }

        public int CurrentLevel => Data.CurrentLevel;

        PlayerProgressData Data =>
            _data ?? throw new InvalidOperationException($"{nameof(PlayerProgressService)} read before Load().");

        public void Load() => _data = _record.LoadOrCreate(data => data.CurrentLevel >= PlayerProgressData.FirstLevel);

        public void AdvanceLevel()
        {
            Data.CurrentLevel++;
            _record.Save(_data);
        }

        public void SetCurrentLevel(int level)
        {
            if (level < PlayerProgressData.FirstLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level numbers start at 1.");
            Data.CurrentLevel = level;
            _record.Save(_data);
        }
    }
}

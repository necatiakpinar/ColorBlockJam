using System;
using RollicCase.Core.Logging;
using UnityEngine;

namespace RollicCase.Core.Persistence
{
    public sealed class PlayerProgressService : IPlayerProgressService
    {
        public const string StorageKey = "player_progress_v1";

        readonly IKeyValueStorage _storage;
        readonly IGameLogger _logger;
        PlayerProgressData _data;

        public PlayerProgressService(IKeyValueStorage storage, IGameLogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public int CurrentLevel => Data.CurrentLevel;

        PlayerProgressData Data =>
            _data ?? throw new InvalidOperationException($"{nameof(PlayerProgressService)} read before Load().");

        public void Load()
        {
            _data = ReadOrCreate();
        }

        public void AdvanceLevel()
        {
            Data.CurrentLevel++;
            Save();
        }

        PlayerProgressData ReadOrCreate()
        {
            if (!_storage.TryGet(StorageKey, out string json))
                return new PlayerProgressData();

            PlayerProgressData data;
            try
            {
                data = JsonUtility.FromJson<PlayerProgressData>(json);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError($"Corrupt player progress, starting fresh: {exception.Message}");
                return new PlayerProgressData();
            }

            if (data == null || data.CurrentLevel < PlayerProgressData.FirstLevel)
            {
                _logger.LogWarning("Player progress held no valid level, starting fresh.");
                return new PlayerProgressData();
            }

            return data;
        }

        void Save()
        {
            _storage.Set(StorageKey, JsonUtility.ToJson(_data));
            _storage.Flush();
        }
    }
}

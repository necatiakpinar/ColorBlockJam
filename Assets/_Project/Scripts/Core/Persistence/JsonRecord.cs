using System;
using ColorBlockJam.Core.Logging;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class JsonRecord<T> where T : class, new()
    {
        readonly string _key;
        readonly IKeyValueStorage _storage;
        readonly IGameLogger _logger;

        public JsonRecord(string key, IKeyValueStorage storage, IGameLogger logger)
        {
            _key = key;
            _storage = storage;
            _logger = logger;
        }

        public T LoadOrCreate(Func<T, bool> isValid)
        {
            if (!_storage.TryGet(_key, out string json))
                return new T();

            T data;
            try
            {
                data = JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError($"Corrupt save '{_key}', starting fresh: {exception.Message}");
                return new T();
            }

            if (data == null || !isValid(data))
            {
                _logger.LogWarning($"Save '{_key}' held invalid values, starting fresh.");
                return new T();
            }

            return data;
        }

        public void Save(T data)
        {
            _storage.Set(_key, JsonUtility.ToJson(data));
            _storage.Flush();
        }
    }
}

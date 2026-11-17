using System;
using ColorBlockJam.Core.Logging;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    /// <summary>
    /// One saved document: <typeparamref name="T"/> as JSON under one versioned storage key (persistence.md).
    /// Reading never throws on bad data: a missing, corrupt or invalid record starts fresh and says why.
    /// </summary>
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

        /// <summary>The saved record, or a new one when there is none or it fails <paramref name="isValid"/>.</summary>
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

        /// <summary>Writes the record and flushes, so it survives the app being killed right after.</summary>
        public void Save(T data)
        {
            _storage.Set(_key, JsonUtility.ToJson(data));
            _storage.Flush();
        }
    }
}

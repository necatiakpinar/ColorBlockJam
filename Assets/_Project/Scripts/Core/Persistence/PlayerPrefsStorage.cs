using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class PlayerPrefsStorage : IKeyValueStorage
    {
        public bool TryGet(string key, out string value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                value = null;
                return false;
            }

            value = PlayerPrefs.GetString(key);
            return true;
        }

        public void Set(string key, string value) => PlayerPrefs.SetString(key, value);

        public void Delete(string key) => PlayerPrefs.DeleteKey(key);

        public void Flush() => PlayerPrefs.Save();
    }
}

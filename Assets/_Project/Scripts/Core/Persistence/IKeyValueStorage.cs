namespace RollicCase.Core.Persistence
{
    /// <summary>Device-local string storage; the seam that lets persistence be tested without PlayerPrefs.</summary>
    public interface IKeyValueStorage
    {
        bool TryGet(string key, out string value);
        void Set(string key, string value);
        void Delete(string key);
        void Flush();
    }
}

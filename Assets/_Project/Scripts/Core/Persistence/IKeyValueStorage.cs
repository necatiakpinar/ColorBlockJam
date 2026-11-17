namespace ColorBlockJam.Core.Persistence
{
    public interface IKeyValueStorage
    {
        bool TryGet(string key, out string value);
        void Set(string key, string value);
        void Delete(string key);
        void Flush();
    }
}

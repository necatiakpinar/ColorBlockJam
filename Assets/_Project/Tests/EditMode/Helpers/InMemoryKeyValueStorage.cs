using System.Collections.Generic;
using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class InMemoryKeyValueStorage : IKeyValueStorage
    {
        readonly Dictionary<string, string> _values = new();

        public int FlushCount { get; private set; }

        public bool TryGet(string key, out string value) => _values.TryGetValue(key, out value);

        public void Set(string key, string value) => _values[key] = value;

        public void Delete(string key) => _values.Remove(key);

        public void Flush() => FlushCount++;
    }
}

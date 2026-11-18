using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlockJam.Core.Assets;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class FakeAssetSource : IAssetSource
    {
        readonly Dictionary<string, Object> _assets = new();
        readonly Dictionary<string, UniTaskCompletionSource> _gates = new();

        public int Initializations { get; private set; }

        public List<string> Loads { get; } = new();

        public List<string> Releases { get; } = new();

        public void Add(string key, Object asset) => _assets[key] = asset;

        public UniTaskCompletionSource Hold(string key)
        {
            var gate = new UniTaskCompletionSource();
            _gates[key] = gate;
            return gate;
        }

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            Initializations++;
            return UniTask.CompletedTask;
        }

        public async UniTask<IAssetHandle> LoadAsync<T>(string key) where T : Object
        {
            Loads.Add(key);
            if (_gates.TryGetValue(key, out UniTaskCompletionSource gate))
                await gate.Task;
            if (!_assets.TryGetValue(key, out Object asset))
                throw new KeyNotFoundException($"No asset at '{key}'.");
            return new Handle(key, asset, Releases);
        }

        sealed class Handle : IAssetHandle
        {
            readonly string _key;
            readonly List<string> _releases;

            public Handle(string key, Object asset, List<string> releases)
            {
                _key = key;
                Asset = asset;
                _releases = releases;
            }

            public Object Asset { get; }

            public void Release() => _releases.Add(_key);
        }
    }
}

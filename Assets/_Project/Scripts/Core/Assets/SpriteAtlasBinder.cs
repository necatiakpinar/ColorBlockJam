using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.U2D;

namespace ColorBlockJam.Core.Assets
{
    public sealed class SpriteAtlasBinder : IDisposable
    {
        readonly AssetLoader _assets;
        readonly CancellationTokenSource _lifetime = new();

        public SpriteAtlasBinder(AssetLoader assets)
        {
            _assets = assets;
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        public async UniTask PreloadAsync(string tag, CancellationToken cancellationToken)
        {
            await _assets.LoadAsync<SpriteAtlas>(tag, cancellationToken);
        }

        public async UniTask BindAsync(string tag, Action<SpriteAtlas> bind)
        {
            bind(await _assets.LoadAsync<SpriteAtlas>(tag, _lifetime.Token));
        }

        public void Dispose()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        void OnAtlasRequested(string tag, Action<SpriteAtlas> bind) => BindAsync(tag, bind).Forget();
    }
}

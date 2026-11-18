using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Core.Assets
{
    public interface IAssetSource
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);

        UniTask<IAssetHandle> LoadAsync<T>(string key) where T : Object;
    }
}

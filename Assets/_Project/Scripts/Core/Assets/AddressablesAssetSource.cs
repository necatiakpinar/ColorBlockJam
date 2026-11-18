using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ColorBlockJam.Core.Assets
{
    public sealed class AddressablesAssetSource : IAssetSource
    {
        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return Addressables.InitializeAsync().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask<IAssetHandle> LoadAsync<T>(string key) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            try
            {
                await handle.ToUniTask();
            }
            catch
            {
                Addressables.Release(handle);
                throw;
            }

            return new AddressablesAssetHandle<T>(handle);
        }
    }
}

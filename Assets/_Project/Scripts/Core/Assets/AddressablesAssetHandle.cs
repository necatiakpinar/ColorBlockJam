using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ColorBlockJam.Core.Assets
{
    public sealed class AddressablesAssetHandle<T> : IAssetHandle where T : Object
    {
        readonly AsyncOperationHandle<T> _handle;

        public AddressablesAssetHandle(AsyncOperationHandle<T> handle)
        {
            _handle = handle;
        }

        public Object Asset => _handle.Result;

        public void Release() => Addressables.Release(_handle);
    }
}

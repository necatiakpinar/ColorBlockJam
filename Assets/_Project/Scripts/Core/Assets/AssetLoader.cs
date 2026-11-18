using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Core.Assets
{
    public sealed class AssetLoader : IDisposable
    {
        readonly IAssetSource _source;
        readonly Dictionary<(string, Type), Lease> _leases = new();
        bool _disposed;

        public AssetLoader(IAssetSource source)
        {
            _source = source;
        }

        public int LoadedCount => _leases.Count;

        public UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference, CancellationToken cancellationToken) where T : Object
        {
            return LoadAsync<T>(KeyOf(reference), cancellationToken);
        }

        public async UniTask<T> LoadAsync<T>(string key, CancellationToken cancellationToken) where T : Object
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            (string, Type) slot = (key, typeof(T));
            if (!_leases.TryGetValue(slot, out Lease lease))
            {
                lease = new Lease(_source.LoadAsync<T>(key));
                _leases.Add(slot, lease);
            }

            lease.Holders++;
            IAssetHandle handle;
            try
            {
                handle = await lease.Loaded.AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!_disposed)
                    Drop(slot, lease);
                throw;
            }
            catch
            {
                Forget(slot, lease);
                throw;
            }

            ThrowIfDisposed();
            return (T)handle.Asset;
        }

        public void Release<T>(AssetReferenceT<T> reference) where T : Object
        {
            Release<T>(KeyOf(reference));
        }

        public void Release<T>(string key) where T : Object
        {
            ThrowIfDisposed();
            (string, Type) slot = (key, typeof(T));
            if (!_leases.TryGetValue(slot, out Lease lease))
                throw new InvalidOperationException($"{typeof(T).Name} '{key}' is not loaded by this loader.");
            if (lease.Loaded.Status == UniTaskStatus.Pending)
                throw new InvalidOperationException($"{typeof(T).Name} '{key}' is still loading; cancel the load to abandon it.");
            Drop(slot, lease);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            foreach (Lease lease in _leases.Values)
                lease.Free();
            _leases.Clear();
        }

        string KeyOf<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (!reference.RuntimeKeyIsValid())
                throw new ArgumentException($"The {typeof(T).Name} reference points at no asset.", nameof(reference));
            return reference.RuntimeKey.ToString();
        }

        void Drop((string, Type) slot, Lease lease)
        {
            lease.Holders--;
            if (lease.Holders > 0)
                return;
            Forget(slot, lease);
            lease.Free();
        }

        void Forget((string, Type) slot, Lease lease)
        {
            if (_leases.TryGetValue(slot, out Lease current) && current == lease)
                _leases.Remove(slot);
        }

        void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AssetLoader));
        }

        sealed class Lease
        {
            readonly UniTaskCompletionSource<IAssetHandle> _loaded = new();
            IAssetHandle _handle;

            public Lease(UniTask<IAssetHandle> load)
            {
                AwaitLoad(load).Forget();
            }

            public UniTask<IAssetHandle> Loaded => _loaded.Task;

            public int Holders { get; set; }

            public bool Freed { get; private set; }

            public void Free()
            {
                if (Freed)
                    return;
                Freed = true;
                _handle?.Release();
            }

            async UniTaskVoid AwaitLoad(UniTask<IAssetHandle> load)
            {
                try
                {
                    _handle = await load;
                }
                catch (Exception exception)
                {
                    _loaded.TrySetException(exception);
                    return;
                }

                if (Freed)
                    _handle.Release();
                _loaded.TrySetResult(_handle);
            }
        }
    }
}

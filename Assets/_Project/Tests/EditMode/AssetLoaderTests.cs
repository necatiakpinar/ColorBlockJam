using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class AssetLoaderTests
    {
        const string Key = "Levels/Pack";

        FakeAssetSource _source;
        AssetLoader _loader;
        TextAsset _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = new TextAsset("{}");
            _source = new FakeAssetSource();
            _source.Add(Key, _asset);
            _loader = new AssetLoader(_source);
        }

        [TearDown]
        public void TearDown()
        {
            _loader.Dispose();
            Object.DestroyImmediate(_asset);
        }

        [UnityTest]
        public IEnumerator LoadAsync_ShouldLoadOnce_WhenTheSameAssetIsAskedTwice() => UniTask.ToCoroutine(async () =>
        {
            TextAsset first = await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            TextAsset second = await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            Assert.That(first, Is.SameAs(_asset));
            Assert.That(second, Is.SameAs(_asset));
            Assert.That(_source.Loads, Has.Count.EqualTo(1));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldShareOneLoad_WhenAskedAgainWhileLoading() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Key);
            UniTask<TextAsset> first = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            UniTask<TextAsset> second = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            gate.TrySetResult();
            (TextAsset a, TextAsset b) = await UniTask.WhenAll(first, second);

            Assert.That(a, Is.SameAs(b));
            Assert.That(_source.Loads, Has.Count.EqualTo(1));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldLoadSeparately_WhenTheSameKeyIsAskedAsAnotherType() => UniTask.ToCoroutine(async () =>
        {
            await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            await _loader.LoadAsync<Object>(Key, CancellationToken.None);

            Assert.That(_source.Loads, Has.Count.EqualTo(2));
            Assert.That(_loader.LoadedCount, Is.EqualTo(2));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldUseTheReferenceKey_WhenGivenAnAssetReference() => UniTask.ToCoroutine(async () =>
        {
            const string guid = "0123456789abcdef0123456789abcdef";
            _source.Add(guid, _asset);

            TextAsset loaded = await _loader.LoadAsync(new AssetReferenceT<TextAsset>(guid), CancellationToken.None);

            Assert.That(loaded, Is.SameAs(_asset));
            Assert.That(_source.Loads, Is.EqualTo(new[] { guid }));
        });

        [UnityTest]
        public IEnumerator Release_ShouldReleaseTheReferencedAsset_WhenGivenAnAssetReference() => UniTask.ToCoroutine(async () =>
        {
            const string guid = "0123456789abcdef0123456789abcdef";
            _source.Add(guid, _asset);
            var reference = new AssetReferenceT<TextAsset>(guid);
            await _loader.LoadAsync(reference, CancellationToken.None);

            _loader.Release(reference);

            Assert.That(_source.Releases, Is.EqualTo(new[] { guid }));
        });

        [Test]
        public void LoadAsync_ShouldThrow_WhenTheReferenceIsEmpty()
        {
            var empty = new AssetReferenceT<TextAsset>(string.Empty);

            Assert.Throws<ArgumentException>(() => _loader.LoadAsync(empty, CancellationToken.None));
        }

        [UnityTest]
        public IEnumerator Release_ShouldKeepTheAsset_WhenAnotherHolderRemains() => UniTask.ToCoroutine(async () =>
        {
            await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            _loader.Release<TextAsset>(Key);
            Assert.That(_source.Releases, Is.Empty);

            _loader.Release<TextAsset>(Key);
            Assert.That(_source.Releases, Is.EqualTo(new[] { Key }));
            Assert.That(_loader.LoadedCount, Is.Zero);
        });

        [Test]
        public void Release_ShouldThrow_WhenTheAssetIsNotLoaded()
        {
            Assert.Throws<InvalidOperationException>(() => _loader.Release<TextAsset>(Key));
        }

        [UnityTest]
        public IEnumerator LoadAsync_ShouldReleaseItsHold_WhenCancelledWhileLoading() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Key);
            using var cancellation = new CancellationTokenSource();
            UniTask<TextAsset> load = _loader.LoadAsync<TextAsset>(Key, cancellation.Token);

            cancellation.Cancel();
            bool cancelled = await load.SuppressCancellationThrow().ContinueWith(result => result.IsCanceled);
            gate.TrySetResult();
            await UniTask.Yield();

            Assert.That(cancelled, Is.True);
            Assert.That(_loader.LoadedCount, Is.Zero);
            Assert.That(_source.Releases, Is.EqualTo(new[] { Key }));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldRetry_WhenTheEarlierLoadFailed() => UniTask.ToCoroutine(async () =>
        {
            const string missing = "Missing";
            bool failed = false;
            try
            {
                await _loader.LoadAsync<TextAsset>(missing, CancellationToken.None);
            }
            catch (KeyNotFoundException)
            {
                failed = true;
            }

            _source.Add(missing, _asset);
            TextAsset loaded = await _loader.LoadAsync<TextAsset>(missing, CancellationToken.None);

            Assert.That(failed, Is.True);
            Assert.That(loaded, Is.SameAs(_asset));
            Assert.That(_source.Loads, Has.Count.EqualTo(2));
        });

        [UnityTest]
        public IEnumerator Dispose_ShouldReleaseEveryLoadedAsset_WhenTheScopeEnds() => UniTask.ToCoroutine(async () =>
        {
            await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            _loader.Dispose();

            Assert.That(_source.Releases, Is.EqualTo(new[] { Key }));
            Assert.Throws<ObjectDisposedException>(() => _loader.Release<TextAsset>(Key));
        });

        [UnityTest]
        public IEnumerator Dispose_ShouldReleaseALoadInFlight_WhenItFinishesLater() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Key);
            UniTask<TextAsset> load = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            _loader.Dispose();
            gate.TrySetResult();
            bool disposed = false;
            try
            {
                await load;
            }
            catch (ObjectDisposedException)
            {
                disposed = true;
            }

            Assert.That(disposed, Is.True);
            Assert.That(_source.Releases, Is.EqualTo(new[] { Key }));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldKeepTheAsset_WhenOneOfTwoWaitingHoldersCancels() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Key);
            using var cancellation = new CancellationTokenSource();
            UniTask<TextAsset> cancelled = _loader.LoadAsync<TextAsset>(Key, cancellation.Token);
            UniTask<TextAsset> kept = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            cancellation.Cancel();
            bool wasCancelled = await cancelled.SuppressCancellationThrow().ContinueWith(result => result.IsCanceled);
            gate.TrySetResult();
            TextAsset loaded = await kept;

            Assert.That(wasCancelled, Is.True);
            Assert.That(loaded, Is.SameAs(_asset));
            Assert.That(_source.Releases, Is.Empty);
            Assert.That(_loader.LoadedCount, Is.EqualTo(1));
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldFailEveryWaitingHolder_WhenTheSharedLoadFails() => UniTask.ToCoroutine(async () =>
        {
            const string missing = "Missing";
            UniTaskCompletionSource gate = _source.Hold(missing);
            UniTask<bool> first = FailsAsync(_loader.LoadAsync<TextAsset>(missing, CancellationToken.None));
            UniTask<bool> second = FailsAsync(_loader.LoadAsync<TextAsset>(missing, CancellationToken.None));

            gate.TrySetResult();
            (bool firstFailed, bool secondFailed) = await UniTask.WhenAll(first, second);

            Assert.That(firstFailed, Is.True);
            Assert.That(secondFailed, Is.True);
            Assert.That(_loader.LoadedCount, Is.Zero);
        });

        [UnityTest]
        public IEnumerator Release_ShouldThrow_WhenTheLoadIsStillRunning() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Key);
            UniTask<TextAsset> first = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            UniTask<TextAsset> second = _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);

            Assert.Throws<InvalidOperationException>(() => _loader.Release<TextAsset>(Key));
            gate.TrySetResult();
            await UniTask.WhenAll(first, second);
            _loader.Release<TextAsset>(Key);
            _loader.Release<TextAsset>(Key);

            Assert.That(_source.Releases, Is.EqualTo(new[] { Key }));
            Assert.That(_loader.LoadedCount, Is.Zero);
        });

        [UnityTest]
        public IEnumerator LoadAsync_ShouldThrow_WhenTheLoaderIsDisposed() => UniTask.ToCoroutine(async () =>
        {
            _loader.Dispose();
            bool disposed = false;
            try
            {
                await _loader.LoadAsync<TextAsset>(Key, CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {
                disposed = true;
            }

            Assert.That(disposed, Is.True);
            Assert.That(_source.Loads, Is.Empty);
        });

        static async UniTask<bool> FailsAsync(UniTask<TextAsset> load)
        {
            try
            {
                await load;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }

            return false;
        }
    }
}

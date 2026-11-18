using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Assets;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine.TestTools;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class SpriteAtlasBinderTests
    {
        const string Tag = "UiAtlas";

        SpriteAtlas _atlas;
        FakeAssetSource _source;
        AssetLoader _assets;
        SpriteAtlasBinder _binder;

        [SetUp]
        public void SetUp()
        {
            _atlas = new SpriteAtlas();
            _source = new FakeAssetSource();
            _source.Add(Tag, _atlas);
            _assets = new AssetLoader(_source);
            _binder = new SpriteAtlasBinder(_assets);
        }

        [TearDown]
        public void TearDown()
        {
            _binder.Dispose();
            _assets.Dispose();
            Object.DestroyImmediate(_atlas);
        }

        [UnityTest]
        public IEnumerator BindAsync_ShouldHandTheAddressableAtlas_WhenUnityAsksForItsTag() => UniTask.ToCoroutine(async () =>
        {
            var bound = new List<SpriteAtlas>();

            await _binder.BindAsync(Tag, bound.Add);

            Assert.That(bound, Is.EqualTo(new[] { _atlas }));
            Assert.That(_source.Loads, Is.EqualTo(new[] { Tag }));
        });

        [UnityTest]
        public IEnumerator BindAsync_ShouldLoadOnce_WhenTheTagIsAskedAgain() => UniTask.ToCoroutine(async () =>
        {
            var bound = new List<SpriteAtlas>();

            await _binder.BindAsync(Tag, bound.Add);
            await _binder.BindAsync(Tag, bound.Add);

            Assert.That(bound, Has.Count.EqualTo(2));
            Assert.That(_source.Loads, Has.Count.EqualTo(1));
        });

        [Test]
        public void BindAsync_ShouldBindAtOnce_WhenTheAtlasWasPreloaded()
        {
            _binder.PreloadAsync(Tag, CancellationToken.None).GetAwaiter().GetResult();
            var bound = new List<SpriteAtlas>();

            UniTask binding = _binder.BindAsync(Tag, bound.Add);

            Assert.That(binding.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(bound, Is.EqualTo(new[] { _atlas }));
            Assert.That(_source.Loads, Has.Count.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator BindAsync_ShouldNotBind_WhenTheBinderIsDisposedWhileLoading() => UniTask.ToCoroutine(async () =>
        {
            UniTaskCompletionSource gate = _source.Hold(Tag);
            var binder = new SpriteAtlasBinder(_assets);
            var bound = new List<SpriteAtlas>();
            UniTask binding = binder.BindAsync(Tag, bound.Add);

            binder.Dispose();
            gate.TrySetResult();
            bool cancelled = await binding.SuppressCancellationThrow();

            Assert.That(cancelled, Is.True);
            Assert.That(bound, Is.Empty);
            Assert.That(_source.Releases, Is.EqualTo(new[] { Tag }));
        });

        [Test]
        public void BindAsync_ShouldKeepTheAtlasLoaded_WhenItsScopeIsAlive()
        {
            _binder.BindAsync(Tag, _ => { }).GetAwaiter().GetResult();
            Assert.That(_source.Releases, Is.Empty);

            _assets.Dispose();

            Assert.That(_source.Releases, Is.EqualTo(new[] { Tag }));
        }
    }
}

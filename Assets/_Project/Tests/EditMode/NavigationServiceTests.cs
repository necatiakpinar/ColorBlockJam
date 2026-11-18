using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class NavigationServiceTests
    {
        ControllableSceneLoader _loader;
        RecordingScreenFader _fader;
        NavigationService _navigation;

        [SetUp]
        public void SetUp()
        {
            _loader = new ControllableSceneLoader();
            _fader = new RecordingScreenFader();
            _navigation = new NavigationService(_loader, _fader);
        }

        [Test]
        public void GoToHome_ShouldFadeOutLoadAndFadeIn_WhenCalled()
        {
            _navigation.GoToHomeAsync().Forget();
            Assert.That(_fader.Log, Is.EqualTo(new[] { "fade out" }), "The screen is black while Home loads.");
            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Home }));

            _loader.CompleteLoad();

            Assert.That(_fader.Log, Is.EqualTo(new[] { "fade out", "fade in" }));
        }

        [Test]
        public void GoToHome_ShouldLoadNothing_WhileTheScreenGoesBlack()
        {
            _fader.HoldFadeOut();

            _navigation.GoToHomeAsync().Forget();
            Assert.That(_loader.Loads, Is.Empty);

            _fader.FinishFadeOut();
            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void GoToHome_ShouldBeIgnored_WhenAnotherRouteIsFadingOut()
        {
            _fader.HoldFadeOut();
            _navigation.GoToHomeAsync().Forget();

            _navigation.GoToGameplayAsync().Forget();
            _fader.FinishFadeOut();

            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void GoToHome_ShouldBringTheScreenBack_WhenTheLoadFails()
        {
            UniTask failed = _navigation.GoToHomeAsync();

            _loader.FailLoad(new InvalidOperationException("boom"));

            Assert.That(_fader.Log, Is.EqualTo(new[] { "fade out", "fade in" }));
            Assert.Throws<InvalidOperationException>(() => failed.GetAwaiter().GetResult());
        }

        [Test]
        public void GoToHome_ShouldLoadHomeScene_WhenCalled()
        {
            _navigation.GoToHomeAsync().Forget();

            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Home }));
        }

        [Test]
        public void GoToGameplay_ShouldLoadGameplayScene_WhenCalled()
        {
            _navigation.GoToGameplayAsync().Forget();

            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Gameplay }));
        }

        [Test]
        public void GoToGameplay_ShouldBeIgnored_WhenALoadIsRunning()
        {
            _navigation.GoToGameplayAsync().Forget();

            _navigation.GoToGameplayAsync().Forget();

            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Gameplay }));
        }

        [Test]
        public void IsLoading_ShouldBeTrueOnlyWhileASceneLoads_WhenRouted()
        {
            bool before = _navigation.IsLoading;
            _navigation.GoToGameplayAsync().Forget();
            bool during = _navigation.IsLoading;

            _loader.CompleteLoad();

            Assert.That(before, Is.False);
            Assert.That(during, Is.True);
            Assert.That(_navigation.IsLoading, Is.False);
        }

        [Test]
        public void GoToHome_ShouldLoad_WhenThePreviousLoadFinished()
        {
            _navigation.GoToGameplayAsync().Forget();
            _loader.CompleteLoad();

            _navigation.GoToHomeAsync().Forget();

            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Gameplay, SceneId.Home }));
        }

        [Test]
        public void GoToHome_ShouldLoad_WhenThePreviousLoadFailed()
        {
            UniTask failed = _navigation.GoToGameplayAsync();
            _loader.FailLoad(new InvalidOperationException("load failed"));

            _navigation.GoToHomeAsync().Forget();

            Assert.That(failed.Status, Is.EqualTo(UniTaskStatus.Faulted));
            Assert.Throws<InvalidOperationException>(() => failed.GetAwaiter().GetResult());
            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Gameplay, SceneId.Home }));
        }
    }
}

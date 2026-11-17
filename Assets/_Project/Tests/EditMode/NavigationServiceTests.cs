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
        NavigationService _navigation;

        [SetUp]
        public void SetUp()
        {
            _loader = new ControllableSceneLoader();
            _navigation = new NavigationService(_loader);
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
            Assert.That(_loader.Loads, Is.EqualTo(new[] { SceneId.Gameplay, SceneId.Home }));
        }
    }
}

using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class HomeFlowTests
    {
        TestViews _views;
        RecordingNavigation _navigation;
        PlayerProgressService _progress;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _navigation = new RecordingNavigation();
            _progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            _progress.Load();
        }

        [TearDown]
        public void TearDown() => _views.Dispose();

        [Test]
        public void Start_ShouldShowCurrentLevel_WhenSceneStarts()
        {
            _progress.AdvanceLevel();
            HomeView view = _views.CreateHomeView(out _);
            using var flow = new HomeFlow(view, _progress, _navigation);

            flow.Start();

            Assert.That(view.LevelLabelText, Is.EqualTo("Level 2"));
        }

        [Test]
        public void LevelButton_ShouldNavigateToGameplay_WhenTapped()
        {
            HomeView view = _views.CreateHomeView(out var levelButton);
            using var flow = new HomeFlow(view, _progress, _navigation);

            levelButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Gameplay }));
        }

        [Test]
        public void LevelButton_ShouldNotNavigate_WhenFlowIsDisposed()
        {
            HomeView view = _views.CreateHomeView(out var levelButton);
            new HomeFlow(view, _progress, _navigation).Dispose();

            levelButton.onClick.Invoke();

            Assert.That(_navigation.Requests, Is.Empty);
        }
    }
}

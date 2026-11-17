using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Scenes;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Debugging;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class GameplayDebugCommandsTests
    {
        const int TimerSeconds = 60;

        TestCatalog _catalogs;
        SignalBus _bus;
        LevelSession _session;
        LevelTimer _timer;
        FakeGameClock _clock;
        WalletService _wallet;
        BoosterConfig _config;
        BoosterInventory _boosters;
        PlayerProgressService _progress;
        RecordingNavigation _navigation;
        GameplayDebugCommands _commands;

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            var levels = new LevelProvider(_catalogs.Create(new LevelDataBuilder(4, 4).Timer(TimerSeconds)
                .Block(Red, 1, 1).Door(Left, 1, 1, Red).Build()));
            _session = new LevelSession(new ActiveBoard(new BoardSpace(1f), _bus), levels, _bus);
            _clock = new FakeGameClock();
            _timer = new LevelTimer(_session, _clock, _bus);
            _wallet = new WalletService(new InMemoryKeyValueStorage(), new RecordingLogger(), _bus);
            _wallet.Load();
            _progress = new PlayerProgressService(new InMemoryKeyValueStorage(), new RecordingLogger());
            _progress.Load();
            _navigation = new RecordingNavigation();
            _config = ScriptableObject.CreateInstance<BoosterConfig>();
            _boosters = new BoosterInventory(new InMemoryKeyValueStorage(), new RecordingLogger(), _config, _bus);
            _boosters.Load();
            _commands = new GameplayDebugCommands(_session, _timer, _wallet, _progress, _navigation, _bus, _boosters);
            _session.Start(1);
        }

        [TearDown]
        public void TearDown()
        {
            _timer.Dispose();
            _session.Dispose();
            _catalogs.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Win_ShouldCompleteTheLevel_WhenItIsPlaying()
        {
            int completed = 0;
            _bus.Subscribe<LevelCompletedSignal>(_ => completed++);

            Assert.That(_commands.Win(), Is.True);
            Assert.That(_commands.State, Is.EqualTo(LevelState.Won));
            Assert.That(completed, Is.EqualTo(1));
        }

        [Test]
        public void Win_ShouldBeRefused_WhenTheLevelIsNotPlaying()
        {
            _session.Pause();

            Assert.That(_commands.Win(), Is.False);
            Assert.That(_commands.State, Is.EqualTo(LevelState.Paused));
        }

        [Test]
        public void RunOutOfTime_ShouldFailTheLevelWithAnEmptyTimer_WhenItIsPlaying()
        {
            int failed = 0;
            _bus.Subscribe<LevelFailedSignal>(_ => failed++);

            Assert.That(_commands.RunOutOfTime(), Is.True);
            Assert.That(_commands.State, Is.EqualTo(LevelState.Failed));
            Assert.That(_commands.SecondsLeft, Is.Zero);
            Assert.That(failed, Is.EqualTo(1));
        }

        [Test]
        public void SetSecondsLeft_ShouldPutTheTimerThereAndKeepCounting_WhenTheLevelIsRunning()
        {
            _bus.Fire(new BlockGrabbedSignal(0));

            _commands.SetSecondsLeft(5f);
            _clock.Advance(5f);

            Assert.That(_commands.State, Is.EqualTo(LevelState.Failed), "Counting went on and ran out.");
        }

        [Test]
        public void Coins_ShouldGoThroughTheWallet_WhenAddedAndSpent()
        {
            _commands.AddCoins(500);

            Assert.That(_commands.SpendCoins(200), Is.True);
            Assert.That(_commands.SpendCoins(1000), Is.False);
            Assert.That(_commands.Coins, Is.EqualTo(300));
        }

        [Test]
        public void Boosters_ShouldGoThroughTheInventory_WhenAddedAndCleared()
        {
            _commands.AddBooster(BoosterType.Hammer, 4);
            int added = _commands.BoosterCount(BoosterType.Hammer);

            _commands.ClearBooster(BoosterType.Hammer);

            Assert.That(added, Is.EqualTo(5));
            Assert.That(_boosters.Count(BoosterType.Hammer), Is.Zero);
        }

        [Test]
        public void PlayLevel_ShouldSaveTheLevelAndReloadTheScene_WhenAsked()
        {
            _commands.PlayLevel(4);

            Assert.That(_progress.CurrentLevel, Is.EqualTo(4));
            Assert.That(_navigation.Requests, Is.EqualTo(new[] { SceneId.Gameplay }));
        }
    }
}

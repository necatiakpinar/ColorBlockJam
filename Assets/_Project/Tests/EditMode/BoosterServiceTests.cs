using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Boosters.Commands;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class BoosterServiceTests
    {
        const int TimerSeconds = 30;

        TestCatalog _catalogs;
        SignalBus _bus;
        ActiveBoard _board;
        LevelSession _session;
        BoosterRig _rig;
        readonly List<string> _events = new();

        [SetUp]
        public void SetUp()
        {
            _catalogs = new TestCatalog();
            _bus = new SignalBus();
            _board = new ActiveBoard(new BoardSpace(1f), _bus);
            var levels = new LevelProvider(_catalogs.Create(new LevelDataBuilder(4, 4).Timer(TimerSeconds)
                .Block(Red, 0, 0, 1, 0).Block(Blue, 3, 3).Block(Red, 3, 0)
                .Door(Left, 3, 1, Red).Door(Top, 0, 1, Blue).Build()));
            _session = new LevelSession(_board, levels, _bus);
            _rig = new BoosterRig(_board, _session, _bus);
            _session.Start(1);
            _events.Clear();
            _bus.Subscribe<BoosterTargetingChangedSignal>(signal => _events.Add($"targeting {signal.Type} {signal.Active}"));
            _bus.Subscribe<BoosterUsedSignal>(signal => _events.Add($"used {signal.Type}"));
            _bus.Subscribe<LevelCompletedSignal>(_ => _events.Add("won"));
        }

        [TearDown]
        public void TearDown()
        {
            _rig.Dispose();
            _session.Dispose();
            _catalogs.Dispose();
        }

        void Tap(int x, int y) =>
            _bus.Fire(new BoardPointerSignal(BoardPointerPhase.Pressed, new Vector2Int(x, y), Vector3.zero));

        BoosterService Service => _rig.Service;

        [Test]
        public void Freeze_ShouldStopTheTimerAtOnceAndSpendOne_WhenUsed()
        {
            _bus.Fire(new BlockGrabbedSignal(0));
            _rig.Clock.Advance(2f);

            BoosterUse use = Service.Activate(BoosterType.Freeze);
            _rig.Clock.Advance(5f);

            Assert.That(use, Is.EqualTo(BoosterUse.Used));
            Assert.That(_rig.Timer.SecondsLeft, Is.EqualTo(TimerSeconds - 2f).Within(1e-4f));
            Assert.That(_rig.Inventory.Count(BoosterType.Freeze), Is.Zero);
            Assert.That(_events, Is.EqualTo(new[] { "used Freeze" }));
        }

        [Test]
        public void Freeze_ShouldBeRefused_WhileAFreezeIsRunning()
        {
            Service.Activate(BoosterType.Freeze);
            _rig.Inventory.Add(BoosterType.Freeze, 1);

            Assert.That(Service.Activate(BoosterType.Freeze), Is.EqualTo(BoosterUse.Refused));
            Assert.That(_rig.Inventory.Count(BoosterType.Freeze), Is.EqualTo(1));
        }

        [Test]
        public void TargetedBooster_ShouldPauseTheLevelUntilATargetIsTapped_WhenActivated()
        {
            BoosterUse use = Service.Activate(BoosterType.Hammer);

            Assert.That(use, Is.EqualTo(BoosterUse.Targeting));
            Assert.That(Service.IsTargeting, Is.True);
            Assert.That(_session.State, Is.EqualTo(LevelState.Paused));
            Assert.That(_rig.Inventory.Count(BoosterType.Hammer), Is.EqualTo(1), "Nothing is spent before a target.");
        }

        [Test]
        public void Hammer_ShouldSmashTheTappedBlockAndResume_WhenABlockIsTapped()
        {
            Service.Activate(BoosterType.Hammer);

            Tap(1, 0);

            Assert.That(_board.Current.Blocks[0].HasExited, Is.True);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_rig.Inventory.Count(BoosterType.Hammer), Is.Zero);
            Assert.That(_events, Is.EqualTo(new[] { "targeting Hammer True", "targeting Hammer False", "used Hammer" }));
        }

        [Test]
        public void Targeting_ShouldIgnoreTheTap_WhenItHitsAnEmptyCell()
        {
            Service.Activate(BoosterType.Hammer);

            Tap(2, 2);

            Assert.That(Service.IsTargeting, Is.True);
            Assert.That(_board.Current.RemainingBlocks, Is.EqualTo(3));
        }

        [Test]
        public void Rocket_ShouldBlastOnlyTheTappedCell_WhenUsed()
        {
            Service.Activate(BoosterType.Rocket);

            Tap(1, 0);

            BoardBlock block = _board.Current.Blocks[0];
            Assert.That(block.HasExited, Is.False);
            Assert.That(block.StartCells, Is.EqualTo(new[] { new Vector2Int(0, 0) }));
        }

        [Test]
        public void Vacuum_ShouldRemoveEveryBlockOfTheTappedColour_WhenUsed()
        {
            Service.Activate(BoosterType.Vacuum);

            Tap(3, 0);

            Assert.That(_board.Current.Blocks[0].HasExited, Is.True);
            Assert.That(_board.Current.Blocks[2].HasExited, Is.True);
            Assert.That(_board.Current.Blocks[1].HasExited, Is.False);
        }

        [Test]
        public void Booster_ShouldWinTheLevel_WhenItRemovesTheLastBlocks()
        {
            Service.Activate(BoosterType.Hammer);
            Tap(3, 3);

            Service.Activate(BoosterType.Vacuum);
            Tap(0, 0);

            Assert.That(_session.State, Is.EqualTo(LevelState.Won));
            Assert.That(_events, Has.Exactly(1).EqualTo("won"));
        }

        [Test]
        public void CancelTargeting_ShouldKeepTheBoosterAndResume_WhenThePlayerChangesTheirMind()
        {
            Service.Activate(BoosterType.Vacuum);

            Service.CancelTargeting();

            Assert.That(Service.IsTargeting, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_rig.Inventory.Count(BoosterType.Vacuum), Is.EqualTo(1));
        }

        [Test]
        public void Activate_ShouldAskForAPurchase_WhenNoneAreLeft()
        {
            _rig.UseUp(BoosterType.Rocket);

            Assert.That(Service.Activate(BoosterType.Rocket), Is.EqualTo(BoosterUse.NeedsPurchase));
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [Test]
        public void Activate_ShouldBeRefused_WhenTheLevelIsNotPlaying()
        {
            _session.Pause();

            Assert.That(Service.Activate(BoosterType.Hammer), Is.EqualTo(BoosterUse.Refused));
            Assert.That(Service.Activate(BoosterType.Freeze), Is.EqualTo(BoosterUse.Refused));
        }

        [Test]
        public void Restart_ShouldDropTheTarget_WhenABoosterWaits()
        {
            Service.Activate(BoosterType.Hammer);

            _session.Restart();

            Assert.That(Service.IsTargeting, Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
            Assert.That(_events[^1], Is.EqualTo("targeting Hammer False"));
        }

        [Test]
        public void Constructor_ShouldThrow_WhenTwoCommandsShareABooster()
        {
            var commands = new IBoosterCommand[] { new RocketBoosterCommand(_board), new RocketBoosterCommand(_board) };

            Assert.Throws<System.ArgumentException>(() => new BoosterService(_board, _session, _rig.Inventory, commands, _bus));
        }
    }
}

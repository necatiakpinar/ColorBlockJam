using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Tests.EditMode.Helpers;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The HUD shows what the gameplay signals announce.</summary>
    public sealed class GameplayHudViewTests
    {
        TestViews _views;
        SignalBus _bus;
        GameplayHudView _hud;

        [SetUp]
        public void SetUp()
        {
            _views = new TestViews();
            _bus = new SignalBus();
            _hud = _views.CreateHud(_bus, out _);
        }

        [TearDown]
        public void TearDown() => _views.Dispose();

        [TestCase(150, "02:30")]
        [TestCase(59, "00:59")]
        [TestCase(0, "00:00")]
        [TestCase(600, "10:00")]
        public void Timer_ShouldShowMinutesAndSeconds_WhenTheTimerChanges(int secondsLeft, string expected)
        {
            _bus.Fire(new TimerChangedSignal(secondsLeft));

            Assert.That(_hud.TimerLabelText, Is.EqualTo(expected));
        }
    }
}

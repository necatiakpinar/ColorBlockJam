using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Lifecycle;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>The real Gameplay scene counts the level's time down on the HUD.</summary>
    public sealed class GameplayTimerTests
    {
        IObjectResolver _container;

        IEnumerator BootToGameplay()
        {
            yield return SceneTestUtility.BootToGameplay();
            _container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
        }

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        static string Format(int seconds) => $"{seconds / 60:00}:{seconds % 60:00}";

        [UnityTest]
        public IEnumerator Hud_ShouldCountTheTimerDown_OnceABlockIsGrabbed()
        {
            yield return BootToGameplay();
            var timer = _container.Resolve<LevelTimer>();
            var hud = SceneTestUtility.FindInActiveScene<GameplayHudView>();
            int startShown = Mathf.CeilToInt(timer.SecondsLeft);
            yield return WaitRealtime(0.5f);
            float untouched = timer.SecondsLeft;

            _container.Resolve<SignalBus>().Fire(new BlockGrabbedSignal(0));
            yield return WaitRealtime(1.2f);

            Assert.That(untouched, Is.EqualTo(startShown), "Waits for the first grab.");
            Assert.That(startShown, Is.GreaterThan(1));
            Assert.That(timer.SecondsLeft, Is.LessThan(startShown - 1f));
            Assert.That(hud.TimerLabelText, Is.EqualTo(Format(Mathf.CeilToInt(timer.SecondsLeft))));
        }

        [UnityTest]
        public IEnumerator Timer_ShouldStandStill_WhileTheAppIsInTheBackground()
        {
            yield return BootToGameplay();
            var timer = _container.Resolve<LevelTimer>();
            // Unity's own background event, delivered to the reporter on the Root scope.
            var reporter = _container.Resolve<ApplicationPauseReporter>();
            _container.Resolve<SignalBus>().Fire(new BlockGrabbedSignal(0));
            yield return WaitRealtime(0.2f);

            reporter.SendMessage("OnApplicationPause", true);
            float frozen = timer.SecondsLeft;
            yield return WaitRealtime(0.5f);
            float after = timer.SecondsLeft;
            reporter.SendMessage("OnApplicationPause", false);

            Assert.That(after, Is.EqualTo(frozen));
        }
    }
}

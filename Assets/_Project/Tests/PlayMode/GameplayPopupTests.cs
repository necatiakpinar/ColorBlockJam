using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class GameplayPopupTests
    {
        LevelSession _session;

        IEnumerator BootToGameplay()
        {
            yield return SceneTestUtility.BootToGameplay();
            _session = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container.Resolve<LevelSession>();
        }

        static bool IsShown<TPopup>() where TPopup : Component =>
            SceneTestUtility.FindInActiveScene<TPopup>().gameObject.activeInHierarchy;

        [UnityTest]
        public IEnumerator Pause_ShouldShowTheSettingsPopupAndResumeOnClose_WhenTapped()
        {
            yield return BootToGameplay();

            SceneTestUtility.TapButton<GameplayHudView>("PauseButton");
            bool shown = IsShown<PausePopupView>();
            LevelState whileOpen = _session.State;
            SceneTestUtility.TapButton<PausePopupView>("CloseButton");

            Assert.That(shown, Is.True);
            Assert.That(whileOpen, Is.EqualTo(LevelState.Paused));
            Assert.That(IsShown<PausePopupView>(), Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }

        [UnityTest]
        public IEnumerator Loss_ShouldShowLevelFailedThenRetry_WhenTheTimerRunsOut()
        {
            yield return BootToGameplay();

            _session.Fail();
            bool failed = IsShown<LevelFailedPopupView>();
            SceneTestUtility.TapButton<LevelFailedPopupView>("RetryButton");

            Assert.That(failed, Is.True);
            Assert.That(IsShown<LevelFailedPopupView>(), Is.False);
            Assert.That(_session.State, Is.EqualTo(LevelState.Playing));
        }
    }
}

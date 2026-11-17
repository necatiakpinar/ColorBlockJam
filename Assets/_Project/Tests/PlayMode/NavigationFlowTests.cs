using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Home;
using UnityEngine.TestTools;

namespace ColorBlockJam.Tests.PlayMode
{
    public sealed class NavigationFlowTests
    {
        [UnityTest]
        public IEnumerator Splash_ShouldReachHomeAndShowLevel_WhenAppBoots()
        {
            yield return SceneTestUtility.BootToHome();

            StringAssert.StartsWith("Level ", SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText);
        }

        [UnityTest]
        public IEnumerator LevelButton_ShouldOpenGameplayShowingTheSameLevel_WhenTappedOnHome()
        {
            yield return SceneTestUtility.BootToHome();
            string homeLabel = SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText;

            SceneTestUtility.TapButton<HomeView>("LevelButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.GameplayScene);

            Assert.That("Level " + SceneTestUtility.FindInActiveScene<GameplayHudView>().LevelLabelText, Is.EqualTo(homeLabel));
        }

        [UnityTest]
        public IEnumerator PausePopupHome_ShouldReturnToHome_WhenTappedInGameplay()
        {
            yield return SceneTestUtility.BootToGameplay();

            SceneTestUtility.TapButton<GameplayHudView>("PauseButton");
            SceneTestUtility.TapButton<PausePopupView>("HomeButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);

            StringAssert.StartsWith("Level ", SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using RollicCase.Game.Gameplay.Hud;
using RollicCase.Game.Home;
using UnityEngine.TestTools;

namespace RollicCase.Tests.PlayMode
{
    /// <summary>End to end: the real boot reaches Home, and Home and Gameplay route to each other.</summary>
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

            Assert.That(SceneTestUtility.FindInActiveScene<GameplayHudView>().LevelLabelText, Is.EqualTo(homeLabel));
        }

        [UnityTest]
        public IEnumerator HomeButton_ShouldReturnToHome_WhenTappedInGameplay()
        {
            yield return SceneTestUtility.BootToGameplay();

            SceneTestUtility.TapButton<GameplayHudView>("HomeButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);

            StringAssert.StartsWith("Level ", SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText);
        }
    }
}

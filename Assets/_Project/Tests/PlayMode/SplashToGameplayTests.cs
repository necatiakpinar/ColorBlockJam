using System.Collections;
using NUnit.Framework;
using RollicCase.Game.Gameplay.Hud;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RollicCase.Tests.PlayMode
{
    /// <summary>End-to-end boot: the real Splash scene must hand over to Gameplay and show the level.</summary>
    public sealed class SplashToGameplayTests
    {
        const string SplashScene = "Splash";
        const string GameplayScene = "Gameplay";
        const float TimeoutSeconds = 15f;

        [UnityTest]
        public IEnumerator Splash_ShouldReachGameplayAndShowLevel_WhenAppBoots()
        {
            SceneManager.LoadScene(SplashScene, LoadSceneMode.Single);

            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (SceneManager.GetActiveScene().name != GameplayScene)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Splash never reached Gameplay.");
                yield return null;
            }

            // One frame for the gameplay scope's entry points to start.
            yield return null;

            GameplayHudView hud = FindInActiveScene<GameplayHudView>();
            Assert.That(hud, Is.Not.Null, "Gameplay scene has no HUD.");
            StringAssert.StartsWith("Level ", hud.LevelLabelText);
        }

        static T FindInActiveScene<T>() where T : Component
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }
    }
}

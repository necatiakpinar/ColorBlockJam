using System.Collections;
using NUnit.Framework;
using RollicCase.Game.Home;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RollicCase.Tests.PlayMode
{
    /// <summary>Drives the real app through its scenes the way a player would.</summary>
    public static class SceneTestUtility
    {
        public const string SplashScene = "Splash";
        public const string HomeScene = "Home";
        public const string GameplayScene = "Gameplay";
        const float TimeoutSeconds = 15f;

        public static IEnumerator WaitForScene(string sceneName)
        {
            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (SceneManager.GetActiveScene().name != sceneName)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), $"Never reached {sceneName}.");
                yield return null;
            }

            // One frame for the scene scope's entry points to start.
            yield return null;
        }

        public static IEnumerator BootToHome()
        {
            // LoadScene takes effect next frame; waiting for Splash first keeps a Home scene left over from
            // the previous test from passing the wait below.
            SceneManager.LoadScene(SplashScene, LoadSceneMode.Single);
            yield return WaitForScene(SplashScene);
            yield return WaitForScene(HomeScene);
        }

        public static IEnumerator BootToGameplay()
        {
            yield return BootToHome();
            TapButton<HomeView>("LevelButton");
            yield return WaitForScene(GameplayScene);
        }

        /// <summary>Taps the button named <paramref name="buttonName"/> under the view of type T.</summary>
        public static void TapButton<TView>(string buttonName) where TView : Component
        {
            FindButton<TView>(buttonName).onClick.Invoke();
        }

        public static Button FindButton<TView>(string buttonName) where TView : Component
        {
            foreach (Button button in FindInActiveScene<TView>().GetComponentsInChildren<Button>(true))
                if (button.name == buttonName)
                    return button;

            Assert.Fail($"{typeof(TView).Name} has no button named {buttonName}.");
            return null;
        }

        public static T FindInActiveScene<T>() where T : Component
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            Assert.Fail($"{SceneManager.GetActiveScene().name} has no {typeof(T).Name}.");
            return null;
        }
    }
}

using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Tests.PlayMode
{
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

            yield return null;
            while (Navigation().IsLoading)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), $"{sceneName} never came out of the fade.");
                yield return null;
            }
        }

        static INavigationService Navigation() => FindInActiveScene<LifetimeScope>().Container.Resolve<INavigationService>();

        public static IEnumerator BootToHome()
        {
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

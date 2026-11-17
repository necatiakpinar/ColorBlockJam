using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Navigation;
using ColorBlockJam.Game.Scopes;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Editor.LevelEditing
{
    /// <summary>
    /// Plays a level straight from the level editor (T-30, level-editor.md §2): saves it as the player's current
    /// level, enters play mode through Splash as always, and once Home is up opens Gameplay on its own, so the
    /// designer lands in the level they were editing. Editor-only; the game has no play-test code.
    /// </summary>
    [InitializeOnLoad]
    public static class LevelPlayTest
    {
        const string PendingKey = "ColorBlockJam.LevelEditor.PlayTestPending";

        static LevelPlayTest()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void Play(int levelNumber)
        {
            PlayerPrefs.SetString(PlayerProgressService.StorageKey,
                JsonUtility.ToJson(new PlayerProgressData { CurrentLevel = levelNumber }));
            PlayerPrefs.Save();
            EditorPrefs.SetBool(PendingKey, true);
            EditorApplication.isPlaying = true;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!EditorPrefs.GetBool(PendingKey, false) || scene.name != "Home")
                return;
            EditorPrefs.SetBool(PendingKey, false);
            LifetimeScope home = LifetimeScope.Find<HomeLifetimeScope>(scene);
            OpenGameplayAsync(home.Container.Resolve<INavigationService>()).Forget();
        }

        /// <summary>Home is loaded while the route that loads it is still finishing; wait for it, then play.</summary>
        static async UniTaskVoid OpenGameplayAsync(INavigationService navigation)
        {
            while (navigation.IsLoading)
                await UniTask.Yield();
            await navigation.GoToGameplayAsync();
        }
    }
}

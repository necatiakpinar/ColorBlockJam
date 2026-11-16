using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RollicCase.Editor
{
    /// <summary>
    /// Pressing Play always boots through the first build scene (Splash), whichever scene is open,
    /// so the app is never entered in a state a player can't reach. Toggle from the menu.
    /// The start scene is set only while entering Play Mode and cleared on return, because the test
    /// runner enters Play Mode from its own init scene and never starts when a start scene is forced.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStartScene
    {
        const string MenuPath = "RollicCase/Play From Splash";
        const string PrefKey = "RollicCase.PlayFromSplash";
        const string TestRunnerScenePrefix = "InitTestScene";

        static PlayModeStartScene()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            IsEnabled = !IsEnabled;
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
                EditorSceneManager.playModeStartScene = ShouldForceSplash() ? FirstBuildScene() : null;
            else if (change == PlayModeStateChange.EnteredEditMode)
                EditorSceneManager.playModeStartScene = null;
        }

        static bool ShouldForceSplash() =>
            IsEnabled
            && !Application.isBatchMode
            && EditorBuildSettings.scenes.Length > 0
            && !SceneManager.GetActiveScene().name.StartsWith(TestRunnerScenePrefix);

        static SceneAsset FirstBuildScene() =>
            AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
    }
}

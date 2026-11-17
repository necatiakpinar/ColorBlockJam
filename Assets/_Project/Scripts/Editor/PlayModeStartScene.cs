using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.Editor
{
    [InitializeOnLoad]
    public static class PlayModeStartScene
    {
        const string MenuPath = "ColorBlockJam/Play From Splash";
        const string PrefKey = "ColorBlockJam.PlayFromSplash";
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

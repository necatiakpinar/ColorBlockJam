using UnityEditor;
using UnityEditor.SceneManagement;

namespace RollicCase.Editor
{
    /// <summary>
    /// Pressing Play always boots through the first build scene (Splash), whichever scene is open,
    /// so the app is never entered in a state a player can't reach. Toggle from the menu.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStartScene
    {
        const string MenuPath = "RollicCase/Play From Splash";
        const string PrefKey = "RollicCase.PlayFromSplash";

        static PlayModeStartScene()
        {
            EditorApplication.delayCall += Apply;
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
            Apply();
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        static void Apply()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            EditorSceneManager.playModeStartScene = IsEnabled && scenes.Length > 0
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[0].path)
                : null;
        }
    }
}

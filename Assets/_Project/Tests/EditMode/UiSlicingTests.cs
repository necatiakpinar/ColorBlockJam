using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class UiSlicingTests
    {
        const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        const float AspectTolerance = 0.06f;
        static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/Splash.unity", "Assets/_Project/Scenes/Home.unity", "Assets/_Project/Scenes/Gameplay.unity",
        };
        static readonly HashSet<string> Gradients = new() { "bg_home_line" };
        static readonly HashSet<string> NativeHeight = new() { "Btn_popup_blue", "Btn_popup_green", "Btn_levelBg_normal" };

        static IEnumerable<string> Sources()
        {
            foreach (string scene in Scenes)
                yield return scene;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
                yield return AssetDatabase.GUIDToAssetPath(guid);
        }

        static List<string> Problems(string path)
        {
            var problems = new List<string>();
            if (path.EndsWith(".unity"))
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool openedHere = !scene.isLoaded;
                if (openedHere)
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (GameObject root in scene.GetRootGameObjects())
                    Collect(root, problems);
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
                return problems;
            }

            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            Collect(prefab, problems);
            PrefabUtility.UnloadPrefabContents(prefab);
            return problems;
        }

        static void Collect(GameObject root, List<string> problems)
        {
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite == null)
                    continue;
                Vector2 rect = image.rectTransform.rect.size;
                if (rect.x <= 0f || rect.y <= 0f)
                    continue;
                string problem = Check(image, rect);
                if (problem != null)
                    problems.Add($"{image.name} ({image.sprite.name}): {problem}");
            }
        }

        static string Check(Image image, Vector2 rect)
        {
            Vector4 border = image.sprite.border;
            Vector2 sprite = image.sprite.rect.size;
            if (image.type == Image.Type.Sliced)
            {
                if (border == Vector4.zero)
                    return "sliced without borders";
                float multiplier = image.pixelsPerUnitMultiplier;
                if ((border.x + border.z) / multiplier > rect.x + 1f || (border.y + border.w) / multiplier > rect.y + 1f)
                    return $"borders do not fit {rect} at multiplier {multiplier}";
                float native = sprite.y / rect.y;
                if (NativeHeight.Contains(image.sprite.name) && Mathf.Abs(multiplier - native) > 0.01f)
                    return $"multiplier {multiplier} squeezes its vertical gradient; use {native:0.000}";
                return null;
            }

            if (image.type != Image.Type.Simple || image.preserveAspect || Gradients.Contains(image.sprite.name))
                return null;
            float stretch = rect.x / rect.y / (sprite.x / sprite.y);
            return Mathf.Abs(stretch - 1f) > AspectTolerance ? $"stretched {stretch:0.00}x" : null;
        }

        [TestCaseSource(nameof(Sources))]
        public void Images_ShouldSliceOrKeepTheirAspect_WhenDrawn(string path)
        {
            Assert.That(Problems(path), Is.Empty, path);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class TextStyleTests
    {
        const string FontFolder = "Assets/_Project/Art/Fonts/LilitaOne/";
        const string FontAssetName = "LilitaOne SDF";
        static readonly HashSet<string> Styles = new() { "LilitaOne SDF - Outlined", "LilitaOne SDF - Plain" };
        static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/Splash.unity", "Assets/_Project/Scenes/Home.unity", "Assets/_Project/Scenes/Gameplay.unity",
        };

        static IEnumerable<string> Sources()
        {
            foreach (string scene in Scenes)
                yield return scene;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }))
                yield return AssetDatabase.GUIDToAssetPath(guid);
        }

        static List<string> OffStyle(GameObject root)
        {
            var problems = new List<string>();
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string material = text.fontSharedMaterial != null ? text.fontSharedMaterial.name : "none";
                if (text.font == null || text.font.name != FontAssetName || !Styles.Contains(material))
                    problems.Add($"{text.transform.parent?.name}/{text.name}: {(text.font != null ? text.font.name : "no font")} / {material}");
            }

            return problems;
        }

        [TestCaseSource(nameof(Sources))]
        public void Texts_ShouldUseTheGameFontInOneOfItsStyles_WhenDrawn(string path)
        {
            var problems = new List<string>();
            if (path.EndsWith(".unity"))
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool openedHere = !scene.isLoaded;
                if (openedHere)
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (GameObject root in scene.GetRootGameObjects())
                    problems.AddRange(OffStyle(root));
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
            else
            {
                problems.AddRange(OffStyle(AssetDatabase.LoadAssetAtPath<GameObject>(path)));
            }

            Assert.That(problems, Is.Empty, path);
        }

        [Test]
        public void NewTexts_ShouldStartInTheGameFont_WhenCreated()
        {
            Assert.That(TMP_Settings.defaultFontAsset.name, Is.EqualTo(FontAssetName));
        }

        [Test]
        public void GameFont_ShouldFallBackForLettersItLacks_WhenDrawn()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontFolder + FontAssetName + ".asset");

            Assert.That(font.fallbackFontAssetTable, Is.Not.Empty);
            Assert.That(font.HasCharacters("0123456789Level Time:k+-!?"), Is.True);
        }

        [Test]
        public void OutlinedStyle_ShouldDrawAnOutlineAndADropShadow_WhenUsed()
        {
            var outlined = AssetDatabase.LoadAssetAtPath<Material>(FontFolder + "LilitaOne SDF - Outlined.mat");

            Assert.That(outlined.IsKeywordEnabled("OUTLINE_ON"), Is.True);
            Assert.That(outlined.IsKeywordEnabled("UNDERLAY_ON"), Is.True);
            Assert.That(outlined.GetFloat("_OutlineWidth"), Is.GreaterThan(0.15f));
            Assert.That(outlined.GetFloat("_UnderlayOffsetY"), Is.LessThan(0f));
        }
    }
}

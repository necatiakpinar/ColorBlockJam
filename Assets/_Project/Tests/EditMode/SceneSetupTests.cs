using System;
using System.Collections.Generic;
using NUnit.Framework;
using RollicCase.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RollicCase.Tests.EditMode
{
    /// <summary>Opens every build scene and checks the UI rules authored scenes must follow (ui.md).</summary>
    public sealed class SceneSetupTests
    {
        static readonly Vector2 ReferenceResolution = new(1080f, 1920f);
        const float MatchWidthOrHeight = 0.5f;

        static IEnumerable<string> BuildScenePaths()
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    yield return scene.path;
        }

        /// <summary>Keeps a build scene open for one test; closes it again unless it was already open.</summary>
        sealed class OpenedScene : IDisposable
        {
            readonly Scene _scene;
            readonly bool _openedHere;

            public OpenedScene(string path)
            {
                _scene = SceneManager.GetSceneByPath(path);
                _openedHere = !_scene.isLoaded;
                if (_openedHere)
                    _scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            public List<T> FindAll<T>() where T : Component
            {
                var found = new List<T>();
                foreach (GameObject root in _scene.GetRootGameObjects())
                    found.AddRange(root.GetComponentsInChildren<T>(true));
                return found;
            }

            public void Dispose()
            {
                if (_openedHere)
                    EditorSceneManager.CloseScene(_scene, true);
            }
        }

        [TestCaseSource(nameof(BuildScenePaths))]
        public void Canvases_ShouldScaleFromReferenceResolution_WhenInBuildScene(string scenePath)
        {
            using var scene = new OpenedScene(scenePath);

            foreach (Canvas canvas in scene.FindAll<Canvas>())
            {
                if (!canvas.isRootCanvas)
                    continue;

                var scaler = canvas.GetComponent<CanvasScaler>();
                Assert.That(scaler, Is.Not.Null, canvas.name);
                Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize), scaler.name);
                Assert.That(scaler.referenceResolution, Is.EqualTo(ReferenceResolution), scaler.name);
                Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(MatchWidthOrHeight), scaler.name);
            }
        }

        [TestCaseSource(nameof(BuildScenePaths))]
        public void Canvases_ShouldKeepContentInsideSafeArea_WhenInBuildScene(string scenePath)
        {
            using var scene = new OpenedScene(scenePath);

            foreach (Canvas canvas in scene.FindAll<Canvas>())
                if (canvas.isRootCanvas)
                    Assert.That(canvas.GetComponentInChildren<SafeAreaFitter>(true), Is.Not.Null, canvas.name);
        }

        [TestCaseSource(nameof(BuildScenePaths))]
        public void Buttons_ShouldGivePressFeedback_WhenInBuildScene(string scenePath)
        {
            using var scene = new OpenedScene(scenePath);

            foreach (Button button in scene.FindAll<Button>())
            {
                var feedback = button.GetComponent<ButtonPressFeedback>();
                Assert.That(feedback, Is.Not.Null, button.name);

                var serialized = new SerializedObject(feedback);
                Assert.That(serialized.FindProperty("_config").objectReferenceValue, Is.Not.Null, button.name);
                Assert.That(serialized.FindProperty("_button").objectReferenceValue, Is.SameAs(button), button.name);
                Assert.That(serialized.FindProperty("_target").objectReferenceValue, Is.Not.Null, button.name);
            }
        }
    }
}

using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class SplashLayoutTests
    {
        const string SplashScene = "Assets/_Project/Scenes/Splash.unity";
        const string AtlasPath = "Assets/_Project/Art/UI/UiAtlas.spriteatlas";

        static Scene Open(out bool openedHere)
        {
            Scene scene = SceneManager.GetSceneByPath(SplashScene);
            openedHere = !scene.isLoaded;
            return openedHere ? EditorSceneManager.OpenScene(SplashScene, OpenSceneMode.Additive) : scene;
        }

        static void Close(Scene scene, bool openedHere)
        {
            if (openedHere)
                EditorSceneManager.CloseScene(scene, true);
        }

        static Transform SafeArea(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = root.transform.Find("SafeArea");
                if (found != null)
                    return found;
            }

            Assert.Fail("The splash has no SafeArea.");
            return null;
        }

        [Test]
        public void ProgressFill_ShouldGrowFromTheLeftEdge_WhenTheBarFills()
        {
            Scene scene = Open(out bool openedHere);
            var fill = (RectTransform)SafeArea(scene).Find("ProgressBar/Fill");
            var image = fill.GetComponent<Image>();

            Assert.That(fill.anchorMin.x, Is.Zero);
            Assert.That(fill.anchorMin.y, Is.Zero);
            Assert.That(fill.anchorMax.y, Is.EqualTo(1f));
            Assert.That(image.type, Is.Not.EqualTo(Image.Type.Filled), "The fill grows by its right anchor, not by fill amount.");
            Close(scene, openedHere);
        }

        [Test]
        public void SplashImages_ShouldUseNoAtlasSprite_WhenTheAtlasIsNotLoadedYet()
        {
            Scene scene = Open(out bool openedHere);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);

            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
                if (image.sprite != null)
                    Assert.That(atlas.CanBindTo(image.sprite), Is.False, $"{image.name} uses {image.sprite.name}");

            Close(scene, openedHere);
        }
    }
}

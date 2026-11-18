using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class HomeLayoutTests
    {
        const string HomeScene = "Assets/_Project/Scenes/Home.unity";
        const float MinimumGap = 5f;
        const float MinimumProfileSize = 200f;
        const float LowestTopBarCentre = -160f;

        static Rect RectIn(RectTransform space, RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = space.InverseTransformPoint(corners[0]);
            Vector2 max = space.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        static Rect VisibleIn(RectTransform space, Image image)
        {
            Rect rect = RectIn(space, image.rectTransform);
            if (image.sprite == null)
                return rect;
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(image.sprite);
            float scale = rect.width / image.sprite.rect.width;
            return Rect.MinMaxRect(rect.xMin + padding.x * scale, rect.yMin + padding.y * scale,
                rect.xMax - padding.z * scale, rect.yMax - padding.w * scale);
        }

        static Rect WithChildren(RectTransform space, RectTransform target)
        {
            Rect area = RectIn(space, target);
            foreach (Image image in target.GetComponentsInChildren<Image>())
            {
                Rect visible = VisibleIn(space, image);
                area = Rect.MinMaxRect(Mathf.Min(area.xMin, visible.xMin), area.yMin, Mathf.Max(area.xMax, visible.xMax), area.yMax);
            }

            return area;
        }

        [Test]
        public void TopBar_ShouldSitHighWithABiggerProfileAndRoomBeforeSettings_WhenLaidOut()
        {
            Scene scene = SceneManager.GetSceneByPath(HomeScene);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(HomeScene, OpenSceneMode.Additive);
            Canvas.ForceUpdateCanvases();
            RectTransform bar = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                bar = bar != null ? bar : root.transform.Find("SafeArea/TopBar") as RectTransform;
            var profile = (RectTransform)bar.Find("ProfileButton");
            Rect lives = WithChildren(bar, (RectTransform)bar.Find("Lives"));
            Rect coins = WithChildren(bar, (RectTransform)bar.Find("Coins"));
            Rect settings = VisibleIn(bar, bar.Find("SettingsButton").GetComponent<Image>());

            Assert.That(profile.sizeDelta.x, Is.GreaterThanOrEqualTo(MinimumProfileSize));
            foreach (string part in new[] { "ProfileButton", "Lives", "Coins", "SettingsButton" })
                Assert.That(((RectTransform)bar.Find(part)).anchoredPosition.y, Is.GreaterThanOrEqualTo(LowestTopBarCentre), part);
            Assert.That(settings.xMin - coins.xMax, Is.GreaterThanOrEqualTo(MinimumGap), "Coins to settings");
            Assert.That(coins.xMin - lives.xMax, Is.GreaterThanOrEqualTo(MinimumGap), "Lives to coins");
            if (openedHere)
                EditorSceneManager.CloseScene(scene, true);
        }

        [TestCase("Background", "HomeBackground")]
        [TestCase("SafeArea/TopBar", "HomeTopBar")]
        [TestCase("SafeArea/LevelPath", "LevelPath")]
        [TestCase("SafeArea/TabBar", "TabBar")]
        public void HomeCanvasParts_ShouldBePrefabInstances_InTheHomeScene(string path, string prefab)
        {
            Scene scene = SceneManager.GetSceneByPath(HomeScene);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(HomeScene, OpenSceneMode.Additive);
            Transform part = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                part = part != null ? part : root.transform.Find(path);

            Assert.That(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(part.gameObject),
                Is.EqualTo($"Assets/_Project/Prefabs/UI/Home/{prefab}.prefab"));
            if (openedHere)
                EditorSceneManager.CloseScene(scene, true);
        }
    }
}

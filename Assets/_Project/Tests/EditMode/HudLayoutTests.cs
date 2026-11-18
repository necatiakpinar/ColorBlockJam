using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class HudLayoutTests
    {
        const string GameplayScene = "Assets/_Project/Scenes/Gameplay.unity";
        const string HomeScene = "Assets/_Project/Scenes/Home.unity";
        const string HudPrefabs = "Assets/_Project/Prefabs/UI/Hud/";
        const string BoosterPrefabs = HudPrefabs + "Boosters/";
        const string PlusPrefab = "Assets/_Project/Prefabs/UI/Common/PlusButton.prefab";
        const float PlusFaceCentre = 0.595f;
        const float BarSpriteHeight = 325f;
        const float BarTopMargin = 31f;
        const float BarBottomMargin = 30f;
        const float EdgeTolerance = 20f;
        const float MinimumPlusToButton = 1.2f;

        static Scene Open(string path, out bool openedHere)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            openedHere = !scene.isLoaded;
            return openedHere ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : scene;
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

            Assert.Fail("The scene has no SafeArea.");
            return null;
        }

        static Rect RectIn(RectTransform space, RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = space.InverseTransformPoint(corners[0]);
            Vector2 max = space.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        [Test]
        public void TopBarButtons_ShouldSpanTheLevelBar_WhenStacked()
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            Canvas.ForceUpdateCanvases();
            var bar = (RectTransform)SafeArea(scene).Find("TopBar");
            var panel = (RectTransform)bar.Find("TimePanel");
            Rect panelRect = RectIn(bar, panel);
            float scale = panelRect.height / BarSpriteHeight;

            Rect restart = RectIn(bar, (RectTransform)bar.Find("SideButtons/RestartButton"));
            Rect pause = RectIn(bar, (RectTransform)bar.Find("SideButtons/PauseButton"));

            Assert.That(restart.yMax, Is.EqualTo(panelRect.yMax - BarTopMargin * scale).Within(EdgeTolerance));
            Assert.That(pause.yMin, Is.EqualTo(panelRect.yMin + BarBottomMargin * scale).Within(EdgeTolerance));
            Close(scene, openedHere);
        }

        [Test]
        public void TimePanel_ShouldDrawTheLevelAndTimeBarSliced_WhenLaidOut()
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            var panel = SafeArea(scene).Find("TopBar/TimePanel").GetComponent<Image>();

            Assert.That(panel.sprite.name, Is.EqualTo("Btn_levelBg_normal"));
            Assert.That(panel.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(panel.sprite.border.x, Is.GreaterThan(324f), "The circle stays inside the left slice.");
            Assert.That(panel.pixelsPerUnitMultiplier, Is.EqualTo(panel.sprite.rect.height / panel.rectTransform.sizeDelta.y).Within(0.01f));
            Assert.That(panel.transform.Find("LevelBadge").GetComponent<Image>(), Is.Null, "The bar's circle is the badge.");
            Close(scene, openedHere);
        }

        [Test]
        public void TimeArea_ShouldCentreItsSlotUnderItsTitle_WhenLaidOut()
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            Canvas.ForceUpdateCanvases();
            var area = (RectTransform)SafeArea(scene).Find("TopBar/TimePanel/TimeArea");
            Rect title = RectIn(area, (RectTransform)area.Find("TimeTitle"));
            Rect icon = RectIn(area, (RectTransform)area.Find("TimeIcon"));
            Rect slot = RectIn(area, (RectTransform)area.Find("TimeSlot"));

            Assert.That(slot.center.x, Is.EqualTo(area.rect.center.x).Within(2f));
            var text = area.Find("TimeTitle").GetComponent<TMPro.TMP_Text>();
            float textWidth = text.GetPreferredValues(text.text, 1000f, 200f).x;
            Assert.That((icon.xMin + title.center.x + textWidth / 2f) / 2f, Is.EqualTo(area.rect.center.x).Within(EdgeTolerance));
            Assert.That(title.center.y, Is.GreaterThan(slot.center.y));
            Close(scene, openedHere);
        }

        [Test]
        public void CoinIcon_ShouldSitInsideItsHolder_WhenOnTheTopBar()
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            Canvas.ForceUpdateCanvases();
            var holder = (RectTransform)SafeArea(scene).Find("TopBar/CoinCounter");
            Rect coin = RectIn(holder, (RectTransform)holder.Find("CoinIcon"));

            Assert.That(coin.xMin, Is.GreaterThanOrEqualTo(holder.rect.xMin - 1f));
            Assert.That(coin.height, Is.LessThanOrEqualTo(holder.rect.height));
            Close(scene, openedHere);
        }

        [TestCase("TopBar/TimePanel/TimeArea/TimeTitle")]
        [TestCase("TopBar/TimePanel/TimeArea/TimeSlot")]
        [TestCase("TopBar/TimePanel/LevelBadge")]
        [TestCase("TopBar/SideButtons/RestartButton")]
        [TestCase("TopBar/SideButtons/PauseButton/Icon")]
        [TestCase("TopBar/CoinCounter/CoinIcon")]
        [TestCase("BoosterBar/FreezeBooster/BuyBadge")]
        public void HudParts_ShouldBeAnchoredToTheirArea_InsteadOfAFixedPoint(string path)
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            var part = (RectTransform)SafeArea(scene).Find(path);

            Assert.That(part.anchorMin, Is.Not.EqualTo(part.anchorMax), path);
            Assert.That(part.sizeDelta.magnitude, Is.LessThan(0.01f), path);
            Close(scene, openedHere);
        }

        [TestCase(0, "Freeze")]
        [TestCase(1, "Rocket")]
        [TestCase(2, "Hammer")]
        [TestCase(3, "Vacuum")]
        public void BoosterSlots_ShouldBeVariantsOfOneBaseButton_WhenPlacedOnTheBar(int slot, string booster)
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            var bar = (RectTransform)SafeArea(scene).Find("BoosterBar");
            var button = (RectTransform)bar.GetChild(slot);
            string variantPath = BoosterPrefabs + booster + "BoosterButton.prefab";
            var variant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);

            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(button.gameObject), Is.EqualTo(variantPath));
            Assert.That(PrefabUtility.GetPrefabAssetType(variant), Is.EqualTo(PrefabAssetType.Variant));
            Assert.That(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(variant)),
                Is.EqualTo(BoosterPrefabs + "BoosterButton.prefab"));
            Assert.That(button.anchorMin.x, Is.EqualTo((slot + 0.5f) / bar.childCount).Within(0.001f));
            foreach (PropertyModification change in PrefabUtility.GetPropertyModifications(button.gameObject))
                Assert.That(PrefabUtility.IsDefaultOverride(change), Is.True, $"{booster} overrides {change.propertyPath}; set it in its variant.");
            Close(scene, openedHere);
        }

        [Test]
        public void BoosterBaseButton_ShouldCarryNoBoosterIcon_WhenEveryVariantSetsItsOwn()
        {
            var baseButton = AssetDatabase.LoadAssetAtPath<GameObject>(BoosterPrefabs + "BoosterButton.prefab");

            Assert.That(baseButton.transform.Find("Icon").GetComponent<Image>().sprite, Is.Null);
        }

        [TestCase("TopBar")]
        [TestCase("BoosterBar")]
        public void HudBars_ShouldBePrefabInstances_InTheGameplayScene(string name)
        {
            Scene scene = Open(GameplayScene, out bool openedHere);
            GameObject bar = SafeArea(scene).Find(name).gameObject;

            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(bar), Is.EqualTo(HudPrefabs + name + ".prefab"));
            Close(scene, openedHere);
        }

        [TestCase(GameplayScene, 5)]
        [TestCase(HomeScene, 2)]
        public void PlusIcons_ShouldComeFromOnePrefabAndFillTheFaceOfTheirButton_WhenShown(string scenePath, int expected)
        {
            Scene scene = Open(scenePath, out bool openedHere);
            Canvas.ForceUpdateCanvases();
            int checkedIcons = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite == null || image.sprite.name != "ic_plus")
                    continue;
                var button = (RectTransform)image.transform.parent;
                Assert.That(image.rectTransform.rect.width,
                    Is.GreaterThanOrEqualTo(button.rect.width * MinimumPlusToButton), button.parent.name);
                GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(button.gameObject);
                Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(PlusPrefab), button.parent.name);
                float centre = (image.rectTransform.anchorMin.y + image.rectTransform.anchorMax.y) / 2f;
                Assert.That(centre, Is.EqualTo(PlusFaceCentre).Within(0.001f), $"{button.parent.name}: the cross sits on the button's face");
                checkedIcons++;
            }

            Assert.That(checkedIcons, Is.EqualTo(expected));
            Close(scene, openedHere);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class PopupPrefabTests
    {
        const string GameplayConfigPath = "Assets/_Project/Configs/GameplayPopupConfig.asset";
        const string HomeConfigPath = "Assets/_Project/Configs/HomePopupConfig.asset";
        const string SettingsBasePath = "Assets/_Project/Prefabs/UI/Popups/SettingsPopup.prefab";
        const float PopupFaceBottom = 95f;
        const float MinimumTouchHeight = 120f;

        static GameplayPopupConfig GameplayConfig => AssetDatabase.LoadAssetAtPath<GameplayPopupConfig>(GameplayConfigPath);
        static HomePopupConfig HomeConfig => AssetDatabase.LoadAssetAtPath<HomePopupConfig>(HomeConfigPath);

        static IEnumerable<PopupView> Popups()
        {
            yield return GameplayConfig.PausePopup;
            yield return GameplayConfig.WinPopup;
            yield return GameplayConfig.LevelFailedPopup;
            yield return HomeConfig.SettingsPopup;
        }

        [Test]
        public void SettingToggles_ShouldTakeTapsAtLeastAFingerTall_InBothScenes()
        {
            foreach (SettingsPopupView popup in new SettingsPopupView[] { GameplayConfig.PausePopup, HomeConfig.SettingsPopup })
                foreach (string toggle in new[] { "VibrationToggle", "SoundToggle", "MusicToggle" })
                {
                    Image image = FindDeep(popup.transform, toggle).GetComponent<Image>();
                    Vector4 padding = image.raycastPadding;
                    float height = image.rectTransform.rect.height - padding.y - padding.w;
                    Assert.That(image.raycastTarget, Is.True, $"{popup.name}/{toggle}");
                    Assert.That(height, Is.GreaterThanOrEqualTo(MinimumTouchHeight), $"{popup.name}/{toggle}");
                }
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name)
                    return child;
            throw new AssertionException($"{root.name} has no {name}.");
        }

        [Test]
        public void SettingsPopups_ShouldBeVariantsOfTheSettingsBasePrefab_InBothScenes()
        {
            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsBasePath);
            foreach (SettingsPopupView popup in new SettingsPopupView[] { GameplayConfig.PausePopup, HomeConfig.SettingsPopup })
            {
                Assert.That(PrefabUtility.GetPrefabAssetType(popup.gameObject), Is.EqualTo(PrefabAssetType.Variant), popup.name);
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(popup.gameObject), Is.SameAs(basePrefab), popup.name);
            }
        }

        [Test]
        public void Config_ShouldReferenceEveryPopupPrefab_WhenLoaded()
        {
            foreach (PopupView popup in Popups())
            {
                Assert.That(popup, Is.Not.Null);
                Assert.That(PrefabUtility.IsPartOfPrefabAsset(popup), Is.True, popup.name);
            }
        }

        [Test]
        public void Popups_ShouldStartHiddenAndFillTheScreen_WhenCreated()
        {
            foreach (PopupView popup in Popups())
            {
                var root = (RectTransform)popup.transform;
                Assert.That(popup.gameObject.activeSelf, Is.False, popup.name);
                Assert.That(root.anchorMin, Is.EqualTo(Vector2.zero), popup.name);
                Assert.That(root.anchorMax, Is.EqualTo(Vector2.one), popup.name);
            }
        }

        [Test]
        public void PopupButtons_ShouldGivePressFeedback_WhenTapped()
        {
            foreach (PopupView popup in Popups())
                foreach (Button button in popup.GetComponentsInChildren<Button>(true))
                {
                    var feedback = button.GetComponent<ButtonPressFeedback>();
                    Assert.That(feedback, Is.Not.Null, $"{popup.name}/{button.name}");
                    var serialized = new SerializedObject(feedback);
                    Assert.That(serialized.FindProperty("_config").objectReferenceValue, Is.Not.Null, button.name);
                    Assert.That(serialized.FindProperty("_button").objectReferenceValue, Is.SameAs(button), button.name);
                }
        }

        [Test]
        public void PopupTexts_ShouldShrinkToFitTheirBox_WhenLaidOut()
        {
            foreach (PopupView popup in Popups())
                foreach (TMP_Text text in popup.GetComponentsInChildren<TMP_Text>(true))
                {
                    Assert.That(text.enableAutoSizing, Is.True, $"{popup.name}/{text.name}");
                    Assert.That(text.fontSizeMin, Is.LessThan(text.fontSizeMax), $"{popup.name}/{text.name}");
                }
        }

        [Test]
        public void SettingsButtons_ShouldBeChubbyWithRoomAroundTheirLabel_InBothScenes()
        {
            foreach (RectTransform button in SettingsButtons())
            {
                var label = (RectTransform)button.Find("Label");
                string where = $"{button.root.name}/{button.name}";

                Assert.That(button.sizeDelta.y, Is.GreaterThanOrEqualTo(160f), where);
                Assert.That(button.sizeDelta.x / button.sizeDelta.y, Is.InRange(2.4f, 2.8f), where);
                Assert.That(label.GetComponent<TMP_Text>().fontSizeMax, Is.LessThanOrEqualTo(54f), where);
                Assert.That(GroupWidth(button), Is.LessThanOrEqualTo(button.sizeDelta.x * 0.8f), where);
            }
        }

        [Test]
        public void SettingsButtonLabels_ShouldSitCentred_WithOrWithoutAnIcon()
        {
            foreach (RectTransform button in SettingsButtons())
            {
                var label = (RectTransform)button.Find("Label");
                var icon = button.Find("Icon") as RectTransform;
                string where = $"{button.root.name}/{button.name}";
                Assert.That(label.GetComponent<TMP_Text>().alignment, Is.EqualTo(TextAlignmentOptions.Center), where);

                if (icon == null)
                {
                    Assert.That(label.anchoredPosition.x, Is.EqualTo(0f), where);
                    Assert.That(label.anchorMin.x + label.anchorMax.x, Is.EqualTo(1f), where);
                    continue;
                }

                float left = icon.anchoredPosition.x - icon.sizeDelta.x / 2f;
                float right = label.anchoredPosition.x + TextWidth(label) / 2f;
                Assert.That(left + right, Is.EqualTo(0f).Within(8f), where);
            }
        }

        static IEnumerable<RectTransform> SettingsButtons()
        {
            string[] names = { "LegalTermsButton", "RestorePurchasesButton", "SupportButton", "LanguageButton" };
            foreach (PopupView popup in new PopupView[] { GameplayConfig.PausePopup, HomeConfig.SettingsPopup })
                foreach (string name in names)
                    yield return (RectTransform)popup.transform.Find("Panel/Content/" + name);
            yield return (RectTransform)GameplayConfig.PausePopup.transform.Find("Panel/Content/HomeButton");
        }

        static float TextWidth(RectTransform label)
        {
            var text = label.GetComponent<TMP_Text>();
            float size = text.fontSize;
            text.fontSize = text.fontSizeMax;
            float width = text.GetPreferredValues(text.text, 2000f, 200f).x;
            text.fontSize = size;
            return width;
        }

        static float GroupWidth(RectTransform button)
        {
            var icon = button.Find("Icon") as RectTransform;
            float text = TextWidth((RectTransform)button.Find("Label"));
            return icon == null ? text : icon.sizeDelta.x + text;
        }

        [TestCase("Btn_popup_blue", 70f, 82f)]
        [TestCase("Btn_popup_green", 105f, 122f)]
        public void PopupButtonSprites_ShouldKeepTheirHighlightInsideTheCornerSlice_WhenNineSliced(string sprite, float highlightRight, float highlightBottom)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath($"Assets/_Project/Art/UI/{sprite}.png");

            Assert.That(importer.spriteBorder.x, Is.GreaterThan(highlightRight), "Left border");
            Assert.That(importer.spriteBorder.w, Is.GreaterThan(highlightBottom), "Top border");
        }

        [Test]
        public void LevelFailedButtons_ShouldSitCentredUnderTheHeader_WhenTheRestOfThePanelIsEmpty()
        {
            var panel = (RectTransform)GameplayConfig.LevelFailedPopup.transform.Find("Panel");
            var header = (RectTransform)panel.Find("Header");
            var retry = (RectTransform)panel.Find("RetryButton");
            var home = (RectTransform)panel.Find("HomeButton");
            float freeTop = header.anchoredPosition.y - header.sizeDelta.y / 2f;
            float freeBottom = -panel.sizeDelta.y / 2f + PopupFaceBottom;
            float groupTop = retry.anchoredPosition.y + retry.sizeDelta.y / 2f;
            float groupBottom = home.anchoredPosition.y - home.sizeDelta.y / 2f;

            Assert.That((groupTop + groupBottom) / 2f, Is.EqualTo((freeTop + freeBottom) / 2f).Within(10f));
        }
    }
}

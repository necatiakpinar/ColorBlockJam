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
    }
}

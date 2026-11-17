using System.Collections.Generic;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    /// <summary>
    /// Builds views with their serialized references wired, as the authored scenes would, so flows can
    /// be tested without loading a scene. Destroy everything with <see cref="Dispose"/>.
    /// </summary>
    public sealed class TestViews : System.IDisposable
    {
        readonly List<Object> _created = new();

        public HomeView CreateHomeView(out Button levelButton) => CreateHomeView(out levelButton, out _);

        /// <summary>The Home view with a three-tile level path.</summary>
        public HomeView CreateHomeView(out Button levelButton, out Button settingsButton)
        {
            var root = Track(new GameObject("HomeView"));
            levelButton = Child(root, "LevelButton").AddComponent<Button>();
            settingsButton = Child(root, "SettingsButton").AddComponent<Button>();

            var view = root.AddComponent<HomeView>();
            Assign(view, "_levelButton", levelButton);
            Assign(view, "_levelLabel", Label(root, "LevelLabel"));
            Assign(view, "_settingsButton", settingsButton);
            Assign(view, "_coins", CreateCoinText(root));
            var serialized = new SerializedObject(view);
            SerializedProperty path = serialized.FindProperty("_pathLabels");
            path.arraySize = 3;
            for (int i = 0; i < 3; i++)
                path.GetArrayElementAtIndex(i).objectReferenceValue = Label(root, "PathLabel" + i);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            view.Construct();
            return view;
        }

        /// <summary>The Home settings popup: the shared settings with no Home button.</summary>
        public SettingsPopupView CreateSettingsPopup(SignalBus signalBus, ISettingsService settings,
            out Button closeButton, out SettingToggleView[] toggles)
        {
            var root = Track(new GameObject("SettingsPopup"));
            var view = root.AddComponent<SettingsPopupView>();
            closeButton = WireSettings(view, root, out toggles);
            view.Construct(signalBus, settings);
            return view;
        }

        /// <summary>The HUD with its taps exposed: restart and pause.</summary>
        public GameplayHudView CreateHud(SignalBus signalBus, IWalletService wallet, out Button restartButton,
            out Button pauseButton)
        {
            var root = Track(new GameObject("Hud"));
            restartButton = Child(root, "RestartButton").AddComponent<Button>();
            pauseButton = Child(root, "PauseButton").AddComponent<Button>();

            var view = root.AddComponent<GameplayHudView>();
            Assign(view, "_group", root.AddComponent<CanvasGroup>());
            Assign(view, "_panel", Child(root, "Panel").AddComponent<RectTransform>());
            Assign(view, "_levelLabel", Label(root, "LevelLabel"));
            Assign(view, "_timerLabel", Label(root, "TimerLabel"));
            Assign(view, "_coins", CreateCoinText(root));
            Assign(view, "_restartButton", restartButton);
            Assign(view, "_pauseButton", pauseButton);
            view.Construct(signalBus, Track(ScriptableObject.CreateInstance<GameplayConfig>()), wallet);
            return view;
        }

        /// <summary>The pause popup with one switch per setting, in <see cref="GameSetting"/> order.</summary>
        public PausePopupView CreatePausePopup(SignalBus signalBus, ISettingsService settings, out Button closeButton,
            out Button homeButton, out SettingToggleView[] toggles)
        {
            var root = Track(new GameObject("PausePopup"));
            var view = root.AddComponent<PausePopupView>();
            closeButton = WireSettings(view, root, out toggles);
            homeButton = Child(root, "HomeButton").AddComponent<Button>();
            Assign(view, "_homeButton", homeButton);
            view.Construct(signalBus, settings);
            view.ConstructHome();
            return view;
        }

        /// <summary>Wires the shared settings parts: one switch per setting, in <see cref="GameSetting"/> order.</summary>
        Button WireSettings(SettingsPopupView view, GameObject root, out SettingToggleView[] toggles)
        {
            var closeButton = Child(root, "CloseButton").AddComponent<Button>();
            GameSetting[] settingsShown = { GameSetting.Vibration, GameSetting.Sound, GameSetting.Music };
            toggles = new SettingToggleView[settingsShown.Length];
            var serialized = new SerializedObject(view);
            SerializedProperty list = serialized.FindProperty("_toggles");
            list.arraySize = toggles.Length;
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i] = CreateToggle(root, settingsShown[i]);
                list.GetArrayElementAtIndex(i).objectReferenceValue = toggles[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            WirePopup(view, root);
            Assign(view, "_closeButton", closeButton);
            return closeButton;
        }

        CoinAmountText CreateCoinText(GameObject parent)
        {
            var text = Child(parent, "Coins").AddComponent<CoinAmountText>();
            Assign(text, "_text", Label(text.gameObject, "Label"));
            return text;
        }

        public WinPopupView CreateWinPopup(out Button continueButton)
        {
            var root = Track(new GameObject("WinPopup"));
            var view = root.AddComponent<WinPopupView>();
            continueButton = Child(root, "ContinueButton").AddComponent<Button>();
            WirePopup(view, root);
            Assign(view, "_rewardLabel", Label(root, "RewardLabel"));
            Assign(view, "_continueButton", continueButton);
            view.Construct();
            return view;
        }

        public OutOfTimePopupView CreateOutOfTimePopup(out Button continueButton, out Button closeButton)
        {
            var root = Track(new GameObject("OutOfTimePopup"));
            var view = root.AddComponent<OutOfTimePopupView>();
            continueButton = Child(root, "ContinueButton").AddComponent<Button>();
            closeButton = Child(root, "CloseButton").AddComponent<Button>();
            WirePopup(view, root);
            Assign(view, "_secondsLabel", Label(root, "SecondsLabel"));
            Assign(view, "_messageLabel", Label(root, "MessageLabel"));
            Assign(view, "_costLabel", Label(root, "CostLabel"));
            Assign(view, "_continueButton", continueButton);
            Assign(view, "_closeButton", closeButton);
            view.Construct();
            return view;
        }

        public LevelFailedPopupView CreateLevelFailedPopup(out Button retryButton, out Button closeButton)
        {
            var root = Track(new GameObject("LevelFailedPopup"));
            var view = root.AddComponent<LevelFailedPopupView>();
            retryButton = Child(root, "RetryButton").AddComponent<Button>();
            closeButton = Child(root, "CloseButton").AddComponent<Button>();
            WirePopup(view, root);
            Assign(view, "_retryButton", retryButton);
            Assign(view, "_closeButton", closeButton);
            view.Construct();
            return view;
        }

        public void Dispose()
        {
            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        SettingToggleView CreateToggle(GameObject parent, GameSetting setting)
        {
            var root = Child(parent, setting + "Toggle");
            var toggle = root.AddComponent<SettingToggleView>();
            var serialized = new SerializedObject(toggle);
            serialized.FindProperty("_setting").intValue = (int)setting;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assign(toggle, "_button", root.AddComponent<Button>());
            Assign(toggle, "_onState", Child(root, "On"));
            Assign(toggle, "_offState", Child(root, "Off"));
            return toggle;
        }

        void WirePopup(PopupView view, GameObject root)
        {
            Assign(view, "_group", root.AddComponent<CanvasGroup>());
            Assign(view, "_panel", Child(root, "Panel").AddComponent<RectTransform>());
            view.ConstructPopup(Track(ScriptableObject.CreateInstance<UiConfig>()));
        }

        static TMP_Text Label(GameObject parent, string name) => Child(parent, name).AddComponent<TextMeshProUGUI>();

        T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        static GameObject Child(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child;
        }

        static void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

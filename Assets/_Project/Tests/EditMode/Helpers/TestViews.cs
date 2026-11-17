using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Home;
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

        public HomeView CreateHomeView(out Button levelButton)
        {
            var root = Track(new GameObject("HomeView"));
            levelButton = Child(root, "LevelButton").AddComponent<Button>();
            var label = Child(root, "LevelLabel").AddComponent<TextMeshProUGUI>();

            var view = root.AddComponent<HomeView>();
            Assign(view, "_levelButton", levelButton);
            Assign(view, "_levelLabel", label);
            view.Construct();
            return view;
        }

        public GameplayHudView CreateHud(SignalBus signalBus, out Button homeButton)
        {
            var root = Track(new GameObject("Hud"));
            var panel = Child(root, "Panel");
            homeButton = Child(root, "HomeButton").AddComponent<Button>();

            var view = root.AddComponent<GameplayHudView>();
            Assign(view, "_group", root.AddComponent<CanvasGroup>());
            Assign(view, "_panel", panel.AddComponent<RectTransform>());
            Assign(view, "_levelLabel", Child(root, "LevelLabel").AddComponent<TextMeshProUGUI>());
            Assign(view, "_timerLabel", Child(root, "TimerLabel").AddComponent<TextMeshProUGUI>());
            Assign(view, "_homeButton", homeButton);
            view.Construct(signalBus, Track(ScriptableObject.CreateInstance<GameplayConfig>()));
            return view;
        }

        public void Dispose()
        {
            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

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

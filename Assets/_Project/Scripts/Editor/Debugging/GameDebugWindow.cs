using System;
using System.Text;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Debugging;
using ColorBlockJam.Game.Scopes;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Editor.Debugging
{
    /// <summary>
    /// The game's own debug panel (debug-tools.md): win or lose the level now, set the timer, give coins, jump
    /// to a level, clear the saves. In play mode it drives <see cref="GameplayDebugCommands"/> from the
    /// Gameplay scope, so every shortcut goes through the real services; it only reads and repaints the state.
    /// </summary>
    public sealed class GameDebugWindow : EditorWindow
    {
        const string MenuPath = "ColorBlockJam/Debug Window %#d";
        static readonly string[] SaveKeys =
            { PlayerProgressService.StorageKey, WalletService.StorageKey, Core.Persistence.SettingsService.StorageKey };

        readonly StringBuilder _log = new();
        int _coins = 100;
        float _seconds = 5f;
        int _level = 1;
        Vector2 _scroll;
        Vector2 _logScroll;

        [MenuItem(MenuPath)]
        static void Open() => GetWindow<GameDebugWindow>("CBJ Debug").Show();

        // The game changes on its own while it plays: repaint a few times a second to show it.
        void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying)
                Repaint();
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (!EditorApplication.isPlaying)
                DrawEditMode();
            else if (TryGetCommands(out GameplayDebugCommands commands))
                DrawGameplay(commands);
            else
                EditorGUILayout.HelpBox("Open a level (Gameplay scene) to use the level shortcuts.", MessageType.Info);
            DrawLog();
            EditorGUILayout.EndScrollView();
        }

        void DrawEditMode()
        {
            EditorGUILayout.HelpBox("Enter play mode to drive the game from here.", MessageType.Info);
            if (GUILayout.Button("Play", GUILayout.Height(30)))
                EditorApplication.isPlaying = true;

            Header("Saves");
            if (GUILayout.Button("Clear player saves (progress, coins, settings)"))
                ClearSaves();
        }

        void DrawGameplay(GameplayDebugCommands commands)
        {
            Header("Level");
            EditorGUILayout.LabelField("Level", commands.LevelNumber.ToString());
            EditorGUILayout.LabelField("State", commands.State.ToString());
            EditorGUILayout.LabelField("Time left", $"{commands.SecondsLeft:0.0} s");
            EditorGUILayout.LabelField("Coins", commands.Coins.ToString());

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Win now", GUILayout.Height(30)))
                Run("Win", commands.Win(), "the level is not playing");
            if (GUILayout.Button("Run out of time", GUILayout.Height(30)))
                Run("Run out of time", commands.RunOutOfTime(), "the level is not playing");
            if (GUILayout.Button("Restart", GUILayout.Height(30)))
                Run("Restart", commands.Restart(), "a won level moves on instead");
            EditorGUILayout.EndHorizontal();

            Header("Timer");
            EditorGUILayout.BeginHorizontal();
            _seconds = EditorGUILayout.FloatField("Seconds left", _seconds);
            if (GUILayout.Button("Set", GUILayout.Width(60)))
                Do($"Timer set to {_seconds:0.#} s", () => commands.SetSecondsLeft(_seconds));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            foreach (int quick in new[] { 3, 10, 30 })
                if (GUILayout.Button($"{quick} s left"))
                    Do($"Timer set to {quick} s", () => commands.SetSecondsLeft(quick));
            if (GUILayout.Button("+30 s"))
                Do("Timer +30 s", () => commands.SetSecondsLeft(commands.SecondsLeft + 30f));
            EditorGUILayout.EndHorizontal();

            Header("Coins");
            EditorGUILayout.BeginHorizontal();
            _coins = Mathf.Max(1, EditorGUILayout.IntField("Amount", _coins));
            if (GUILayout.Button("Add", GUILayout.Width(60)))
                Do($"+{_coins} coins", () => commands.AddCoins(_coins));
            if (GUILayout.Button("Spend", GUILayout.Width(60)))
                Run($"Spend {_coins} coins", commands.SpendCoins(_coins), "not enough coins");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            foreach (int quick in new[] { 100, 1000, 10000 })
                if (GUILayout.Button($"+{quick}"))
                    Do($"+{quick} coins", () => commands.AddCoins(quick));
            EditorGUILayout.EndHorizontal();

            Header("Levels");
            EditorGUILayout.BeginHorizontal();
            _level = Mathf.Max(1, EditorGUILayout.IntField("Level", _level));
            if (GUILayout.Button("Play", GUILayout.Width(60)))
                Do($"Play level {_level}", () => commands.PlayLevel(_level));
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Next level"))
                Do($"Play level {commands.LevelNumber + 1}", () => commands.PlayLevel(commands.LevelNumber + 1));
        }

        void DrawLog()
        {
            Header("Log");
            _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.Height(120));
            EditorGUILayout.TextArea(_log.ToString(), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Clear log"))
                _log.Clear();
        }

        static bool TryGetCommands(out GameplayDebugCommands commands)
        {
            commands = null;
            LifetimeScope scope = LifetimeScope.Find<GameplayLifetimeScope>();
            if (scope == null || scope.Container == null)
                return false;
            commands = scope.Container.Resolve<GameplayDebugCommands>();
            return true;
        }

        void ClearSaves()
        {
            foreach (string key in SaveKeys)
                PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Write("Player saves cleared");
        }

        void Do(string action, Action command)
        {
            command();
            Write(action);
        }

        void Run(string action, bool done, string whyNot) => Write(done ? action : $"{action}: refused, {whyNot}");

        void Write(string line) => _log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {line}\n");

        static void Header(string title)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }
    }
}

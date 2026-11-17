using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ColorBlockJam.Core.Logging;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Scopes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class ArchitectureTests
    {
        const string ScriptsFolder = "Assets/_Project/Scripts";
        const string TestsFolder = "Assets/_Project/Tests";
        const string CoreAsmdef = ScriptsFolder + "/Core/ColorBlockJam.Core.asmdef";
        const string GameAssembly = "ColorBlockJam.Game";
        static readonly string[] PerFrameMethods = { "Update", "LateUpdate", "FixedUpdate" };
        static readonly Type[] PerFrameInterfaces = { typeof(ITickable), typeof(ILateTickable), typeof(IFixedTickable) };

        static readonly Dictionary<string, string> PerFrameAllowed = new();

        static readonly string[] DebugLogAllowed = { "Core/Logging/UnityGameLogger.cs" };

        static IEnumerable<Type> ProjectTypes()
        {
            foreach (Assembly assembly in new[] { typeof(SignalBus).Assembly, typeof(RootLifetimeScope).Assembly })
                foreach (Type type in assembly.GetTypes())
                    yield return type;
        }

        static IEnumerable<string> ScriptFiles() => Directory.GetFiles(ScriptsFolder, "*.cs", SearchOption.AllDirectories);

        static string Relative(string path) => path.Replace('\\', '/').Substring(ScriptsFolder.Length + 1);

        static List<string> SourceHits(Regex pattern, ICollection<string> allowedFiles)
        {
            var hits = new List<string>();
            foreach (string file in ScriptFiles())
            {
                string relative = Relative(file);
                if (allowedFiles.Contains(relative))
                    continue;
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (pattern.IsMatch(lines[i]))
                        hits.Add($"{relative}:{i + 1}");
                }
            }

            return hits;
        }

        static int CommentAt(string source)
        {
            for (int i = 0; i < source.Length - 1; i++)
            {
                char c = source[i];
                if (c == '/' && (source[i + 1] == '/' || source[i + 1] == '*'))
                    return i;
                if (c == '"')
                    i = StringEnd(source, i, i > 0 && source[i - 1] == '@');
                else if (c == '\'')
                    i = CharEnd(source, i);
            }

            return -1;
        }

        static int StringEnd(string source, int open, bool verbatim)
        {
            for (int i = open + 1; i < source.Length; i++)
            {
                if (!verbatim && source[i] == '\\')
                    i++;
                else if (source[i] == '"' && verbatim && i + 1 < source.Length && source[i + 1] == '"')
                    i++;
                else if (source[i] == '"')
                    return i;
            }

            return source.Length;
        }

        static int CharEnd(string source, int open)
        {
            int close = source.IndexOf('\'', open + (source[open + 1] == '\\' ? 3 : 2));
            return close < 0 ? source.Length : close;
        }

        [Test]
        public void Code_ShouldHaveNoComments_WhenWritten()
        {
            var found = new List<string>();
            foreach (string folder in new[] { ScriptsFolder, TestsFolder })
                foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
                {
                    string source = File.ReadAllText(file);
                    int at = CommentAt(source);
                    if (at >= 0)
                        found.Add($"{file.Replace('\\', '/')}:{source.Substring(0, at).Split('\n').Length}");
                }

            Assert.That(found, Is.Empty, "The code explains itself: no comments (CLAUDE.md, Comments).");
        }

        [Test]
        public void ProjectTypes_ShouldHaveNoPerFrameCode_WhenNotAllowListed()
        {
            const BindingFlags declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var found = new List<string>();
            foreach (Type type in ProjectTypes())
            {
                if (PerFrameAllowed.ContainsKey(type.FullName))
                    continue;
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                    foreach (string name in PerFrameMethods)
                        if (type.GetMethod(name, declared, null, Type.EmptyTypes, null) != null)
                            found.Add($"{type.FullName}.{name}");
                foreach (Type tick in PerFrameInterfaces)
                    if (tick.IsAssignableFrom(type) && !type.IsInterface)
                        found.Add($"{type.FullName} : {tick.Name}");
            }

            Assert.That(found, Is.Empty, "Per-frame code needs a reason in PerFrameAllowed (CLAUDE.md, Update loop).");
        }

        [Test]
        public void CoreAssembly_ShouldNotReferenceGame_WhenCompiledOrDeclared()
        {
            foreach (AssemblyName reference in typeof(SignalBus).Assembly.GetReferencedAssemblies())
                Assert.That(reference.Name, Does.Not.StartWith(GameAssembly), "Core uses a Game type.");

            string asmdef = File.ReadAllText(CoreAsmdef);
            Assert.That(asmdef, Does.Not.Contain($"\"{GameAssembly}"), "Core's assembly definition references Game.");
        }

        [Test]
        public void Scripts_ShouldLogThroughTheGameLogger_WhenTheyLog()
        {
            List<string> hits = SourceHits(new Regex(@"\bDebug\.Log"), DebugLogAllowed);

            Assert.That(hits, Is.Empty, "Use the injected IGameLogger, never Debug.Log (CLAUDE.md, Logging).");
        }

        [Test]
        public void Scripts_ShouldResolveThroughDependencyInjection_WhenTheyNeedAnObject()
        {
            var pattern = new Regex(@"\bCamera\.main\b|\bFind(First|Any)?Object(s)?(OfType|ByType)\b|\bGameObject\.Find(WithTag)?\(");

            List<string> hits = SourceHits(pattern, Array.Empty<string>());

            Assert.That(hits, Is.Empty, "No Camera.main or Find* lookups: resolve through DI (CLAUDE.md, Architecture).");
        }

        [Test]
        public void Scenes_ShouldHaveEverySerializedReferenceSet_WhenTheBuildScenesOpen()
        {
            var missing = new List<string>();
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                bool wasOpen = SceneManager.GetSceneByPath(buildScene.path).isLoaded;
                Scene scene = wasOpen ? SceneManager.GetSceneByPath(buildScene.path) : EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);
                try
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                        foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                            CollectMissing(component, scene.name, missing);
                }
                finally
                {
                    if (!wasOpen)
                        EditorSceneManager.CloseScene(scene, true);
                }
            }

            Assert.That(missing, Is.Empty, "A missing reference is a setup bug (CLAUDE.md, Safety).");
        }

        static void CollectMissing(MonoBehaviour component, string sceneName, List<string> missing)
        {
            if (component == null)
            {
                missing.Add($"{sceneName}: a component whose script is missing");
                return;
            }

            string space = component.GetType().Namespace;
            if (space == null || !space.StartsWith("ColorBlockJam"))
                return;
            SerializedProperty property = new SerializedObject(component).GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null &&
                    property.name != "m_Script")
                    missing.Add($"{sceneName}/{Path(component.transform)}: {component.GetType().Name}.{property.propertyPath}");
        }

        static string Path(Transform transform) =>
            transform.parent == null ? transform.name : $"{Path(transform.parent)}/{transform.name}";
    }
}

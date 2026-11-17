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
    /// <summary>
    /// The rules in CLAUDE.md that code review alone would let slip (T-37, testing.md): no per-frame code, Core never
    /// depends on Game, logs go through <see cref="IGameLogger"/>, no scene lookups, and every scene's serialized
    /// references are set.
    /// </summary>
    public sealed class ArchitectureTests
    {
        const string ScriptsFolder = "Assets/_Project/Scripts";
        const string CoreAsmdef = ScriptsFolder + "/Core/ColorBlockJam.Core.asmdef";
        const string GameAssembly = "ColorBlockJam.Game";
        static readonly string[] PerFrameMethods = { "Update", "LateUpdate", "FixedUpdate" };
        static readonly Type[] PerFrameInterfaces = { typeof(ITickable), typeof(ILateTickable), typeof(IFixedTickable) };

        // Per-frame code with the reason the event-driven way was unavailable (CLAUDE.md "Update loop"). Empty today.
        static readonly Dictionary<string, string> PerFrameAllowed = new();

        // The one place allowed to call UnityEngine.Debug.
        static readonly string[] DebugLogAllowed = { "Core/Logging/UnityGameLogger.cs" };

        static IEnumerable<Type> ProjectTypes()
        {
            foreach (Assembly assembly in new[] { typeof(SignalBus).Assembly, typeof(RootLifetimeScope).Assembly })
                foreach (Type type in assembly.GetTypes())
                    yield return type;
        }

        static IEnumerable<string> ScriptFiles() => Directory.GetFiles(ScriptsFolder, "*.cs", SearchOption.AllDirectories);

        static string Relative(string path) => path.Replace('\\', '/').Substring(ScriptsFolder.Length + 1);

        /// <summary>Source lines matching <paramref name="pattern"/>, comments left out, as "file:line".</summary>
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
                    string code = lines[i].TrimStart();
                    if (!code.StartsWith("//") && pattern.IsMatch(code))
                        hits.Add($"{relative}:{i + 1}");
                }
            }

            return hits;
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
                // A scene the developer already has open is read as it is and left open.
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

        /// <summary>Unset object references on one of the project's own components; Unity's and packages' are not ours to judge.</summary>
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

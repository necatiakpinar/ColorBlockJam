using System;
using System.Linq;
using NUnit.Framework;
using RollicCase.Core.Scenes;
using UnityEditor;

namespace RollicCase.Tests.EditMode
{
    public sealed class BuildSettingsTests
    {
        static string[] EnabledSceneNames() => EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToArray();

        [Test]
        public void BuildSettings_ShouldStartWithSplash_WhenAppLaunches()
        {
            var loader = new SceneLoader();

            Assert.That(EnabledSceneNames().FirstOrDefault(), Is.EqualTo(loader.GetSceneName(SceneId.Splash)));
        }

        [Test]
        public void BuildSettings_ShouldContainEverySceneId_WhenLoaderNamesIt()
        {
            var loader = new SceneLoader();
            string[] enabled = EnabledSceneNames();

            foreach (SceneId sceneId in Enum.GetValues(typeof(SceneId)))
                Assert.That(enabled, Does.Contain(loader.GetSceneName(sceneId)), sceneId.ToString());
        }

        [Test]
        public void PlayerSettings_ShouldLockPortrait_WhenBuilding()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.Portrait));
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.False);
        }

        [Test]
        public void PlayerSettings_ShouldBuildIl2CppArm64_WhenTargetingAndroid()
        {
            Assert.That(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android),
                Is.EqualTo(ScriptingImplementation.IL2CPP));
            Assert.That(PlayerSettings.Android.targetArchitectures, Is.EqualTo(AndroidArchitecture.ARM64));
        }
    }
}

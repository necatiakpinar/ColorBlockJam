using System;
using NUnit.Framework;
using ColorBlockJam.Game.Audio;
using ColorBlockJam.Game.Configs;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.U2D;
using UnityEngine.U2D;

namespace ColorBlockJam.Tests.EditMode
{
    public sealed class AddressablesSetupTests
    {
        const string AtlasPath = "Assets/_Project/Art/UI/UiAtlas.spriteatlas";
        const string AudioConfigPath = "Assets/_Project/Configs/AudioConfig.asset";
        const string LevelPackConfigPath = "Assets/_Project/Configs/LevelPackConfig.asset";
        const string UiConfigPath = "Assets/_Project/Configs/UiConfig.asset";

        static AddressableAssetSettings Settings => AddressableAssetSettingsDefaultObject.Settings;

        static AddressableAssetEntry EntryAt(string path) => Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));

        [Test]
        public void UiAtlas_ShouldBeAddressableByItsName_WhenLateBound()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            AddressableAssetEntry entry = EntryAt(AtlasPath);

            Assert.That(entry, Is.Not.Null, "The atlas is addressable.");
            Assert.That(entry.parentGroup.Name, Is.EqualTo("UI"));
            Assert.That(entry.address, Is.EqualTo(atlas.name), "SpriteAtlasBinder loads the atlas by the tag Unity asks for.");
            Assert.That(AssetDatabase.LoadAssetAtPath<UiConfig>(UiConfigPath).AtlasAddress, Is.EqualTo(entry.address), "The boot preloads it.");
            Assert.That(atlas.IsIncludeInBuild(), Is.False, "An atlas also built into the player would ship twice.");
        }

        [Test]
        public void EveryCueClip_ShouldBeAddressable_WhenTheGameShips()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(AudioConfigPath);

            foreach (AudioCue cue in Enum.GetValues(typeof(AudioCue)))
                Assert.That(Settings.FindAssetEntry(config.For(cue).Clip.AssetGUID)?.parentGroup.Name, Is.EqualTo("Audio"), cue.ToString());
        }

        [Test]
        public void BundledLevelPack_ShouldBeAddressable_WhenTheGameShips()
        {
            var config = AssetDatabase.LoadAssetAtPath<LevelPackConfig>(LevelPackConfigPath);

            Assert.That(Settings.FindAssetEntry(config.BundledPack.AssetGUID)?.parentGroup.Name, Is.EqualTo("Levels"));
        }

        [Test]
        public void EveryContentGroup_ShouldListGuidsInItsCatalog_WhenAssetReferencesLoadByGuid()
        {
            foreach (AddressableAssetGroup group in Settings.groups)
            {
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema != null)
                    Assert.That(schema.IncludeGUIDInCatalog, Is.True, group.Name);
            }
        }

        [Test]
        public void Settings_ShouldBuildContent_WhenThePlayerBuilds()
        {
            Assert.That(Settings.BuildAddressablesWithPlayerBuild,
                Is.EqualTo(AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer));
        }
    }
}

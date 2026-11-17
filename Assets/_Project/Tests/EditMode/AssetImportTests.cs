using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>Guards the import rules in art-pipeline.md so a reimport or a new file cannot drift.</summary>
    public sealed class AssetImportTests
    {
        const string UiFolder = "Assets/_Project/Art/UI";
        const string ModelsFolder = "Assets/_Project/Art/Models";
        const float CellImportScale = 0.5f;

        [Test]
        public void UiTextures_ShouldImportAsSpritesWithoutMipmaps_WhenInUiFolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UiFolder });
            Assert.That(guids, Is.Not.Empty);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);

                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.mipmapEnabled, Is.False, path);
            }
        }

        [Test]
        public void Models_ShouldImportAtCellScaleWithoutMaterials_WhenInModelsFolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { ModelsFolder });
            Assert.That(guids, Is.Not.Empty);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);

                Assert.That(importer.globalScale, Is.EqualTo(CellImportScale), path);
                Assert.That(importer.materialImportMode, Is.EqualTo(ModelImporterMaterialImportMode.None), path);
                Assert.That(importer.importAnimation, Is.False, path);
                Assert.That(importer.importTangents, Is.EqualTo(ModelImporterTangents.None), $"{path}: no material uses a normal map.");
                Assert.That(importer.isReadable, Is.False, $"{path}: a readable mesh keeps a CPU copy.");
            }
        }

        [Test]
        public void AtlasSprites_ShouldStayUncompressed_WhenTheAtlasCompressesThePage()
        {
            foreach (Object packable in SpriteAtlasExtensions.GetPackables(Atlas()))
            {
                string path = AssetDatabase.GetAssetPath(packable);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);

                // A compressed source is compressed twice when packed, and loses detail (art-pipeline.md §2).
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), path);
                Assert.That(importer.GetPlatformTextureSettings("Android").overridden, Is.False, path);
            }
        }

        [Test]
        public void Atlas_ShouldCompressToAstcOnAndroid_WhenBuilt()
        {
            TextureImporterPlatformSettings android = SpriteAtlasExtensions.GetPlatformSettings(Atlas(), "Android");

            Assert.That(android.overridden, Is.True);
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
        }

        [Test]
        public void Atlas_ShouldFitOnOnePage_WhenPackedForAndroid()
        {
            SpriteAtlas atlas = Atlas();
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, BuildTarget.Android);
            var previews = (Texture2D[])typeof(SpriteAtlasExtensions)
                .GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { atlas });

            // A second page splits the UI's draw calls and doubles its memory (art-pipeline.md §2).
            Assert.That(previews, Has.Length.EqualTo(1));
        }

        static SpriteAtlas Atlas() => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(UiFolder + "/UiAtlas.spriteatlas");

        [Test]
        public void FloorTile_ShouldSpanOneCell_WhenImported()
        {
            var tile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Board/Floor_Tile.prefab");
            Bounds bounds = tile.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;

            Assert.That(bounds.size.x, Is.EqualTo(1f).Within(0.05f));
            Assert.That(bounds.size.z, Is.EqualTo(1f).Within(0.05f));
        }
    }
}

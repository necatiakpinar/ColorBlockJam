using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RollicCase.Tests.EditMode
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
            }
        }

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

using System.IO;
using ColorBlockJam.Game.Levels;
using UnityEditor;

namespace ColorBlockJam.Editor.LevelEditing
{
    /// <summary>
    /// Adds, copies, removes and reorders the catalog's level assets (T-26, level-editor.md §3). Every change goes
    /// straight to disk without an undo record: the editor's undo covers the draft, and an undo record on an asset
    /// lets the test runner's undo rollback revert a saved level. A new level is an asset next to the others, named
    /// after the first free number.
    /// </summary>
    public sealed class LevelCatalogEditing
    {
        const string ListField = "_levels";

        readonly LevelCatalog _catalog;
        readonly string _folder;

        public LevelCatalogEditing(LevelCatalog catalog, string folder)
        {
            _catalog = catalog;
            _folder = folder;
        }

        public LevelCatalog Catalog => _catalog;

        /// <summary>Creates a level asset holding <paramref name="data"/> and inserts it at <paramref name="index"/>.</summary>
        public LevelDefinition Insert(int index, LevelData data)
        {
            var definition = UnityEngine.ScriptableObject.CreateInstance<LevelDefinition>();
            definition.SetData(data);
            AssetDatabase.CreateAsset(definition, NextFreePath());

            var serialized = new SerializedObject(_catalog);
            SerializedProperty list = serialized.FindProperty(ListField);
            list.InsertArrayElementAtIndex(index);
            list.GetArrayElementAtIndex(index).objectReferenceValue = definition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return definition;
        }

        public LevelDefinition Append(LevelData data) => Insert(_catalog.Levels.Count, data);

        /// <summary>Copies the level and puts the copy right after it.</summary>
        public LevelDefinition Duplicate(int index) => Insert(index + 1, LevelDraft.From(_catalog.Levels[index].Data).ToLevel());

        /// <summary>Takes the level out of the catalog and deletes its asset.</summary>
        public void Delete(int index)
        {
            LevelDefinition definition = _catalog.Levels[index];
            var serialized = new SerializedObject(_catalog);
            SerializedProperty list = serialized.FindProperty(ListField);
            list.GetArrayElementAtIndex(index).objectReferenceValue = null;
            list.DeleteArrayElementAtIndex(index);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(definition));
            AssetDatabase.SaveAssets();
        }

        /// <summary>Moves a level to another place in the order; level numbers follow the order.</summary>
        public void Move(int from, int to)
        {
            var serialized = new SerializedObject(_catalog);
            serialized.FindProperty(ListField).MoveArrayElement(from, to);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Writes the draft into the level's asset and saves it.</summary>
        public void Save(int index, LevelDraft draft)
        {
            LevelDefinition definition = _catalog.Levels[index];
            definition.SetData(draft.ToLevel());
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }

        string NextFreePath()
        {
            for (int number = 1; ; number++)
            {
                string path = $"{_folder}/Level_{number:000}.asset";
                if (!File.Exists(path))
                    return path;
            }
        }
    }
}

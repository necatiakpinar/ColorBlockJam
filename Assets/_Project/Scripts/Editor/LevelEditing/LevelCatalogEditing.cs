using System.Collections.Generic;
using System.IO;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Packs;
using UnityEditor;

namespace ColorBlockJam.Editor.LevelEditing
{
    public sealed class LevelCatalogEditing
    {
        const string ListField = "_levels";
        public const string PackFile = "levels.json";

        readonly LevelCatalog _catalog;
        readonly string _folder;

        public LevelCatalogEditing(LevelCatalog catalog, string folder)
        {
            _catalog = catalog;
            _folder = folder;
        }

        public LevelCatalog Catalog => _catalog;

        public string PackPath => $"{_folder}/{PackFile}";

        public void WritePack()
        {
            var json = new LevelPackJson();
            var levels = new List<LevelData>(_catalog.Levels.Count);
            foreach (LevelDefinition definition in _catalog.Levels)
                levels.Add(definition.Data);
            int revision = 1;
            if (File.Exists(PackPath) && json.TryRead(File.ReadAllText(PackPath), out LevelPack previous, out _))
            {
                bool same = json.Write(new LevelPack(previous.Revision, levels)) == json.Write(previous);
                revision = same ? previous.Revision : previous.Revision + 1;
            }

            File.WriteAllText(PackPath, json.Write(new LevelPack(revision, levels)));
            AssetDatabase.ImportAsset(PackPath);
        }

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
            WritePack();
            return definition;
        }

        public LevelDefinition Append(LevelData data) => Insert(_catalog.Levels.Count, data);

        public LevelDefinition Duplicate(int index) => Insert(index + 1, LevelDraft.From(_catalog.Levels[index].Data).ToLevel());

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
            WritePack();
        }

        public void Move(int from, int to)
        {
            var serialized = new SerializedObject(_catalog);
            serialized.FindProperty(ListField).MoveArrayElement(from, to);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            WritePack();
        }

        public void Save(int index, LevelDraft draft)
        {
            LevelDefinition definition = _catalog.Levels[index];
            definition.SetData(draft.ToLevel());
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            WritePack();
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

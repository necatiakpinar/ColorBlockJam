using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class TestCatalog : IDisposable
    {
        readonly List<Object> _created = new();

        public LevelCatalog Create(params LevelData[] levels)
        {
            var catalog = Track(ScriptableObject.CreateInstance<LevelCatalog>());
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                var definition = Track(ScriptableObject.CreateInstance<LevelDefinition>());
                definition.SetData(levels[i]);
                list.GetArrayElementAtIndex(i).objectReferenceValue = definition;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
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
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Editor.LevelEditing;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEditor;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The level editor's catalog actions on a throw-away catalog in a temporary folder (T-26).</summary>
    public sealed class LevelCatalogEditingTests
    {
        const string Folder = "Assets/__LevelEditorTests";

        LevelCatalog _catalog;
        LevelCatalogEditing _editing;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.CreateFolder("Assets", "__LevelEditorTests");
            _catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            AssetDatabase.CreateAsset(_catalog, Folder + "/Catalog.asset");
            _editing = new LevelCatalogEditing(_catalog, Folder);
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(Folder);

        static LevelData Level(int width) => new LevelDataBuilder(width, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build();

        List<int> Widths()
        {
            var widths = new List<int>();
            foreach (LevelDefinition level in _catalog.Levels)
                widths.Add(level.Data.Width);
            return widths;
        }

        [Test]
        public void Append_ShouldCreateAnAssetAtTheEnd_WhenANewLevelIsAdded()
        {
            _editing.Append(Level(3));
            LevelDefinition added = _editing.Append(Level(4));

            Assert.That(Widths(), Is.EqualTo(new[] { 3, 4 }));
            Assert.That(AssetDatabase.GetAssetPath(added), Is.EqualTo(Folder + "/Level_002.asset"));
        }

        [Test]
        public void Duplicate_ShouldPutACopyRightAfter_WhenALevelIsCopied()
        {
            _editing.Append(Level(3));
            _editing.Append(Level(5));

            LevelDefinition copy = _editing.Duplicate(0);

            Assert.That(Widths(), Is.EqualTo(new[] { 3, 3, 5 }));
            Assert.That(copy, Is.Not.SameAs(_catalog.Levels[0]));
        }

        [Test]
        public void Move_ShouldChangeThePlayOrder_WhenALevelIsMoved()
        {
            _editing.Append(Level(3));
            _editing.Append(Level(4));
            _editing.Append(Level(5));

            _editing.Move(2, 0);

            Assert.That(Widths(), Is.EqualTo(new[] { 5, 3, 4 }));
        }

        [Test]
        public void Delete_ShouldRemoveTheLevelAndItsAsset_WhenDeleted()
        {
            _editing.Append(Level(3));
            LevelDefinition doomed = _editing.Append(Level(4));
            string path = AssetDatabase.GetAssetPath(doomed);

            _editing.Delete(1);

            Assert.That(Widths(), Is.EqualTo(new[] { 3 }));
            Assert.That(AssetDatabase.LoadAssetAtPath<LevelDefinition>(path), Is.Null);
        }

        [Test]
        public void Save_ShouldWriteTheDraftIntoTheAsset_WhenSaved()
        {
            _editing.Append(Level(3));
            LevelDraft draft = LevelDraft.From(_catalog.Levels[0].Data);
            draft.TimerSeconds = 123;

            _editing.Save(0, draft);

            Assert.That(_catalog.Levels[0].Data.TimerSeconds, Is.EqualTo(123));
        }

        [Test]
        public void Save_ShouldSurviveAnUndoRollback_WhenTheTestRunnerRevertsTheEditor()
        {
            _editing.Append(Level(3));
            LevelDraft draft = LevelDraft.From(_catalog.Levels[0].Data);
            draft.TimerSeconds = 123;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            _editing.Save(0, draft);
            _editing.Append(Level(4));
            Undo.RevertAllDownToGroup(group);

            Assert.That(_catalog.Levels[0].Data.TimerSeconds, Is.EqualTo(123));
            Assert.That(Widths(), Is.EqualTo(new[] { 3, 4 }));
        }
    }
}

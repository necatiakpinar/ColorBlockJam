using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Tests.EditMode.Helpers;
using UnityEngine;
using static ColorBlockJam.Game.Levels.BlockColor;
using static ColorBlockJam.Game.Levels.WallSide;

namespace ColorBlockJam.Tests.EditMode
{
    /// <summary>The level pack JSON: readable, round-trips every field, and words what it refuses (level-packs.md §2).</summary>
    public sealed class LevelPackJsonTests
    {
        readonly LevelPackJson _json = new();

        static LevelData Level() => new LevelDataBuilder(5, 6)
            .Blocked(Red, new BlockerSet(new IceBlockerData(2), new ArrowBlockerData(ArrowAxis.Horizontal)), 1, 1, 2, 1)
            .Block(Blue, 3, 4).Door(Left, 1, 2, Red).Door(Top, 3, 1, Blue).Timer(90).Build();

        static LevelPack Pack() => new(7, new[] { Level() });

        static string Minimal(string block) =>
            "{\"format\":1,\"revision\":1,\"levels\":[{\"width\":4,\"height\":4,\"timer\":60,\"difficulty\":\"Normal\"," +
            "\"doors\":[{\"side\":\"Left\",\"start\":0,\"length\":1,\"color\":\"Red\"}],\"blocks\":[" + block + "]}]}";

        [Test]
        public void Read_ShouldGiveBackEveryField_WhenAWrittenPackIsRead()
        {
            Assert.That(_json.TryRead(_json.Write(Pack()), out LevelPack read, out string error), Is.True, error);

            Assert.That(read.Revision, Is.EqualTo(7));
            Assert.That(JsonUtility.ToJson(read.Levels[0]), Is.EqualTo(JsonUtility.ToJson(Level())));
        }

        [Test]
        public void Write_ShouldUseNamesNotNumbers_WhenAPackIsWritten()
        {
            string json = _json.Write(Pack());

            Assert.That(json, Does.Contain("\"color\": \"Red\""));
            Assert.That(json, Does.Contain("\"arrow\": \"Horizontal\""));
            Assert.That(json, Does.Contain("\"side\": \"Left\""));
            Assert.That(json, Does.Not.Contain("_width"));
        }

        [Test]
        public void Write_ShouldLeaveOutSpaces_WhenACompactPackIsAskedFor()
        {
            Assert.That(_json.Write(Pack(), readable: false), Does.Not.Contain("\n"));
        }

        [Test]
        public void Read_ShouldTakeNoArrow_WhenABlockLeavesItOut()
        {
            Assert.That(_json.TryRead(Minimal("{\"color\":\"Red\",\"cells\":[{\"x\":0,\"y\":0}]}"), out LevelPack read, out string error), Is.True, error);

            Assert.That(read.Levels[0].Blocks[0].Blockers.IsEmpty, Is.True);
        }

        [Test]
        public void Read_ShouldIgnoreFieldsItDoesNotKnow_WhenANewerPackAddsSome()
        {
            string json = Minimal("{\"color\":\"Red\",\"cells\":[{\"x\":0,\"y\":0}],\"glow\":true}");

            Assert.That(_json.TryRead(json, out _, out string error), Is.True, error);
        }

        [Test]
        public void Read_ShouldNameTheLevelAndBlock_WhenAColourIsUnknown()
        {
            Assert.That(_json.TryRead(Minimal("{\"color\":\"Teal\",\"cells\":[]}"), out _, out string error), Is.False);

            Assert.That(error, Does.Contain("Level 1").And.Contain("block 1").And.Contain("Teal"));
        }

        [Test]
        public void Read_ShouldRefuseANumber_WhenANameIsExpected()
        {
            Assert.That(_json.TryRead(Minimal("{\"color\":\"1000\",\"cells\":[]}"), out _, out _), Is.False);
        }

        [Test]
        public void Read_ShouldRefuseAMisspeltArrow_WhenOneIsGiven()
        {
            Assert.That(_json.TryRead(Minimal("{\"color\":\"Red\",\"cells\":[],\"arrow\":\"Sideways\"}"), out _, out string error), Is.False);

            Assert.That(error, Does.Contain("Sideways"));
        }

        [Test]
        public void Read_ShouldRefuse_WhenTheFormatIsNotThisBuilds()
        {
            string json = Minimal("{\"color\":\"Red\",\"cells\":[]}").Replace("\"format\":1", "\"format\":2");

            Assert.That(_json.TryRead(json, out _, out string error), Is.False);
            Assert.That(error, Does.Contain("format 2"));
        }

        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{\"format\":1,\"revision\":1,\"levels\":[]}")]
        public void Read_ShouldRefuse_WhenThereAreNoLevelsToRead(string json)
        {
            Assert.That(_json.TryRead(json, out LevelPack pack, out string error), Is.False);
            Assert.That(pack, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void Read_ShouldKeepTheOrder_WhenAPackHasManyLevels()
        {
            var levels = new List<LevelData>();
            for (int width = 3; width <= 8; width++)
                levels.Add(new LevelDataBuilder(width, 4).Block(Red, 0, 0).Door(Left, 0, 1, Red).Build());

            _json.TryRead(_json.Write(new LevelPack(1, levels)), out LevelPack read, out _);

            for (int i = 0; i < levels.Count; i++)
                Assert.That(read.Levels[i].Width, Is.EqualTo(levels[i].Width));
        }
    }
}

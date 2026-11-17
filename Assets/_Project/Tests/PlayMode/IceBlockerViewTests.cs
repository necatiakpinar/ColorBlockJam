using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>A frozen block on screen (T-34): drawn in ice with its count, counting down and thawing.</summary>
    public sealed class IceBlockerViewTests
    {
        string _progress;

        [SetUp]
        public void SetUp() =>
            _progress = PlayerPrefs.HasKey(PlayerProgressService.StorageKey) ? PlayerPrefs.GetString(PlayerProgressService.StorageKey) : null;

        [TearDown]
        public void TearDown()
        {
            if (_progress == null)
                PlayerPrefs.DeleteKey(PlayerProgressService.StorageKey);
            else
                PlayerPrefs.SetString(PlayerProgressService.StorageKey, _progress);
            PlayerPrefs.Save();
        }

        // Red and blue next to their doors; yellow frozen until both have left.
        static LevelData FrozenLevel() => new(4, 3, 60,
            new[]
            {
                new BlockData(BlockColor.Red, new[] { new Vector2Int(0, 2) }),
                new BlockData(BlockColor.Blue, new[] { new Vector2Int(3, 2) }),
                new BlockData(BlockColor.Yellow, new[] { new Vector2Int(1, 0), new Vector2Int(2, 0) }, 2),
            },
            new[]
            {
                new DoorData(WallSide.Left, 2, 1, BlockColor.Red), new DoorData(WallSide.Right, 2, 1, BlockColor.Blue),
                new DoorData(WallSide.Bottom, 1, 2, BlockColor.Yellow),
            },
            LevelDifficulty.Normal);

        [UnityTest]
        public IEnumerator FrozenBlock_ShouldShowItsIceCountDownAndThaw_WhenOthersLeave()
        {
            yield return SceneTestUtility.BootToGameplay();
            IObjectResolver container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            var board = container.Resolve<ActiveBoard>();
            board.Load(FrozenLevel());
            BlockView yellow = SceneTestUtility.FindInActiveScene<BlockLayerView>().Views[2];
            bool shownAtStart = yellow.ShowsIce;
            bool redShowsIce = SceneTestUtility.FindInActiveScene<BlockLayerView>().Views[0].ShowsIce;
            string countAtStart = yellow.IceText;

            board.TryStep(0, Vector2Int.left);
            string countAfterOne = yellow.IceText;
            board.TryStep(1, Vector2Int.right);
            yield return null;

            Assert.That(shownAtStart, Is.True);
            Assert.That(countAtStart, Is.EqualTo("2"));
            Assert.That(countAfterOne, Is.EqualTo("1"));
            Assert.That(yellow.ShowsIce, Is.False);
            Assert.That(redShowsIce, Is.False, "A block without ice shows no count.");
            LogAssert.NoUnexpectedReceived();
        }
    }
}

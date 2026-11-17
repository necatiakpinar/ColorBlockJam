using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>Winning moves on to the next level, and after the last one the catalog starts over.</summary>
    public sealed class LevelProgressionFlowTests
    {
        string _savedProgress;
        bool _hadProgress;

        [SetUp]
        public void SetUp()
        {
            // These tests win levels, which saves the player's progress: put it back afterwards.
            _hadProgress = PlayerPrefs.HasKey(PlayerProgressService.StorageKey);
            _savedProgress = PlayerPrefs.GetString(PlayerProgressService.StorageKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadProgress)
                PlayerPrefs.SetString(PlayerProgressService.StorageKey, _savedProgress);
            else
                PlayerPrefs.DeleteKey(PlayerProgressService.StorageKey);
            PlayerPrefs.Save();
        }

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Wins_ShouldPlayEachNextLevelAndLoopAfterTheLast_WhenLevelsAreWonInARow()
        {
            yield return SceneTestUtility.BootToGameplay();
            IObjectResolver container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            var bus = container.Resolve<SignalBus>();
            var session = container.Resolve<LevelSession>();
            var progress = container.Resolve<IPlayerProgressService>();
            var board = container.Resolve<ActiveBoard>();
            var levels = container.Resolve<LevelProvider>();
            float delay = container.Resolve<GameplayConfig>().NextLevelDelay;
            int catalogSize = container.Resolve<LevelCatalog>().Levels.Count;
            int first = session.LevelNumber;
            var started = new List<int>();
            bus.Subscribe<LevelStartedSignal>(signal => started.Add(signal.LevelNumber));

            // A full lap of the catalog: the last win wraps around to the first layout.
            for (int win = 0; win < catalogSize; win++)
            {
                bus.Fire(new BoardClearedSignal());
                Assert.That(session.State, Is.EqualTo(LevelState.Won));
                Assert.That(progress.CurrentLevel, Is.EqualTo(first + win + 1), "Progress moves on at the win.");
                yield return WaitRealtime(delay + 0.2f);
            }

            var expected = new List<int>();
            for (int n = 1; n <= catalogSize; n++)
                expected.Add(first + n);
            Assert.That(started, Is.EqualTo(expected));
            Assert.That(session.State, Is.EqualTo(LevelState.Playing));
            LevelData sameLayout = levels.GetLevel(first);
            Assert.That(board.Current.Width, Is.EqualTo(sameLayout.Width), "Back on the layout it started from.");
            Assert.That(board.Current.Height, Is.EqualTo(sameLayout.Height));
            Assert.That(board.Current.RemainingBlocks, Is.EqualTo(sameLayout.Blocks.Count));
        }
    }
}

using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>Winning shows the win popup; Continue flies the coins to the counter and goes Home, which plays the next level (T-51).</summary>
    public sealed class LevelProgressionFlowTests
    {
        static readonly string[] SavedKeys = { PlayerProgressService.StorageKey, WalletService.StorageKey };
        readonly string[] _saved = new string[SavedKeys.Length];

        [SetUp]
        public void SetUp()
        {
            // These tests win levels, which saves the player's progress and coins: put them back afterwards.
            for (int i = 0; i < SavedKeys.Length; i++)
                _saved[i] = PlayerPrefs.HasKey(SavedKeys[i]) ? PlayerPrefs.GetString(SavedKeys[i]) : null;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < SavedKeys.Length; i++)
            {
                if (_saved[i] == null)
                    PlayerPrefs.DeleteKey(SavedKeys[i]);
                else
                    PlayerPrefs.SetString(SavedKeys[i], _saved[i]);
            }

            PlayerPrefs.Save();
        }

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Win_ShouldFlyTheCoinsAndPlayTheNextLevelFromHome_WhenContinueIsTapped()
        {
            yield return SceneTestUtility.BootToGameplay();
            IObjectResolver container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            var progress = container.Resolve<IPlayerProgressService>();
            var hud = SceneTestUtility.FindInActiveScene<GameplayHudView>();
            int first = container.Resolve<LevelSession>().LevelNumber;
            string coinsBefore = hud.CoinLabelText;

            container.Resolve<SignalBus>().Fire(new BoardClearedSignal());
            string coinsAtWin = hud.CoinLabelText;
            yield return WaitRealtime(container.Resolve<GameplayConfig>().WinPopupDelay + 0.2f);
            Assert.That(SceneTestUtility.FindInActiveScene<WinPopupView>().gameObject.activeSelf, Is.True);
            SceneTestUtility.TapButton<WinPopupView>("ContinueButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);
            string homeLevel = SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText;
            SceneTestUtility.TapButton<HomeView>("LevelButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.GameplayScene);

            Assert.That(progress.CurrentLevel, Is.EqualTo(first + 1), "Progress moves on at the win.");
            Assert.That(coinsAtWin, Is.EqualTo(coinsBefore), "The counter waits for the coins to fly in.");
            Assert.That(homeLevel, Does.Contain((first + 1).ToString()), "Home offers the next level.");
            Assert.That(SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container.Resolve<LevelSession>().LevelNumber,
                Is.EqualTo(first + 1));
        }
    }
}

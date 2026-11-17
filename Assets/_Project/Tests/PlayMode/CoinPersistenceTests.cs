using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Economy;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>A win pays its reward once, and the balance survives leaving and coming back (R-19).</summary>
    public sealed class CoinPersistenceTests
    {
        static readonly string[] SavedKeys = { WalletService.StorageKey, PlayerProgressService.StorageKey };
        readonly string[] _saved = new string[SavedKeys.Length];

        [SetUp]
        public void SetUp()
        {
            // These tests earn coins and win a level: put the player's saves back afterwards.
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

        /// <summary>Home is reached through the pause popup.</summary>
        static void GoHome()
        {
            SceneTestUtility.TapButton<GameplayHudView>("PauseButton");
            SceneTestUtility.TapButton<PausePopupView>("HomeButton");
        }

        static IObjectResolver GameplayContainer() =>
            SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;

        [UnityTest]
        public IEnumerator Coins_ShouldRiseByTheRewardOnceAndSurviveAVisitHome_WhenALevelIsWon()
        {
            yield return SceneTestUtility.BootToGameplay();
            IObjectResolver container = GameplayContainer();
            var wallet = container.Resolve<IWalletService>();
            int level = container.Resolve<LevelSession>().LevelNumber;
            int reward = container.Resolve<LevelRewardProvider>().CoinsFor(container.Resolve<LevelProvider>().GetLevel(level));
            int before = wallet.Coins;

            container.Resolve<SignalBus>().Fire(new BoardClearedSignal());
            container.Resolve<SignalBus>().Fire(new BoardClearedSignal());
            int afterWin = wallet.Coins;
            yield return WaitRealtime(container.Resolve<GameplayConfig>().WinPopupDelay + 0.2f);
            // Continue flies the coins and goes Home (T-51).
            SceneTestUtility.TapButton<WinPopupView>("ContinueButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);
            SceneTestUtility.TapButton<HomeView>("LevelButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.GameplayScene);

            Assert.That(afterWin, Is.EqualTo(before + reward), "Paid once, though the clear was announced twice.");
            Assert.That(GameplayContainer().Resolve<IWalletService>().Coins, Is.EqualTo(afterWin));
        }

        [UnityTest]
        public IEnumerator Coins_ShouldStayTheSame_WhenALevelIsLeftUnfinished()
        {
            yield return SceneTestUtility.BootToGameplay();
            int before = GameplayContainer().Resolve<IWalletService>().Coins;

            GoHome();
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);
            SceneTestUtility.TapButton<HomeView>("LevelButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.GameplayScene);

            Assert.That(GameplayContainer().Resolve<IWalletService>().Coins, Is.EqualTo(before));
        }
    }
}

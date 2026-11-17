using System.Collections;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Game.Boosters;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Boosters;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>
    /// The boosters through the real HUD, input and views (T-36, boosters.md): a tap on the bar, a tap on the board,
    /// and what the player sees.
    /// </summary>
    public sealed class BoosterFlowTests
    {
        static readonly string[] SavedKeys =
            { PlayerProgressService.StorageKey, WalletService.StorageKey, BoosterInventory.StorageKey };
        readonly string[] _saved = new string[SavedKeys.Length];

        [SetUp]
        public void SetUp()
        {
            // These tests spend boosters and coins: put the saves back afterwards.
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

        static IObjectResolver Gameplay() => SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        static void Own(BoosterType type)
        {
            var inventory = Gameplay().Resolve<BoosterInventory>();
            if (inventory.Count(type) == 0)
                inventory.Add(type, 1);
        }

        /// <summary>Presses and lifts a finger on a board cell through the real input view.</summary>
        static void TapCell(Vector2Int cell)
        {
            var input = SceneTestUtility.FindInActiveScene<BoardInputView>();
            var space = Gameplay().Resolve<BoardSpace>();
            var camera = Gameplay().Resolve<Camera>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = camera.WorldToScreenPoint(space.CellToWorld(cell)),
                pointerId = 0,
            };
            input.OnPointerDown(pointer);
            input.OnPointerUp(pointer);
        }

        [UnityTest]
        public IEnumerator Hammer_ShouldSmashTheTappedBlock_WhenUsedFromTheBar()
        {
            yield return SceneTestUtility.BootToGameplay();
            Own(BoosterType.Hammer);
            int owned = Gameplay().Resolve<BoosterInventory>().Count(BoosterType.Hammer);
            BoardBlock block = Gameplay().Resolve<ActiveBoard>().Current.Blocks[0];

            SceneTestUtility.TapButton<BoosterBarView>("HammerBooster");
            yield return WaitRealtime(0.3f);
            bool bannerShown = SceneTestUtility.FindInActiveScene<BoosterTargetingView>().IsShowing;
            LevelState whileChoosing = Gameplay().Resolve<LevelSession>().State;
            TapCell(block.GetCell(0));
            yield return WaitRealtime(Gameplay().Resolve<BoardViewConfig>().SmashDuration + 0.2f);

            Assert.That(bannerShown, Is.True);
            Assert.That(whileChoosing, Is.EqualTo(LevelState.Paused));
            Assert.That(block.HasExited, Is.True);
            Assert.That(SceneTestUtility.FindInActiveScene<BlockLayerView>().Views[0], Is.Null);
            Assert.That(Gameplay().Resolve<BoosterInventory>().Count(BoosterType.Hammer), Is.EqualTo(owned - 1));
            Assert.That(Gameplay().Resolve<LevelSession>().State, Is.Not.EqualTo(LevelState.Paused));
            Assert.That(SceneTestUtility.FindInActiveScene<BoosterTargetingView>().IsShowing, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Rocket_ShouldRebuildTheBlockWithoutTheBlastedCell_WhenItHasOthers()
        {
            yield return SceneTestUtility.BootToGameplay();
            Own(BoosterType.Rocket);
            BoardState board = Gameplay().Resolve<ActiveBoard>().Current;
            BoardBlock block = null;
            foreach (BoardBlock candidate in board.Blocks)
                if (candidate.CellCount > 1)
                    block = candidate;
            Assume.That(block, Is.Not.Null, "The level needs a block of more than one cell.");
            int cells = block.CellCount;
            BlockView view = SceneTestUtility.FindInActiveScene<BlockLayerView>().Views[block.Id];
            int parts = view.PartCount;

            SceneTestUtility.TapButton<BoosterBarView>("RocketBooster");
            TapCell(block.GetCell(0));
            yield return null;

            Assert.That(block.CellCount, Is.EqualTo(cells - 1));
            Assert.That(view.PartCount, Is.LessThan(parts));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Freeze_ShouldHoldTheTimerAndShowTheIce_WhenUsedFromTheBar()
        {
            yield return SceneTestUtility.BootToGameplay();
            Own(BoosterType.Freeze);
            var timer = Gameplay().Resolve<LevelTimer>();

            SceneTestUtility.TapButton<BoosterBarView>("FreezeBooster");
            float before = timer.SecondsLeft;
            yield return WaitRealtime(1f);

            Assert.That(timer.IsFrozen, Is.True);
            Assert.That(timer.SecondsLeft, Is.EqualTo(before));
            Assert.That(SceneTestUtility.FindInActiveScene<FreezeView>().IsShowing, Is.True);
            Assert.That(SceneTestUtility.FindInActiveScene<FreezeView>().MeterFill, Is.LessThan(1f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator EmptyBooster_ShouldOfferOneForCoinsAndPlayOn_WhenBought()
        {
            yield return SceneTestUtility.BootToGameplay();
            var inventory = Gameplay().Resolve<BoosterInventory>();
            var wallet = Gameplay().Resolve<IWalletService>();
            while (inventory.TryUse(BoosterType.Vacuum))
            {
            }

            int price = Gameplay().Resolve<BoosterConfig>().For(BoosterType.Vacuum).Price;
            wallet.Add(price);
            int coins = wallet.Coins;
            bool showsPlus = SceneTestUtility.FindInActiveScene<BoosterBarView>().ButtonFor(BoosterType.Vacuum).ShowsBuyBadge;

            SceneTestUtility.TapButton<BoosterBarView>("VacuumBooster");
            var popup = SceneTestUtility.FindInActiveScene<BoosterPurchasePopupView>();
            bool opened = popup.gameObject.activeSelf;
            LevelState whileOpen = Gameplay().Resolve<LevelSession>().State;
            SceneTestUtility.TapButton<BoosterPurchasePopupView>("BuyButton");
            yield return null;

            Assert.That(showsPlus, Is.True);
            Assert.That(opened, Is.True);
            Assert.That(popup.TitleText, Is.EqualTo("Vacuum"));
            Assert.That(whileOpen, Is.EqualTo(LevelState.Paused));
            Assert.That(popup.gameObject.activeSelf, Is.False);
            Assert.That(inventory.Count(BoosterType.Vacuum), Is.EqualTo(1));
            Assert.That(wallet.Coins, Is.EqualTo(coins - price));
            Assert.That(Gameplay().Resolve<LevelSession>().State, Is.EqualTo(LevelState.Playing));
            Assert.That(Gameplay().Resolve<BoosterService>().IsTargeting, Is.False, "Buying does not use it.");
            LogAssert.NoUnexpectedReceived();
        }
    }
}

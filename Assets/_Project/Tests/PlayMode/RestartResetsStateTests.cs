using System.Collections;
using System.Text;
using NUnit.Framework;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Hud;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Popups;
using ColorBlockJam.Game.Gameplay.Session;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Home;
using ColorBlockJam.Game.Lives;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>
    /// Nothing of an attempt survives a restart or a trip Home (T-22, R-21, R-25): the board, the block views,
    /// the timer, the coins and the scene's objects come back exactly as a fresh start leaves them.
    /// </summary>
    public sealed class RestartResetsStateTests
    {
        static readonly string[] SavedKeys =
            { PlayerProgressService.StorageKey, WalletService.StorageKey, LivesService.StorageKey };
        readonly string[] _saved = new string[SavedKeys.Length];

        [SetUp]
        public void SetUp()
        {
            // A loss and a win change the saves: put them back afterwards.
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

        /// <summary>Everything a restart must put back, as one comparable text.</summary>
        static string Snapshot()
        {
            IObjectResolver container = Gameplay();
            var text = new StringBuilder();
            BoardState board = container.Resolve<ActiveBoard>().Current;
            foreach (BoardBlock block in board.Blocks)
                text.Append($"block {block.Id} at {block.Offset} exited {block.HasExited}\n");
            foreach (BlockView view in SceneTestUtility.FindInActiveScene<BlockLayerView>().Views)
                text.Append($"view at {view.transform.localPosition} scale {view.transform.localScale}\n");
            text.Append($"timer {container.Resolve<LevelTimer>().SecondsLeft:0.00}\n");
            text.Append($"coins {container.Resolve<IWalletService>().Coins}\n");
            text.Append($"state {container.Resolve<LevelSession>().State}\n");
            int active = 0;
            foreach (Transform transform in Object.FindObjectsOfType<Transform>())
                active++;
            text.Append($"active objects {active}\n");
            return text.ToString();
        }

        /// <summary>
        /// Each restart of a started attempt costs a life (DA-38); ten in a row would run out, so the lives start
        /// full every round. The saved lives are put back in TearDown.
        /// </summary>
        static void FillLives()
        {
            PlayerPrefs.DeleteKey(LivesService.StorageKey);
            Gameplay().Resolve<LivesService>().Load();
        }

        /// <summary>Drags the first block one cell to the right with the real input, starting the timer.</summary>
        static void DragFirstBlock()
        {
            var input = SceneTestUtility.FindInActiveScene<BoardInputView>();
            var space = Gameplay().Resolve<BoardSpace>();
            var camera = Gameplay().Resolve<Camera>();
            Vector2Int cell = Gameplay().Resolve<ActiveBoard>().Current.Blocks[0].GetCell(0);
            PointerEventData At(Vector2Int c) => new(EventSystem.current)
            {
                position = camera.WorldToScreenPoint(space.CellToWorld(c)),
                pointerId = 0,
            };

            input.OnPointerDown(At(cell));
            input.OnDrag(At(cell + Vector2Int.right));
            input.OnPointerUp(At(cell + Vector2Int.right));
        }

        [UnityTest]
        public IEnumerator Restart_ShouldLeaveTheSameStateAsAFreshStart_WhenRepeatedTenTimes()
        {
            yield return SceneTestUtility.BootToGameplay();
            float settle = Gameplay().Resolve<BoardViewConfig>().SnapDuration + 0.3f;
            yield return WaitRealtime(settle);
            string fresh = Snapshot();

            for (int attempt = 0; attempt < 10; attempt++)
            {
                FillLives();
                DragFirstBlock();
                yield return WaitRealtime(0.4f);
                Assert.That(Snapshot(), Is.Not.EqualTo(fresh), "The attempt changed something to restart from.");
                SceneTestUtility.TapButton<GameplayHudView>("RestartButton");
                yield return WaitRealtime(settle);

                Assert.That(Snapshot(), Is.EqualTo(fresh), $"After restart {attempt + 1}");
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Home_ShouldGiveTheSameLevelAndAFreshAttempt_WhenLeftAfterALoss()
        {
            yield return SceneTestUtility.BootToGameplay();
            var session = Gameplay().Resolve<LevelSession>();
            int level = session.LevelNumber;
            int fullTime = Mathf.CeilToInt(Gameplay().Resolve<LevelTimer>().SecondsLeft);

            session.Fail();
            SceneTestUtility.TapButton<OutOfTimePopupView>("CloseButton");
            SceneTestUtility.TapButton<LevelFailedPopupView>("CloseButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);
            string homeLabel = SceneTestUtility.FindInActiveScene<HomeView>().LevelLabelText;
            SceneTestUtility.TapButton<HomeView>("LevelButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.GameplayScene);

            Assert.That(homeLabel, Is.EqualTo("Level " + level));
            Assert.That(Gameplay().Resolve<LevelSession>().LevelNumber, Is.EqualTo(level));
            Assert.That(Gameplay().Resolve<LevelSession>().State, Is.EqualTo(LevelState.Playing));
            Assert.That(Gameplay().Resolve<LevelTimer>().SecondsLeft, Is.EqualTo(fullTime));
            foreach (var popup in new Component[]
                     {
                         SceneTestUtility.FindInActiveScene<PausePopupView>(), SceneTestUtility.FindInActiveScene<WinPopupView>(),
                         SceneTestUtility.FindInActiveScene<OutOfTimePopupView>(), SceneTestUtility.FindInActiveScene<LevelFailedPopupView>(),
                     })
                Assert.That(popup.gameObject.activeSelf, Is.False, popup.name);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SceneUnload_ShouldLeaveNoGameplayListener_WhenGoingHome()
        {
            yield return SceneTestUtility.BootToGameplay();
            SceneTestUtility.TapButton<GameplayHudView>("PauseButton");
            SceneTestUtility.TapButton<PausePopupView>("HomeButton");
            yield return SceneTestUtility.WaitForScene(SceneTestUtility.HomeScene);
            IObjectResolver home = SceneTestUtility.FindInActiveScene<HomeLifetimeScope>().Container;
            var bus = home.Resolve<SignalBus>();
            var progress = home.Resolve<IPlayerProgressService>();
            var wallet = home.Resolve<IWalletService>();
            int level = progress.CurrentLevel;
            int coins = wallet.Coins;

            // A gameplay listener left behind would touch a destroyed view or change the saves here.
            Assert.DoesNotThrow(() =>
            {
                bus.Fire(new LevelStartedSignal(level));
                bus.Fire(new TimerChangedSignal(5));
                bus.Fire(new BlockGrabbedSignal(0));
                bus.Fire(new BoardClearedSignal());
                bus.Fire(new LevelCompletedSignal(level));
                bus.Fire(new LevelFailedSignal(level));
                bus.Fire(new CoinsChangedSignal(coins, 0));
                // A gameplay popup that was never opened must not keep listening either (lives.md).
                bus.Fire(new LivesChangedSignal(4, 5, 60));
            });
            yield return WaitRealtime(1.5f);

            Assert.That(progress.CurrentLevel, Is.EqualTo(level), "No progression listener survived.");
            Assert.That(wallet.Coins, Is.EqualTo(coins), "No reward listener survived.");
            Assert.That(SceneTestUtility.FindInActiveScene<HomeView>().gameObject.activeInHierarchy, Is.True);
            LogAssert.NoUnexpectedReceived();
        }
    }
}

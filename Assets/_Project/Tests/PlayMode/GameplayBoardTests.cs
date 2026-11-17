using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Input;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Scopes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
using VContainer;

namespace ColorBlockJam.Tests.PlayMode
{
    /// <summary>The real Gameplay scene draws the board and lets a finger take blocks out through their doors.</summary>
    public sealed class GameplayBoardTests
    {
        IObjectResolver _container;
        ActiveBoard _board;
        BoardSpace _space;
        Camera _camera;
        BoardInputView _input;
        BlockLayerView _blocks;

        IEnumerator BootToBoard()
        {
            yield return SceneTestUtility.BootToGameplay();
            _container = SceneTestUtility.FindInActiveScene<GameplayLifetimeScope>().Container;
            _board = _container.Resolve<ActiveBoard>();
            _space = _container.Resolve<BoardSpace>();
            _camera = _container.Resolve<Camera>();
            _input = SceneTestUtility.FindInActiveScene<BoardInputView>();
            _blocks = SceneTestUtility.FindInActiveScene<BlockLayerView>();
        }

        PointerEventData PointerAt(int x, int y) => new(EventSystem.current)
        {
            position = _camera.WorldToScreenPoint(_space.CellToWorld(new Vector2Int(x, y))),
            pointerId = 0,
        };

        void Swipe(Vector2Int from, Vector2Int to)
        {
            _input.OnPointerDown(PointerAt(from.x, from.y));
            _input.OnDrag(PointerAt(to.x, to.y));
            _input.OnPointerUp(PointerAt(to.x, to.y));
        }

        static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        static int ExpectedBoardParts(LevelData level)
        {
            int parts = 1 + level.Width * level.Height + 4; // base, tiles, corners
            foreach (WallSide side in new[] { WallSide.Bottom, WallSide.Top, WallSide.Left, WallSide.Right })
                for (int i = 0; i < level.WallLength(side); i++)
                    parts += IsDoorCell(level, side, i) ? 1 : 2;
            return parts;
        }

        static bool IsDoorCell(LevelData level, WallSide side, int index)
        {
            foreach (DoorData door in level.Doors)
                if (door.Side == side && index >= door.Start && index <= door.End)
                    return true;
            return false;
        }

        /// <summary>Two blocks next to their doors: red leaves left, blue leaves right.</summary>
        static LevelData TwoBlockLevel() => new(4, 3, 60,
            new[]
            {
                new BlockData(BlockColor.Red, new[] { new Vector2Int(0, 1) }),
                new BlockData(BlockColor.Blue, new[] { new Vector2Int(3, 1) }),
            },
            new[]
            {
                new DoorData(WallSide.Left, 1, 1, BlockColor.Red),
                new DoorData(WallSide.Right, 1, 1, BlockColor.Blue),
            });

        [UnityTest]
        public IEnumerator Board_ShouldDrawEveryPartAndBlock_WhenGameplayStarts()
        {
            yield return BootToBoard();
            LevelData level = _container.Resolve<LevelProvider>().GetLevel(1);
            _board.Load(level);

            Assert.That(SceneTestUtility.FindInActiveScene<BoardView>().BuiltPartCount, Is.EqualTo(ExpectedBoardParts(level)));
            Assert.That(_blocks.Views.Count, Is.EqualTo(level.Blocks.Count));
            foreach (BlockView view in _blocks.Views)
                Assert.That(view.PartCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator Swipe_ShouldTakeTheBlockOffTheBoardAndClearItOnce_WhenEveryBlockLeaves()
        {
            yield return BootToBoard();
            var bus = _container.Resolve<SignalBus>();
            var cleared = new List<BoardClearedSignal>();
            bus.Subscribe<BoardClearedSignal>(cleared.Add);
            try
            {
                _board.Load(TwoBlockLevel());
                BlockView redView = _blocks.Views[0];

                Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
                Assert.That(_board.Current.Blocks[0].HasExited, Is.True);
                Assert.That(_blocks.Views[0], Is.Null);
                Assert.That(cleared, Is.Empty);

                yield return WaitRealtime(_container.Resolve<BoardViewConfig>().ExitDuration + 0.2f);
                Assert.That(redView.gameObject.activeInHierarchy, Is.False, "The exited block view returns to its pool.");

                Swipe(new Vector2Int(3, 1), new Vector2Int(5, 1));

                Assert.That(_board.Current.IsCleared, Is.True);
                Assert.That(cleared.Count, Is.EqualTo(1));
            }
            finally
            {
                bus.Unsubscribe<BoardClearedSignal>(cleared.Add);
            }
        }

        [UnityTest]
        public IEnumerator Outline_ShouldShowWhileABlockIsHeld_WhenGrabbedAndHideOnRelease()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView blue = _blocks.Views[1];

            _input.OnPointerDown(PointerAt(3, 1));
            bool shownWhileHeld = blue.IsOutlined;
            _input.OnPointerUp(PointerAt(3, 1));

            Assert.That(shownWhileHeld, Is.True);
            Assert.That(blue.IsOutlined, Is.False);
            Assert.That(_blocks.Views[0].IsOutlined, Is.False, "Only the held block is outlined.");
        }

        [UnityTest]
        public IEnumerator Reload_ShouldRebuildCleanBlocks_WhenABlockIsStillSlidingOut()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView exiting = _blocks.Views[0];
            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));

            _board.Load(TwoBlockLevel());

            // The sliding view went back to its pool with the old board: it is either idle or reused, at full
            // size, for one of the new blocks.
            bool reused = false;
            foreach (BlockView view in _blocks.Views)
                reused |= view == exiting;
            Assert.That(reused || !exiting.gameObject.activeInHierarchy, Is.True);
            Assert.That(_blocks.Views.Count, Is.EqualTo(2));
            foreach (BlockView view in _blocks.Views)
            {
                Assert.That(view.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(view.PartCount, Is.GreaterThan(0));
            }

            Swipe(new Vector2Int(3, 1), new Vector2Int(2, 1));
            yield return WaitRealtime(_container.Resolve<BoardViewConfig>().SnapDuration + 0.2f);
            Assert.That(_blocks.Views[1].transform.localPosition.x, Is.EqualTo(2.5f).Within(0.01f),
                "A reused block view still slides to its new cell.");
        }

        [UnityTest]
        public IEnumerator BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView view = _blocks.Views[1];
            view.Follow(new Vector2(-0.4f, 0f));
            view.SnapTo(Vector2Int.zero);

            Assert.That(() =>
            {
                for (int i = 0; i < 200; i++)
                {
                    view.Follow(new Vector2(-0.4f, 0f));
                    view.SnapTo(Vector2Int.zero);
                }
            }, Is.Not.AllocatingGCMemory());
        }

        [UnityTest]
        public IEnumerator Swipe_ShouldLeaveTheBlockInPlace_WhenTheDoorIsTheWrongColour()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1)); // red out
            Swipe(new Vector2Int(3, 1), new Vector2Int(-2, 1)); // blue slides left to the red door

            Assert.That(_board.Current.Blocks[1].HasExited, Is.False);
            Assert.That(_board.Current.Blocks[1].GetCell(0), Is.EqualTo(new Vector2Int(0, 1)));
        }
    }
}

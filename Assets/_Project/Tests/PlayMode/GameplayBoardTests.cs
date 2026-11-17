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
            int parts = 1 + level.Width * level.Height + 4 + level.Doors.Count * 2;
            foreach (WallSide side in new[] { WallSide.Bottom, WallSide.Top, WallSide.Left, WallSide.Right })
            {
                bool inWall = false;
                for (int i = 0; i < level.WallLength(side); i++)
                {
                    bool wallCell = !IsDoorCell(level, side, i);
                    if (wallCell && !inWall)
                        parts++;
                    inWall = wallCell;
                }
            }

            return parts;
        }

        static bool IsDoorCell(LevelData level, WallSide side, int index)
        {
            foreach (DoorData door in level.Doors)
                if (door.Side == side && index >= door.Start && index <= door.End)
                    return true;
            return false;
        }

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
        public IEnumerator Board_ShouldCloseTheWallRingWithoutSeams_WhenWallsAreStretchedBetweenDoors()
        {
            yield return BootToBoard();
            _board.Load(new LevelData(6, 5, 60,
                new[] { new BlockData(BlockColor.Red, new[] { new Vector2Int(0, 0) }) },
                new[]
                {
                    new DoorData(WallSide.Top, 1, 3, BlockColor.Red),
                    new DoorData(WallSide.Left, 0, 1, BlockColor.Blue),
                    new DoorData(WallSide.Right, 2, 2, BlockColor.Green),
                }));

            var ring = new List<Bounds>();
            foreach (MeshRenderer part in SceneTestUtility.FindInActiveScene<BoardView>().GetComponentsInChildren<MeshRenderer>())
            {
                string name = part.transform.parent.name;
                if (name.StartsWith("Wall_") || name.StartsWith("Door_"))
                    ring.Add(part.bounds);
            }

            AssertSideIsClosed(ring, b => b.min.x, b => b.min.z, b => b.max.z, lowest: true);
            AssertSideIsClosed(ring, b => b.max.x, b => b.min.z, b => b.max.z, lowest: false);
            AssertSideIsClosed(ring, b => b.min.z, b => b.min.x, b => b.max.x, lowest: true);
            AssertSideIsClosed(ring, b => b.max.z, b => b.min.x, b => b.max.x, lowest: false);
        }

        [UnityTest]
        public IEnumerator Doors_ShouldShowOneCentredArrowPointingOutOfTheBoard_WhenTheBoardIsBuilt()
        {
            yield return BootToBoard();
            var level = new LevelData(5, 4, 60,
                new[] { new BlockData(BlockColor.Red, new[] { new Vector2Int(2, 2) }) },
                new[]
                {
                    new DoorData(WallSide.Bottom, 1, 2, BlockColor.Red),
                    new DoorData(WallSide.Top, 0, 3, BlockColor.Blue),
                    new DoorData(WallSide.Left, 2, 1, BlockColor.Green),
                    new DoorData(WallSide.Right, 1, 2, BlockColor.Yellow),
                });
            _board.Load(level);
            var boardView = SceneTestUtility.FindInActiveScene<BoardView>();

            foreach (DoorData door in level.Doors)
            {
                Transform segment = boardView.DoorSegmentAt(door.Side, door.Start);
                Vector3 outward = door.Side switch
                {
                    WallSide.Bottom => Vector3.back,
                    WallSide.Top => Vector3.forward,
                    WallSide.Left => Vector3.left,
                    _ => Vector3.right,
                };
                var arrows = new List<Transform>();
                foreach (Transform child in segment)
                    if (child.name.StartsWith("Door_Arrow"))
                        arrows.Add(child);

                Assert.That(arrows.Count, Is.EqualTo(1), $"{door.Side} door");
                Transform arrow = arrows[0];
                Assert.That(Vector3.Dot(-arrow.forward, outward), Is.EqualTo(1f).Within(1e-3f), "Points out.");
                Assert.That(arrow.lossyScale.x, Is.EqualTo(arrow.lossyScale.z).Within(1e-4f), "Not stretched.");
                Vector3 offset = arrow.position - segment.position;
                offset -= Vector3.Project(offset, Vector3.up);
                Assert.That(offset.magnitude, Is.LessThan(0.01f * _space.CellSize), $"{door.Side} arrow is centred");
            }
        }

        void AssertSideIsClosed(List<Bounds> ring, System.Func<Bounds, float> across, System.Func<Bounds, float> start,
            System.Func<Bounds, float> end, bool lowest)
        {
            float tolerance = 0.002f * _space.CellSize;
            float edge = across(ring[0]);
            foreach (Bounds part in ring)
                edge = lowest ? Mathf.Min(edge, across(part)) : Mathf.Max(edge, across(part));
            var side = new List<Bounds>();
            foreach (Bounds part in ring)
                if (Mathf.Abs(across(part) - edge) < tolerance)
                    side.Add(part);
            side.Sort((a, b) => start(a).CompareTo(start(b)));

            for (int i = 1; i < side.Count; i++)
                Assert.That(start(side[i]), Is.EqualTo(end(side[i - 1])).Within(tolerance), $"gap before part {i}");
            float ringStart = float.MaxValue;
            float ringEnd = float.MinValue;
            foreach (Bounds part in ring)
            {
                ringStart = Mathf.Min(ringStart, start(part));
                ringEnd = Mathf.Max(ringEnd, end(part));
            }

            Assert.That(start(side[0]), Is.EqualTo(ringStart).Within(tolerance));
            Assert.That(end(side[side.Count - 1]), Is.EqualTo(ringEnd).Within(tolerance));
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

                BoardViewConfig config = _container.Resolve<BoardViewConfig>();
                yield return WaitRealtime((1f + config.ExitCutOffset + config.ExitClearance) / config.ExitSpeed + 0.2f);
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
        public IEnumerator Exit_ShouldOpenTheDoorAndCloseItAfter_WhenTheBlockLeaves()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            Transform door = SceneTestUtility.FindInActiveScene<BoardView>().DoorSegmentAt(WallSide.Left, 1);
            BoardViewConfig config = _container.Resolve<BoardViewConfig>();
            float closedY = door.localPosition.y;

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            yield return WaitRealtime(config.ExitSnapDuration + 0.05f);
            float whileOpen = door.localPosition.y;
            yield return WaitRealtime((1f + config.ExitCutOffset + config.ExitClearance) / config.ExitSpeed + config.DoorGateCloseDuration + 0.2f);

            Assert.That(whileOpen, Is.LessThan(closedY - config.DoorGateDepth * 0.5f), "Sunk into the floor.");
            Assert.That(door.localPosition.y, Is.EqualTo(closedY).Within(1e-3f), "Closed again.");
        }

        [UnityTest]
        public IEnumerator Exit_ShouldSinkTheWholeDoor_WhenASmallerBlockLeavesThroughIt()
        {
            yield return BootToBoard();
            _board.Load(new LevelData(4, 3, 60,
                new[] { new BlockData(BlockColor.Red, new[] { new Vector2Int(0, 1) }) },
                new[] { new DoorData(WallSide.Left, 0, 3, BlockColor.Red) }));
            var boardView = SceneTestUtility.FindInActiveScene<BoardView>();
            Transform door = boardView.DoorSegmentAt(WallSide.Left, 1);
            BoardViewConfig config = _container.Resolve<BoardViewConfig>();

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            yield return WaitRealtime(config.ExitSnapDuration + 0.05f);

            Assert.That(boardView.DoorSegmentAt(WallSide.Left, 0), Is.SameAs(door), "One segment for the whole door.");
            Assert.That(boardView.DoorSegmentAt(WallSide.Left, 2), Is.SameAs(door));
            Assert.That(door.localScale.x, Is.EqualTo(3f), "Stretched over its three cells.");
            Assert.That(door.localPosition.y, Is.LessThan(-config.DoorGateDepth * 0.5f), "The whole door sank.");
        }

        [UnityTest]
        public IEnumerator Exit_ShouldShareOneSpray_WhenTwoBlocksLeaveTheSameDoorTogether()
        {
            yield return BootToBoard();
            _board.Load(new LevelData(4, 3, 60,
                new[]
                {
                    new BlockData(BlockColor.Red, new[] { new Vector2Int(0, 1) }),
                    new BlockData(BlockColor.Red, new[] { new Vector2Int(1, 1) }),
                },
                new[] { new DoorData(WallSide.Left, 1, 1, BlockColor.Red) }));

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            Swipe(new Vector2Int(1, 1), new Vector2Int(-2, 1));
            yield return null;

            Assert.That(_board.Current.IsCleared, Is.True);
            ExitBurstView burst = SceneTestUtility.FindInActiveScene<ExitBurstView>();
            Assert.That(burst.transform.parent.GetComponentsInChildren<ExitBurstView>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Exit_ShouldSettleAtTheDoorBeforeSlidingIn_WhenTheBlockLeaves()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView red = _blocks.Views[0];
            Vector3 mouth = red.transform.localPosition;
            float snap = _container.Resolve<BoardViewConfig>().ExitSnapDuration;

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            yield return WaitRealtime(snap * 0.5f);
            float whileSettling = red.transform.localPosition.x;
            yield return WaitRealtime(snap + 0.1f);

            Assert.That(whileSettling, Is.GreaterThanOrEqualTo(mouth.x - 1e-3f), "Not into the door while settling.");
            Assert.That(red.transform.localPosition.x, Is.LessThan(mouth.x - 0.1f), "Then it slides in.");
        }

        [UnityTest]
        public IEnumerator Exit_ShouldCutTheBlockAtTheDoorAndSprayDebris_WhenItLeaves()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView red = _blocks.Views[0];

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            yield return null;

            Assert.That(red.transform.localScale, Is.EqualTo(Vector3.one), "It does not shrink; the door cuts it.");
            int cut = 0;
            foreach (MeshRenderer mesh in red.GetComponentsInChildren<MeshRenderer>())
            {
                if (mesh.sharedMaterial.shader.name != "ColorBlockJam/Block Exit")
                    continue;
                cut++;
                Vector4 plane = mesh.sharedMaterial.GetVector("_ClipPlane");
                float cutX = _space.Origin.x - _container.Resolve<BoardViewConfig>().ExitCutOffset * _space.CellSize;
                Assert.That(new Vector3(plane.x, plane.y, plane.z), Is.EqualTo(Vector3.right));
                Assert.That(plane.w, Is.EqualTo(-cutX).Within(1e-4f));
            }

            Assert.That(cut, Is.EqualTo(red.PartCount), "Every piece of the exiting block uses the exit material.");
            ExitBurstView burst = SceneTestUtility.FindInActiveScene<ExitBurstView>();
            Assert.That(burst.GetComponent<ParticleSystem>().isPlaying, Is.True, "Debris sprays out of the door.");
        }

        [UnityTest]
        public IEnumerator BlockView_ShouldGlideAfterTheFinger_WhenItMovesSuddenly()
        {
            yield return BootToBoard();
            _board.Load(TwoBlockLevel());
            BlockView view = _blocks.Views[1];
            Vector3 start = view.transform.localPosition;
            Vector3 end = start + new Vector3(-1f, 0f, 0f);
            float followDuration = _container.Resolve<BoardViewConfig>().FollowDuration;

            view.Follow(new Vector2(-1f, 0f));
            Vector3 sameFrame = view.transform.localPosition;
            yield return WaitRealtime(followDuration + 0.1f);

            Assert.That(Vector3.Distance(sameFrame, start), Is.LessThan(0.01f), "No jump on the frame of the flick.");
            Assert.That(Vector3.Distance(view.transform.localPosition, end), Is.LessThan(1e-3f),
                "Caught up once the follow time has passed.");
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

            Swipe(new Vector2Int(0, 1), new Vector2Int(-2, 1));
            Swipe(new Vector2Int(3, 1), new Vector2Int(-2, 1));

            Assert.That(_board.Current.Blocks[1].HasExited, Is.False);
            Assert.That(_board.Current.Blocks[1].GetCell(0), Is.EqualTo(new Vector2Int(0, 1)));
        }
    }
}

using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Gameplay.Animations;
using DG.Tweening;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Builds the static board for each attempt: floor tiles over a base (the gaps read as grid lines), the
    /// wall ring, coloured doors with an arrow in the middle pointing out, and rounded corners. Parts sit under a root at <see cref="BoardSpace.Origin"/>
    /// scaled by the cell size, so positions here are in cells. The wall and door meshes are straight
    /// extrusions, so each stretch of wall between doors (or corners) is one segment stretched along it and
    /// each door one segment stretched over its cells: the ring reads as continuous bars on any board size.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField]
        Transform _root;

        [SerializeField]
        Transform _storage;

        BoardViewConfig _config;
        BoardSpace _boardSpace;
        SignalBus _signalBus;
        ColorMaterialCache _doorMaterials;
        // Length of the wall mesh along its run, in cells (art-pipeline.md §4).
        const float WallSegmentLength = 0.5f;

        readonly List<(PartPool Pool, GameObject Part)> _built = new();
        // The door segment covering each wall cell (every cell of a door maps to its one segment), and the
        // gate tween each segment is playing.
        readonly Dictionary<(WallSide Side, int Index), Transform> _doorSegments = new();
        readonly Dictionary<Transform, Sequence> _gates = new();
        LevelData _level;
        PartPool _base, _tiles, _walls, _corners, _doors, _arrows;

        [Inject]
        public void Construct(BoardViewConfig config, BoardSpace boardSpace, SignalBus signalBus)
        {
            _config = config;
            _boardSpace = boardSpace;
            _signalBus = signalBus;
            _doorMaterials = new ColorMaterialCache(config.DoorMaterial, config);
            _base = new PartPool(config.BoardBase, _storage);
            _tiles = new PartPool(config.FloorTile, _storage);
            _walls = new PartPool(config.WallSegment, _storage);
            _corners = new PartPool(config.WallCorner, _storage);
            _doors = new PartPool(config.DoorSegment, _storage);
            _arrows = new PartPool(config.DoorArrow, _storage);
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
        }

        /// <summary>Parts currently making up the board.</summary>
        internal int BuiltPartCount => _built.Count;

        /// <summary>The door segment on <paramref name="side"/> at <paramref name="index"/> cells along it.</summary>
        internal Transform DoorSegmentAt(WallSide side, int index) => _doorSegments[(side, index)];

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
            _root.localPosition = _boardSpace.Origin;
            _root.localScale = Vector3.one * _boardSpace.CellSize;

            LevelData level = signal.Level;
            _level = level;
            BuildFloor(level.Width, level.Height);
            BuildWall(level, WallSide.Bottom);
            BuildWall(level, WallSide.Top);
            BuildWall(level, WallSide.Left);
            BuildWall(level, WallSide.Right);
            BuildCorners(level.Width, level.Height);
        }

        void BuildFloor(int width, int height)
        {
            Place(_base, new Vector3(width * 0.5f, -_config.BaseDepth, height * 0.5f), 0f, new Vector3(width, 1f, height));
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Place(_tiles, new Vector3(x + 0.5f, 0f, y + 0.5f), 0f, Vector3.one);
        }

        /// <summary>One wall segment per stretch between doors, one door segment per door.</summary>
        void BuildWall(LevelData level, WallSide side)
        {
            int length = level.WallLength(side);
            int runStart = 0;
            int index = 0;
            while (index < length)
            {
                DoorData door = DoorAt(level, side, index);
                if (door == null)
                {
                    index++;
                    continue;
                }

                PlaceWallRun(level, side, runStart, index);
                PlaceDoor(level, side, door);
                index = door.End + 1;
                runStart = index;
            }

            PlaceWallRun(level, side, runStart, length);
        }

        /// <summary>The wall from cell <paramref name="from"/> up to (not including) <paramref name="to"/>.</summary>
        void PlaceWallRun(LevelData level, WallSide side, int from, int to)
        {
            if (to <= from)
                return;
            // The wall mesh runs along its local z, WallSegmentLength cells long; along x walls turn a quarter.
            float yaw = side is WallSide.Bottom or WallSide.Top ? 90f : 0f;
            var stretch = new Vector3(1f, 1f, (to - from) / WallSegmentLength);
            Place(_walls, WallPoint(level, side, (from + to) * 0.5f), yaw, stretch);
        }

        void PlaceDoor(LevelData level, WallSide side, DoorData door)
        {
            // The door mesh runs along its local x, one cell long.
            var stretch = new Vector3(door.Length, 1f, 1f);
            GameObject part = Place(_doors, WallPoint(level, side, door.Start + door.Length * 0.5f), DoorYaw(side), stretch);
            part.GetComponentInChildren<MeshRenderer>().sharedMaterial = _doorMaterials.Get(door.Color);
            for (int cell = door.Start; cell <= door.End; cell++)
                _doorSegments[(side, cell)] = part.transform;
            PlaceArrow(part.transform, door.Length);
        }

        /// <summary>
        /// One arrow on top of the door, in its middle however wide it is (DA-24). It hangs under the door so it
        /// sinks with it, undoing the door's stretch; the door's local -z points out of the board on every side.
        /// </summary>
        void PlaceArrow(Transform door, int doorLength)
        {
            GameObject arrow = _arrows.Get(door);
            arrow.transform.localPosition = Vector3.zero;
            arrow.transform.localRotation = Quaternion.identity;
            arrow.transform.localScale = new Vector3(1f / doorLength, 1f, 1f);
            _built.Add((_arrows, arrow));
        }

        void BuildCorners(int width, int height)
        {
            // The corner mesh rounds toward +x +z; each board corner turns it outward.
            float half = _config.WallThickness * 0.5f;
            Place(_corners, new Vector3(width + half, 0f, height + half), 0f, Vector3.one);
            Place(_corners, new Vector3(width + half, 0f, -half), 90f, Vector3.one);
            Place(_corners, new Vector3(-half, 0f, -half), 180f, Vector3.one);
            Place(_corners, new Vector3(-half, 0f, height + half), 270f, Vector3.one);
        }

        /// <summary>A point on the centre line of a wall, <paramref name="along"/> cells from its start.</summary>
        Vector3 WallPoint(LevelData level, WallSide side, float along)
        {
            float half = _config.WallThickness * 0.5f;
            return side switch
            {
                WallSide.Bottom => new Vector3(along, 0f, -half),
                WallSide.Top => new Vector3(along, 0f, level.Height + half),
                WallSide.Left => new Vector3(-half, 0f, along),
                _ => new Vector3(level.Width + half, 0f, along),
            };
        }

        /// <summary>The door mesh runs along x; side walls turn it a quarter, the top wall faces it inward.</summary>
        float DoorYaw(WallSide side) => side switch
        {
            WallSide.Bottom => 0f,
            WallSide.Top => 180f,
            WallSide.Left => 90f,
            _ => 270f,
        };

        DoorData DoorAt(LevelData level, WallSide side, int index)
        {
            foreach (DoorData door in level.Doors)
                if (door.Side == side && index >= door.Start && index <= door.End)
                    return door;
            return null;
        }

        GameObject Place(PartPool pool, Vector3 position, float yaw, Vector3 scale)
        {
            GameObject part = pool.Get(_root);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            part.transform.localScale = scale;
            _built.Add((pool, part));
            return part;
        }

        /// <summary>
        /// Opens the door a block leaves through, the whole door however wide the block: it sinks into the
        /// floor while the block settles, stays down while it slides through, then rises back.
        /// </summary>
        void OnBlockExited(BlockExitedSignal signal)
        {
            var path = new ExitPath(_level.Blocks[signal.BlockId].Cells, signal.Offset, signal.Direction,
                _boardSpace.Size);
            float hold = (path.Length + _config.ExitCutOffset + _config.ExitClearance) / _config.ExitSpeed;
            // A block leaves only through a door that spans it, so its first cell names that door.
            Transform door = _doorSegments[(path.Side, path.CrossMin)];
            if (_gates.TryGetValue(door, out Sequence running))
                running.Kill();
            _gates[door] = default(DoorGateAnimation).Play(new DoorGateAnimation.Config(door, 0f,
                _config.DoorGateDepth, _config.ExitSnapDuration, hold, _config.DoorGateCloseDuration));
        }

        void ReleaseAll()
        {
            foreach (Sequence gate in _gates.Values)
                gate.Kill();
            _gates.Clear();
            _doorSegments.Clear();
            foreach ((PartPool pool, GameObject part) in _built)
                pool.Release(part);
            _built.Clear();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
            foreach (Sequence gate in _gates.Values)
                gate.Kill();
            _doorMaterials.Dispose();
        }
    }
}

using System.Collections.Generic;
using RollicCase.Core.Signals;
using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using UnityEngine;
using VContainer;

namespace RollicCase.Game.Gameplay.View
{
    /// <summary>
    /// Builds the static board for each attempt: floor tiles over a base (the gaps read as grid lines), the
    /// wall ring, coloured doors and rounded corners. Parts sit under a root at <see cref="BoardSpace.Origin"/>
    /// scaled by the cell size, so positions here are in cells.
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
        readonly List<(PartPool Pool, GameObject Part)> _built = new();
        PartPool _base, _tiles, _walls, _corners, _doors;

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
            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
        }

        /// <summary>Parts currently making up the board.</summary>
        internal int BuiltPartCount => _built.Count;

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
            _root.localPosition = _boardSpace.Origin;
            _root.localScale = Vector3.one * _boardSpace.CellSize;

            LevelData level = signal.Level;
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

        void BuildWall(LevelData level, WallSide side)
        {
            int length = level.WallLength(side);
            for (int index = 0; index < length; index++)
            {
                DoorData door = DoorAt(level, side, index);
                if (door != null)
                {
                    GameObject part = Place(_doors, WallPoint(level, side, index + 0.5f), DoorYaw(side), Vector3.one);
                    part.GetComponentInChildren<MeshRenderer>().sharedMaterial = _doorMaterials.Get(door.Color);
                    continue;
                }

                // Wall segments are half a cell long: two per cell.
                Place(_walls, WallPoint(level, side, index + 0.25f), 0f, Vector3.one);
                Place(_walls, WallPoint(level, side, index + 0.75f), 0f, Vector3.one);
            }
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

        void ReleaseAll()
        {
            foreach ((PartPool pool, GameObject part) in _built)
                pool.Release(part);
            _built.Clear();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
            _doorMaterials.Dispose();
        }
    }
}

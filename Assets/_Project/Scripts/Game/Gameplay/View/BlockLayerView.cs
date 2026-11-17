using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// Draws the blocks: builds each one from modular pieces when a board loads, follows the finger while a
    /// block is held, settles it on its cell when dropped and plays its exit. Boosters (DA-36) smash or vacuum
    /// blocks away, and a blasted cell rebuilds its block from the cells left; while a booster waits for a target
    /// every block is outlined. Reacts to signals only; the board model has already committed every change. (Blocks move only by dragging, so the per-cell
    /// <see cref="BlockMovedSignal"/> needs no view of its own.)
    /// </summary>
    public sealed class BlockLayerView : MonoBehaviour
    {
        [SerializeField]
        Transform _root;

        [SerializeField]
        Transform _storage;

        [SerializeField]
        BlockView _blockPrefab;

        readonly BlockTileResolver _tileResolver = new();
        readonly List<BlockView> _views = new();
        readonly List<BlockView> _exiting = new();
        BoardViewConfig _config;
        BoardSpace _boardSpace;
        SignalBus _signalBus;
        ColorMaterialCache _materials;
        ColorMaterialCache _iceMaterials;
        System.Action<ExitBurstView> _releaseBurst;
        ObjectPool<BlockView> _blockPool;
        Dictionary<BlockPieceKind, PartPool> _piecePools;
        ObjectPool<ExitBurstView> _burstPool;
        ObjectPool<ExitBurstView> _shatterPool;
        System.Action<ExitBurstView> _releaseShatter;
        // The last spray started at each door, to fold exits close together into it.
        readonly Dictionary<(WallSide Side, int CrossMin, int CrossMax), (ExitBurstView Burst, float StartedAt)> _lastBursts = new();
        ActiveBoard _board;
        OutlineSettings _outline;

        [Inject]
        public void Construct(BoardViewConfig config, BoardSpace boardSpace, ActiveBoard board, SignalBus signalBus)
        {
            _config = config;
            _boardSpace = boardSpace;
            _board = board;
            _signalBus = signalBus;
            _materials = new ColorMaterialCache(config.BlockMaterial, config);
            _iceMaterials = new ColorMaterialCache(config.IceMaterial, config, config.IceWhiten);
            _outline = new OutlineSettings(config.OutlineWidth, config.OutlineBaseHeight, config.BlockTopHeight,
                config.OutlineGap, config.BlockCornerRadius, config.OutlineCornerSegments);
            _blockPool = new ObjectPool<BlockView>(
                () => Instantiate(_blockPrefab, _storage),
                view => view.gameObject.SetActive(true),
                view => view.gameObject.SetActive(false),
                view => Destroy(view.gameObject));
            _piecePools = new Dictionary<BlockPieceKind, PartPool>
            {
                [BlockPieceKind.Center] = new(config.BlockCenter, _storage),
                [BlockPieceKind.Edge] = new(config.BlockEdge, _storage),
                [BlockPieceKind.OuterCorner] = new(config.BlockOuterCorner, _storage),
                [BlockPieceKind.InnerCorner] = new(config.BlockInnerCorner, _storage),
            };

            _burstPool = new ObjectPool<ExitBurstView>(
                () => Instantiate(config.ExitBurst, _root),
                burst => burst.gameObject.SetActive(true),
                burst => burst.gameObject.SetActive(false),
                burst => Destroy(burst.gameObject));
            _releaseBurst = _burstPool.Release;
            _shatterPool = new ObjectPool<ExitBurstView>(
                () => Instantiate(config.IceShatter, _root),
                shatter => shatter.gameObject.SetActive(true),
                shatter => shatter.gameObject.SetActive(false),
                shatter => Destroy(shatter.gameObject));
            _releaseShatter = _shatterPool.Release;

            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<BlockDraggedSignal>(OnBlockDragged);
            _signalBus.Subscribe<BlockDroppedSignal>(OnBlockDropped);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Subscribe<BlockRemovedSignal>(OnBlockRemoved);
            _signalBus.Subscribe<BlockCellRemovedSignal>(OnBlockCellRemoved);
            _signalBus.Subscribe<BlockSplitOffSignal>(OnBlockSplitOff);
            _signalBus.Subscribe<BoosterTargetingChangedSignal>(OnBoosterTargetingChanged);
            _signalBus.Subscribe<IceCountedDownSignal>(OnIceCountedDown);
            _signalBus.Subscribe<BlockThawedSignal>(OnBlockThawed);
        }

        /// <summary>Block views on screen, indexed by block id; null once a block has exited.</summary>
        internal IReadOnlyList<BlockView> Views => _views;

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
            _root.localPosition = _boardSpace.Origin;
            _root.localScale = Vector3.one * _boardSpace.CellSize;

            foreach (BlockData block in signal.Level.Blocks)
            {
                BlockView view = _blockPool.Get();
                view.transform.SetParent(_root, false);
                Build(view, block.Cells, block.Color, block.Ice);
                _views.Add(view);
            }
        }

        /// <summary>
        /// Lays the pieces of a block with <paramref name="cells"/> (start cells) into the view; a frozen block
        /// (<paramref name="ice"/> above 0) is drawn in ice with its count on top.
        /// </summary>
        void Build(BlockView view, IReadOnlyList<Vector2Int> cells, BlockColor color, int ice)
        {
            Vector3 centre = Centre(cells);
            view.Show(centre, _config.SnapDuration, _config.SnapEase, _config.FollowDuration, _config.FollowEase);

            Material material = ice > 0 ? _iceMaterials.Get(color) : _materials.Get(color);
            foreach (BlockPiece piece in _tileResolver.Resolve(cells))
            {
                PartPool pool = _piecePools[piece.Kind];
                GameObject part = pool.Get(view.transform);
                var mesh = part.GetComponentInChildren<MeshRenderer>();
                mesh.sharedMaterial = material;
                view.AddPart(pool, part, mesh, piece.Position, piece.Yaw, piece.Scale);
            }

            view.BuildOutline(cells, _outline);
            view.SetIce(ice, LabelCell(cells, centre));
        }

        /// <summary>
        /// Where the ice count sits: the block's centre when that lies on the block (a 2x2 block's middle), else the
        /// middle of its cell nearest the centre, so it is always on the block.
        /// </summary>
        Vector3 LabelCell(IReadOnlyList<Vector2Int> cells, Vector3 centre)
        {
            foreach (Vector2Int cell in cells)
                if (centre.x >= cell.x && centre.x <= cell.x + 1 && centre.z >= cell.y && centre.z <= cell.y + 1)
                    return centre;

            Vector3 best = default;
            float bestDistance = float.MaxValue;
            foreach (Vector2Int cell in cells)
            {
                var middle = new Vector3(cell.x + 0.5f, 0f, cell.y + 0.5f);
                float distance = (middle - centre).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;
                bestDistance = distance;
                best = middle;
            }

            return best;
        }

        Vector3 Centre(IReadOnlyList<Vector2Int> cells)
        {
            Vector2 sum = Vector2.zero;
            foreach (Vector2Int cell in cells)
                sum += cell;
            Vector2 centre = sum / cells.Count + new Vector2(0.5f, 0.5f);
            return new Vector3(centre.x, 0f, centre.y);
        }

        void OnBlockGrabbed(BlockGrabbedSignal signal) => _views[signal.BlockId].SetOutlined(true);

        void OnBlockDragged(BlockDraggedSignal signal) => _views[signal.BlockId].Follow(signal.Offset);

        void OnBlockDropped(BlockDroppedSignal signal)
        {
            BlockView view = _views[signal.BlockId];
            view.SetOutlined(false);
            view.SnapTo(signal.Offset);
        }

        /// <summary>
        /// Slides the block through its door at <see cref="BoardViewConfig.ExitSpeed"/>, cut at the board's edge,
        /// while cubes of its colour spray out of the door for as long as it takes.
        /// </summary>
        void OnBlockExited(BlockExitedSignal signal)
        {
            BlockView view = _views[signal.BlockId];
            _views[signal.BlockId] = null;
            _exiting.Add(view);

            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            var path = new ExitPath(block.StartCells, signal.Offset, signal.Direction, _boardSpace.Size);
            float distance = path.Length + _config.ExitCutOffset + _config.ExitClearance;
            Vector3 doorLocal = path.DoorCentre(_config.ExitDebrisHeight);

            // Keep what lies on the board side of the door: -d . p + d . edge >= 0.
            var outward = new Vector3(signal.Direction.x, 0f, signal.Direction.y);
            // Cut a little inside the door, so the block covers the floor's edge the opening door uncovers.
            Vector3 edgeWorld = _root.TransformPoint(doorLocal + outward * _config.ExitCutOffset);
            var clipPlane = new Vector4(-outward.x, 0f, -outward.z, Vector3.Dot(outward, edgeWorld));

            Color color = _config.TintOf(block.Color);
            float slide = view.PlayExit(signal.Offset, signal.Direction, distance, _config.ExitSpeed,
                _config.ExitSnapDuration, _config.ExitSnapEase, _config.ExitEase, _config.BlockExitMaterial, color,
                clipPlane, Recycle);
            Spray(path, doorLocal + outward * _config.WallThickness, color, slide);
        }

        /// <summary>A booster took the whole block: it breaks where it stands, or the vacuum lifts it away in turn.</summary>
        void OnBlockRemoved(BlockRemovedSignal signal)
        {
            BlockView view = _views[signal.BlockId];
            _views[signal.BlockId] = null;
            _exiting.Add(view);

            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            if (signal.Kind == BlockRemovalKind.Vacuumed)
            {
                view.PlayVacuum(signal.Order * _config.VacuumStagger, _config.VacuumLift, _config.VacuumDuration,
                    Recycle);
                return;
            }

            view.PlaySmash(_config.SmashSwell, _config.SmashDuration, Recycle);
            Vector3 centre = Centre(block.StartCells) + new Vector3(block.Offset.x, 0f, block.Offset.y);
            Burst(centre, _config.TintOf(block.Color), _config.SmashDebrisPerCell * block.CellCount);
        }

        /// <summary>
        /// The rocket blasted one cell: the block is rebuilt from the cells it has left, where it is (a piece that split
        /// off follows with <see cref="BlockSplitOffSignal"/>).
        /// </summary>
        void OnBlockCellRemoved(BlockCellRemovedSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _views[signal.BlockId];
            view.ReleaseParts();
            Build(view, block.StartCells, block.Color, block.Ice);
            view.PlaceAt(block.Offset);
            Burst(new Vector3(signal.Cell.x + 0.5f, 0f, signal.Cell.y + 0.5f), _config.TintOf(block.Color),
                _config.SmashDebrisPerCell);
        }

        /// <summary>A block left: every frozen block shows its new count.</summary>
        void OnIceCountedDown(IceCountedDownSignal signal)
        {
            for (int id = 0; id < _views.Count; id++)
            {
                BoardBlock block = _board.Current.Blocks[id];
                if (_views[id] != null && block.IsFrozen)
                    _views[id].SetIce(block.Ice);
            }
        }

        /// <summary>The ice broke: the block takes its colour back with a swell while shards of ice fly off.</summary>
        void OnBlockThawed(BlockThawedSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _views[signal.BlockId];
            view.SetIce(0);
            view.SetMaterial(_materials.Get(block.Color));
            view.PlayThaw(_config.ThawSwell, _config.ThawDuration);
            Vector3 centre = Centre(block.StartCells) + new Vector3(block.Offset.x, 0f, block.Offset.y);
            ExitBurstView shatter = _shatterPool.Get();
            shatter.Play(centre + Vector3.up * _config.ExitDebrisHeight, Vector2Int.up, 1f, Color.white, 0f,
                _config.ThawDuration, 0f, _config.ThawDebrisPerCell * block.CellCount, _releaseShatter);
        }

        /// <summary>A piece the rocket split off is a block of its own now: it gets its own view where it lies.</summary>
        void OnBlockSplitOff(BlockSplitOffSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _blockPool.Get();
            view.transform.SetParent(_root, false);
            Build(view, block.StartCells, block.Color, block.Ice);
            view.PlaceAt(block.Offset);
            // Board ids are the views' indices; a split-off block always takes the next one.
            _views.Add(view);
        }

        void OnBoosterTargetingChanged(BoosterTargetingChangedSignal signal)
        {
            foreach (BlockView view in _views)
                view?.SetOutlined(signal.Active);
        }

        /// <summary>Cubes of <paramref name="color"/> thrown up from where a block broke or thawed.</summary>
        void Burst(Vector3 local, Color color, float cubes)
        {
            ExitBurstView burst = _burstPool.Get();
            burst.Play(local + Vector3.up * _config.ExitDebrisHeight, Vector2Int.up, 1f, color, 0f,
                _config.SmashDuration, 0f, cubes, _releaseBurst);
        }

        /// <summary>
        /// Starts the debris out of the door, outside the wall ring so the wall does not hide it. A block
        /// leaving the same door within <see cref="BoardViewConfig.ExitBurstCoalesceWindow"/> of the last
        /// one adds its cubes to that spray instead of starting another particle system.
        /// </summary>
        void Spray(in ExitPath path, Vector3 sprayLocal, Color color, float slide)
        {
            var door = (path.Side, path.CrossMin, path.CrossMax);
            float now = Time.unscaledTime;
            if (_lastBursts.TryGetValue(door, out (ExitBurstView Burst, float StartedAt) last)
                && now - last.StartedAt <= _config.ExitBurstCoalesceWindow && last.Burst.gameObject.activeSelf)
            {
                last.Burst.Add(Mathf.RoundToInt(_config.ExitDebrisRate * path.Width * slide), color);
                return;
            }

            ExitBurstView burst = _burstPool.Get();
            burst.Play(sprayLocal, path.Direction, path.Width, color, _config.ExitSnapDuration, slide,
                _config.ExitDebrisRate, _config.ExitDebrisBurst, _releaseBurst);
            _lastBursts[door] = (burst, now);
        }

        void Recycle(BlockView view)
        {
            _exiting.Remove(view);
            view.ReleaseParts();
            _blockPool.Release(view);
        }

        void ReleaseAll()
        {
            // Blocks still sliding out belong to the previous board; they go back to the pool with it.
            for (int i = _exiting.Count - 1; i >= 0; i--)
                Recycle(_exiting[i]);
            foreach (BlockView view in _views)
                if (view != null)
                    Recycle(view);
            _views.Clear();
        }

        void OnDestroy()
        {
            _signalBus.Unsubscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Unsubscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Unsubscribe<BlockDraggedSignal>(OnBlockDragged);
            _signalBus.Unsubscribe<BlockDroppedSignal>(OnBlockDropped);
            _signalBus.Unsubscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Unsubscribe<BlockRemovedSignal>(OnBlockRemoved);
            _signalBus.Unsubscribe<BlockCellRemovedSignal>(OnBlockCellRemoved);
            _signalBus.Unsubscribe<BlockSplitOffSignal>(OnBlockSplitOff);
            _signalBus.Unsubscribe<BoosterTargetingChangedSignal>(OnBoosterTargetingChanged);
            _signalBus.Unsubscribe<IceCountedDownSignal>(OnIceCountedDown);
            _signalBus.Unsubscribe<BlockThawedSignal>(OnBlockThawed);
            _materials.Dispose();
            _iceMaterials.Dispose();
        }
    }
}

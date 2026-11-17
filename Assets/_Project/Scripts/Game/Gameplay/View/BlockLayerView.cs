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
    /// block is held, settles it on its cell when dropped and plays its exit. Reacts to signals only; the
    /// board model has already committed every change. (Blocks move only by dragging, so the per-cell
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
        System.Action<ExitBurstView> _releaseBurst;
        ObjectPool<BlockView> _blockPool;
        Dictionary<BlockPieceKind, PartPool> _piecePools;
        ObjectPool<ExitBurstView> _burstPool;
        // The last spray started at each door, to fold exits close together into it.
        readonly Dictionary<(WallSide Side, int CrossMin, int CrossMax), (ExitBurstView Burst, float StartedAt)> _lastBursts = new();
        LevelData _level;
        OutlineSettings _outline;

        [Inject]
        public void Construct(BoardViewConfig config, BoardSpace boardSpace, SignalBus signalBus)
        {
            _config = config;
            _boardSpace = boardSpace;
            _signalBus = signalBus;
            _materials = new ColorMaterialCache(config.BlockMaterial, config);
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

            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<BlockDraggedSignal>(OnBlockDragged);
            _signalBus.Subscribe<BlockDroppedSignal>(OnBlockDropped);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
            _signalBus.Subscribe<BlockRejectedSignal>(OnBlockRejected);
        }

        /// <summary>Block views on screen, indexed by block id; null once a block has exited.</summary>
        internal IReadOnlyList<BlockView> Views => _views;

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
            _level = signal.Level;
            _root.localPosition = _boardSpace.Origin;
            _root.localScale = Vector3.one * _boardSpace.CellSize;

            foreach (BlockData block in signal.Level.Blocks)
                _views.Add(Build(block));
        }

        BlockView Build(BlockData block)
        {
            Vector2 sum = Vector2.zero;
            foreach (Vector2Int cell in block.Cells)
                sum += cell;
            Vector2 centre = sum / block.Cells.Count + new Vector2(0.5f, 0.5f);

            BlockView view = _blockPool.Get();
            view.transform.SetParent(_root, false);
            view.Show(new Vector3(centre.x, 0f, centre.y), _config.SnapDuration, _config.SnapEase,
                _config.FollowDuration, _config.FollowEase);

            Material material = _materials.Get(block.Color);
            foreach (BlockPiece piece in _tileResolver.Resolve(block.Cells))
            {
                PartPool pool = _piecePools[piece.Kind];
                GameObject part = pool.Get(view.transform);
                var mesh = part.GetComponentInChildren<MeshRenderer>();
                mesh.sharedMaterial = material;
                view.AddPart(pool, part, mesh, piece.Position, piece.Yaw, piece.Scale);
            }

            view.BuildOutline(block.Cells, _outline);

            return view;
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

            BlockData block = _level.Blocks[signal.BlockId];
            var path = new ExitPath(block.Cells, signal.Offset, signal.Direction, _boardSpace.Size);
            float distance = path.Length + _config.ExitClearance;
            Vector3 doorLocal = path.DoorCentre(_config.ExitDebrisHeight);

            // Keep what lies on the board side of the door: -d . p + d . edge >= 0.
            var outward = new Vector3(signal.Direction.x, 0f, signal.Direction.y);
            Vector3 edgeWorld = _root.TransformPoint(doorLocal);
            var clipPlane = new Vector4(-outward.x, 0f, -outward.z, Vector3.Dot(outward, edgeWorld));

            Color color = _config.TintOf(block.Color);
            float slide = view.PlayExit(signal.Offset, signal.Direction, distance, _config.ExitSpeed,
                _config.ExitSnapDuration, _config.ExitSnapEase, _config.ExitEase, _config.BlockExitMaterial, color,
                clipPlane, Recycle);
            Spray(path, doorLocal + outward * _config.WallThickness, color, slide);
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
            burst.Play(sprayLocal, path.Direction, path.Width, _config.ExitDebrisHeight, color,
                _config.ExitSnapDuration, slide, _config.ExitDebrisRate, _releaseBurst);
            _lastBursts[door] = (burst, now);
        }

        void OnBlockRejected(BlockRejectedSignal signal) => _views[signal.BlockId].PlayReject(
            signal.Direction, _config.RejectStrength, _config.RejectDuration, _config.RejectVibrato);

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
            _signalBus.Unsubscribe<BlockRejectedSignal>(OnBlockRejected);
            _materials.Dispose();
        }
    }
}

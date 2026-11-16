using System.Collections.Generic;
using RollicCase.Core.Signals;
using RollicCase.Game.Configs;
using RollicCase.Game.Gameplay.Board;
using RollicCase.Game.Gameplay.Signals;
using RollicCase.Game.Levels;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace RollicCase.Game.Gameplay.View
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
        ObjectPool<BlockView> _blockPool;
        Dictionary<BlockPieceKind, PartPool> _piecePools;

        [Inject]
        public void Construct(BoardViewConfig config, BoardSpace boardSpace, SignalBus signalBus)
        {
            _config = config;
            _boardSpace = boardSpace;
            _signalBus = signalBus;
            _materials = new ColorMaterialCache(config.BlockMaterial, config);
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

            _signalBus.Subscribe<BoardLoadedSignal>(OnBoardLoaded);
            _signalBus.Subscribe<BlockGrabbedSignal>(OnBlockGrabbed);
            _signalBus.Subscribe<BlockDraggedSignal>(OnBlockDragged);
            _signalBus.Subscribe<BlockDroppedSignal>(OnBlockDropped);
            _signalBus.Subscribe<BlockExitedSignal>(OnBlockExited);
        }

        /// <summary>Block views on screen, indexed by block id; null once a block has exited.</summary>
        internal IReadOnlyList<BlockView> Views => _views;

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
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
            view.Show(new Vector3(centre.x, 0f, centre.y), _config.SnapDuration, _config.SnapEase);

            Material material = _materials.Get(block.Color);
            foreach (BlockPiece piece in _tileResolver.Resolve(block.Cells))
            {
                PartPool pool = _piecePools[piece.Kind];
                GameObject part = pool.Get(view.transform);
                part.GetComponentInChildren<MeshRenderer>().sharedMaterial = material;
                view.AddPart(pool, part, piece.Position, piece.Yaw, piece.Scale);
            }

            view.BuildOutline(block.Cells, _config.OutlineWidth, _config.OutlineHeight);

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

        void OnBlockExited(BlockExitedSignal signal)
        {
            BlockView view = _views[signal.BlockId];
            _views[signal.BlockId] = null;
            _exiting.Add(view);
            view.PlayExit(signal.Direction, _config.ExitDistance, _config.ExitDuration, _config.ExitEase, Recycle);
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
            _materials.Dispose();
        }
    }
}

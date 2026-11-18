using System.Collections.Generic;
using ColorBlockJam.Core.Signals;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Gameplay.Board;
using ColorBlockJam.Game.Gameplay.Board.Blockers;
using ColorBlockJam.Game.Gameplay.View.Blockers;
using ColorBlockJam.Game.Gameplay.Signals;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace ColorBlockJam.Game.Gameplay.View
{
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
        ObjectPool<IceBlockerView> _icePool;
        ObjectPool<ArrowBlockerView> _arrowPool;
        readonly Dictionary<BlockView, IceBlockerView> _iceViews = new();
        readonly Dictionary<BlockView, ArrowBlockerView> _arrowViews = new();
        System.Action<ExitBurstView> _releaseBurst;
        ObjectPool<BlockView> _blockPool;
        Dictionary<BlockPieceKind, PartPool> _piecePools;
        ObjectPool<ExitBurstView> _burstPool;
        ObjectPool<ExitBurstView> _shatterPool;
        System.Action<ExitBurstView> _releaseShatter;
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
            _icePool = new ObjectPool<IceBlockerView>(
                () => Instantiate(config.IceBlocker, _storage),
                ice => ice.gameObject.SetActive(true),
                ice => ice.gameObject.SetActive(false),
                ice => Destroy(ice.gameObject));
            _arrowPool = new ObjectPool<ArrowBlockerView>(
                () => Instantiate(config.ArrowBlocker, _storage),
                arrow => arrow.gameObject.SetActive(true),
                arrow => arrow.gameObject.SetActive(false),
                arrow => Destroy(arrow.gameObject));
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

        internal IReadOnlyList<BlockView> Views => _views;

        void OnBoardLoaded(BoardLoadedSignal signal)
        {
            ReleaseAll();
            _root.localPosition = _boardSpace.Origin;
            _root.localScale = Vector3.one * _boardSpace.CellSize;

            foreach (BoardBlock block in _board.Current.Blocks)
            {
                BlockView view = _blockPool.Get();
                view.transform.SetParent(_root, false);
                Build(view, block);
                _views.Add(view);
            }
        }

        internal ArrowBlockerView ArrowViewOf(int blockId) =>
            _views[blockId] != null && _arrowViews.TryGetValue(_views[blockId], out ArrowBlockerView arrow) ? arrow : null;

        internal IceBlockerView IceViewOf(int blockId) =>
            _views[blockId] != null && _iceViews.TryGetValue(_views[blockId], out IceBlockerView ice) ? ice : null;

        void Build(BlockView view, BoardBlock block)
        {
            IReadOnlyList<Vector2Int> cells = block.StartCells;
            Vector3 centre = Centre(cells);
            view.Show(centre, _config.SnapDuration, _config.SnapEase, _config.FollowDuration, _config.FollowEase);

            bool frozen = block.TryGetBlocker(out IceBlocker ice) && ice.IsFrozen;
            Material material = frozen ? _iceMaterials.Get(block.Color) : _materials.Get(block.Color);
            foreach (BlockPiece piece in _tileResolver.Resolve(cells))
            {
                PartPool pool = _piecePools[piece.Kind];
                GameObject part = pool.Get(view.transform);
                var mesh = part.GetComponentInChildren<MeshRenderer>();
                mesh.sharedMaterial = material;
                view.AddPart(pool, part, mesh, piece.Position, piece.Yaw, piece.Scale);
            }

            view.BuildOutline(cells, _outline);
            if (block.TryGetBlocker(out ArrowBlocker arrow))
            {
                ArrowBlockerView arrowView = _arrowPool.Get();
                view.AttachBlockerView(arrowView.transform, centre);
                arrowView.Show(arrow.Axis, Span(cells, arrow.Axis));
                _arrowViews[view] = arrowView;
            }

            if (!frozen)
                return;
            IceBlockerView iceView = _icePool.Get();
            view.AttachBlockerView(iceView.transform, LabelCell(cells, centre));
            iceView.Show(ice.ExitsLeft);
            _iceViews[view] = iceView;
        }

        int Span(IReadOnlyList<Vector2Int> cells, ArrowAxis axis)
        {
            int min = int.MaxValue, max = int.MinValue;
            foreach (Vector2Int cell in cells)
            {
                int value = axis == ArrowAxis.Horizontal ? cell.x : cell.y;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            return max - min + 1;
        }

        void Undress(BlockView view)
        {
            if (_arrowViews.Remove(view, out ArrowBlockerView arrowView))
            {
                arrowView.transform.SetParent(_storage, false);
                _arrowPool.Release(arrowView);
            }

            UndressIce(view);
        }

        void UndressIce(BlockView view)
        {
            if (!_iceViews.Remove(view, out IceBlockerView iceView))
                return;
            iceView.transform.SetParent(_storage, false);
            _icePool.Release(iceView);
        }

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

        void OnBlockExited(BlockExitedSignal signal)
        {
            BlockView view = _views[signal.BlockId];
            _views[signal.BlockId] = null;
            _exiting.Add(view);

            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            var path = new ExitPath(block.StartCells, signal.Offset, signal.Direction, _boardSpace.Size);
            float distance = _config.ExitSlideDistance(path.Length);
            Vector3 doorLocal = path.DoorCentre(_config.ExitDebrisHeight);

            var outward = new Vector3(signal.Direction.x, 0f, signal.Direction.y);
            Vector3 edgeWorld = _root.TransformPoint(doorLocal + outward * _config.ExitCutOffset);
            var clipPlane = new Vector4(-outward.x, 0f, -outward.z, Vector3.Dot(outward, edgeWorld));

            Color color = _config.TintOf(block.Color);
            float slide = view.PlayExit(signal.Offset, signal.Direction, distance, _config.ExitSpeed,
                _config.ExitSnapDuration, _config.ExitSnapEase, _config.ExitEase, _config.BlockExitMaterial, color,
                clipPlane, Recycle);
            Spray(path, doorLocal + outward * _config.WallThickness, color, slide);
        }

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

        void OnBlockCellRemoved(BlockCellRemovedSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _views[signal.BlockId];
            Undress(view);
            view.ReleaseParts();
            Build(view, block);
            view.PlaceAt(block.Offset);
            Burst(new Vector3(signal.Cell.x + 0.5f, 0f, signal.Cell.y + 0.5f), _config.TintOf(block.Color),
                _config.SmashDebrisPerCell);
        }

        void OnIceCountedDown(IceCountedDownSignal signal)
        {
            for (int id = 0; id < _views.Count; id++)
            {
                IceBlockerView iceView = IceViewOf(id);
                if (iceView != null && _board.Current.Blocks[id].TryGetBlocker(out IceBlocker ice))
                    iceView.Show(ice.ExitsLeft);
            }
        }

        void OnBlockThawed(BlockThawedSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _views[signal.BlockId];
            UndressIce(view);
            view.SetMaterial(_materials.Get(block.Color));
            view.PlayThaw(_config.ThawSwell, _config.ThawDuration);
            Vector3 centre = Centre(block.StartCells) + new Vector3(block.Offset.x, 0f, block.Offset.y);
            ExitBurstView shatter = _shatterPool.Get();
            shatter.Play(centre + Vector3.up * _config.ExitDebrisHeight, Vector2Int.up, 1f, Color.white, 0f,
                _config.ThawDuration, 0f, _config.ThawDebrisPerCell * block.CellCount, _releaseShatter);
        }

        void OnBlockSplitOff(BlockSplitOffSignal signal)
        {
            BoardBlock block = _board.Current.Blocks[signal.BlockId];
            BlockView view = _blockPool.Get();
            view.transform.SetParent(_root, false);
            Build(view, block);
            view.PlaceAt(block.Offset);
            _views.Add(view);
        }

        void OnBoosterTargetingChanged(BoosterTargetingChangedSignal signal)
        {
            foreach (BlockView view in _views)
                view?.SetOutlined(signal.Active);
        }

        void Burst(Vector3 local, Color color, float cubes)
        {
            ExitBurstView burst = _burstPool.Get();
            burst.Play(local + Vector3.up * _config.ExitDebrisHeight, Vector2Int.up, 1f, color, 0f,
                _config.SmashDuration, 0f, cubes, _releaseBurst);
        }

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
            Undress(view);
            view.ReleaseParts();
            _blockPool.Release(view);
        }

        void ReleaseAll()
        {
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

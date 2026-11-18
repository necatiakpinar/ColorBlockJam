using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using ColorBlockJam.Game.Gameplay.Animations;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    public sealed class BlockView : MonoBehaviour
    {
        [Tooltip("Flat outline drawn over everything while the block is held (ColorBlockJam/Selection Outline).")]
        [SerializeField]
        MeshFilter _outlineFilter;

        [SerializeField]
        MeshRenderer _outlineRenderer;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ClipPlaneId = Shader.PropertyToID("_ClipPlane");

        readonly List<(PartPool Pool, GameObject Part, Renderer Mesh)> _parts = new();
        readonly OutlineMeshBuilder _outlineBuilder = new();
        readonly List<Vector3> _outlineVertices = new();
        readonly List<Vector3> _outlineNormals = new();
        readonly List<Vector2> _outlineLifts = new();
        readonly List<int> _outlineTriangles = new();
        Mesh _outlineMesh;
        Vector3 _pivot;
        TweenerCore<Vector3, Vector3, VectorOptions> _move;
        TweenerCore<Vector3, Vector3, VectorOptions> _follow;
        Sequence _exit;
        Sequence _thaw;
        Material _exitMaterial;
        Action<BlockView> _onExitFinished;
        TweenCallback _exitFinished;

        internal int PartCount => _parts.Count;
        internal bool IsOutlined => _outlineRenderer.enabled;

        internal void Show(Vector3 pivot, float snapDuration, Ease snapEase, float followDuration, Ease followEase)
        {
            _pivot = pivot;
            _exit?.Kill();
            _thaw?.Kill();
            transform.localPosition = pivot;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            _move ??= default(BlockMoveAnimation).Create(transform, snapDuration, snapEase);
            _move.Pause();
            _follow ??= default(BlockMoveAnimation).Create(transform, followDuration, followEase);
            _follow.Pause();
        }

        internal void BuildOutline(IReadOnlyList<Vector2Int> cells, in OutlineSettings settings)
        {
            if (_outlineMesh == null)
            {
                _outlineMesh = new Mesh { name = "BlockOutline" };
                _outlineMesh.MarkDynamic();
                _outlineFilter.sharedMesh = _outlineMesh;
            }

            _outlineBuilder.Build(cells, -_pivot, settings, _outlineVertices, _outlineNormals, _outlineLifts,
                _outlineTriangles);
            _outlineMesh.Clear();
            _outlineMesh.SetVertices(_outlineVertices);
            _outlineMesh.SetNormals(_outlineNormals);
            _outlineMesh.SetUVs(0, _outlineLifts);
            _outlineMesh.SetTriangles(_outlineTriangles, 0);
            _outlineMesh.RecalculateBounds();
            SetOutlined(false);
        }

        internal void SetOutlined(bool outlined) => _outlineRenderer.enabled = outlined;

        internal void AttachBlockerView(Transform blockerView, Vector3 cellPosition)
        {
            blockerView.SetParent(transform, false);
            blockerView.localPosition = new Vector3(cellPosition.x - _pivot.x, 0f, cellPosition.z - _pivot.z);
            blockerView.localRotation = Quaternion.identity;
            blockerView.localScale = Vector3.one;
        }

        internal void SetMaterial(Material material)
        {
            for (int i = 0; i < _parts.Count; i++)
                _parts[i].Mesh.sharedMaterial = material;
        }

        internal void PlayThaw(float swell, float duration)
        {
            _thaw?.Kill();
            _thaw = default(BlockThawAnimation).Play(new BlockThawAnimation.Config(transform, swell, duration));
        }

        internal void PlaceAt(Vector2Int offset) => transform.localPosition = _pivot + new Vector3(offset.x, 0f, offset.y);

        internal void PlaySmash(float swell, float duration, Action<BlockView> onFinished)
        {
            Leave(onFinished);
            _exit = default(BlockSmashAnimation).Play(new BlockSmashAnimation.Config(transform, swell, duration,
                _exitFinished));
        }

        internal void PlayVacuum(float delay, float lift, float duration, Action<BlockView> onFinished)
        {
            Leave(onFinished);
            _exit = default(BlockVacuumAnimation).Play(new BlockVacuumAnimation.Config(transform, delay, lift, duration,
                _exitFinished));
        }

        void Leave(Action<BlockView> onFinished)
        {
            _thaw?.Kill();
            _move.Pause();
            _follow.Pause();
            SetOutlined(false);
            _onExitFinished = onFinished;
            _exitFinished ??= OnExitFinished;
        }

        internal void AddPart(PartPool pool, GameObject part, Renderer mesh, Vector3 cellPosition, float yaw,
            Vector3 scale)
        {
            part.transform.SetParent(transform, false);
            part.transform.localPosition = cellPosition - _pivot;
            part.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            part.transform.localScale = scale;
            _parts.Add((pool, part, mesh));
        }

        internal void Follow(Vector2 offset)
        {
            _move.Pause();
            default(BlockMoveAnimation).Retarget(_follow, _pivot + new Vector3(offset.x, 0f, offset.y));
        }

        internal void SnapTo(Vector2Int offset)
        {
            _follow.Pause();
            default(BlockMoveAnimation).Retarget(_move, _pivot + new Vector3(offset.x, 0f, offset.y));
        }

        internal float PlayExit(Vector2Int offset, Vector2Int direction, float distance, float speed,
            float snapDuration, Ease snapEase, Ease ease, Material exitTemplate, Color color, Vector4 clipPlane,
            Action<BlockView> onFinished)
        {
            Leave(onFinished);
            UseExitMaterial(exitTemplate, color, clipPlane);

            var outward = new Vector3(direction.x, 0f, direction.y);
            Vector3 lined = _pivot + new Vector3(offset.x, 0f, offset.y);
            float alreadyIn = Mathf.Max(0f, Vector3.Dot(transform.localPosition - lined, outward));
            Vector3 door = lined + outward * alreadyIn;
            Vector3 end = lined + outward * distance;
            float slide = (distance - alreadyIn) / speed;
            _exit = default(BlockExitAnimation).Play(new BlockExitAnimation.Config(
                transform, door, snapDuration, snapEase, end, slide, ease, _exitFinished));
            return slide;
        }

        void UseExitMaterial(Material template, Color color, Vector4 clipPlane)
        {
            if (_exitMaterial == null)
                _exitMaterial = new Material(template) { name = template.name + "_" + name };
            _exitMaterial.SetColor(BaseColorId, color);
            _exitMaterial.SetVector(ClipPlaneId, clipPlane);
            for (int i = 0; i < _parts.Count; i++)
                _parts[i].Mesh.sharedMaterial = _exitMaterial;
        }

        internal void ReleaseParts()
        {
            _exit?.Kill();
            _thaw?.Kill();
            _move?.Pause();
            _follow?.Pause();
            foreach ((PartPool pool, GameObject part, _) in _parts)
                pool.Release(part);
            _parts.Clear();
        }

        void OnExitFinished() => _onExitFinished(this);

        void OnDestroy()
        {
            _move?.Kill();
            _follow?.Kill();
            _exit?.Kill();
            _thaw?.Kill();
            Destroy(_outlineMesh);
            Destroy(_exitMaterial);
        }
    }
}

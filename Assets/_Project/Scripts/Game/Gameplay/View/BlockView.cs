using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using ColorBlockJam.Game.Gameplay.Animations;
using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// One block on screen. Its transform sits at the block's centre (so the exit shrink stays in place) and
    /// carries the mesh pieces as children. Owns its tweens; holds no rules. Pooled and reused.
    /// </summary>
    public sealed class BlockView : MonoBehaviour
    {
        [Tooltip("Flat outline drawn over everything while the block is held (ColorBlockJam/Selection Outline).")]
        [SerializeField]
        MeshFilter _outlineFilter;

        [SerializeField]
        MeshRenderer _outlineRenderer;

        readonly List<(PartPool Pool, GameObject Part)> _parts = new();
        readonly OutlineMeshBuilder _outlineBuilder = new();
        readonly List<Vector3> _outlineVertices = new();
        readonly List<int> _outlineTriangles = new();
        Mesh _outlineMesh;
        Vector3 _pivot;
        TweenerCore<Vector3, Vector3, VectorOptions> _move;
        Sequence _exit;
        Action<BlockView> _onExitFinished;
        TweenCallback _exitFinished;

        internal int PartCount => _parts.Count;
        internal bool IsOutlined => _outlineRenderer.enabled;

        /// <summary>Places the block at its start cells; <paramref name="pivot"/> is its centre in cells.</summary>
        internal void Show(Vector3 pivot, float snapDuration, Ease snapEase)
        {
            _pivot = pivot;
            _exit?.Kill();
            transform.localPosition = pivot;
            transform.localScale = Vector3.one;
            _move ??= default(BlockMoveAnimation).Create(transform, snapDuration, snapEase);
            _move.Pause();
        }

        /// <summary>Traces the outline around the block's start cells; hidden until <see cref="SetOutlined"/>.</summary>
        internal void BuildOutline(IReadOnlyList<Vector2Int> cells, float width, float height)
        {
            if (_outlineMesh == null)
            {
                _outlineMesh = new Mesh { name = "BlockOutline" };
                _outlineMesh.MarkDynamic();
                _outlineFilter.sharedMesh = _outlineMesh;
            }

            _outlineBuilder.Build(cells, -_pivot, width, height, _outlineVertices, _outlineTriangles);
            _outlineMesh.Clear();
            _outlineMesh.SetVertices(_outlineVertices);
            _outlineMesh.SetTriangles(_outlineTriangles, 0);
            _outlineMesh.RecalculateBounds();
            SetOutlined(false);
        }

        internal void SetOutlined(bool outlined) => _outlineRenderer.enabled = outlined;

        internal void AddPart(PartPool pool, GameObject part, Vector3 cellPosition, float yaw, Vector3 scale)
        {
            part.transform.SetParent(transform, false);
            part.transform.localPosition = cellPosition - _pivot;
            part.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            part.transform.localScale = scale;
            _parts.Add((pool, part));
        }

        /// <summary>Draws the held block exactly where the finger has it (fractional cells).</summary>
        internal void Follow(Vector2 offset)
        {
            _move.Pause();
            transform.localPosition = _pivot + new Vector3(offset.x, 0f, offset.y);
        }

        /// <summary>Settles the block onto its cell with the snap tween.</summary>
        internal void SnapTo(Vector2Int offset)
        {
            default(BlockMoveAnimation).Retarget(_move, _pivot + new Vector3(offset.x, 0f, offset.y));
        }

        /// <param name="onFinished">Called when the block has vanished, to return it to its pool.</param>
        internal void PlayExit(Vector2Int direction, float distance, float duration, Ease ease,
            Action<BlockView> onFinished)
        {
            _move.Pause();
            SetOutlined(false);
            _onExitFinished = onFinished;
            _exitFinished ??= OnExitFinished;
            // Starts from wherever the finger left it, mid-cell or not.
            Vector3 end = transform.localPosition + new Vector3(direction.x, 0f, direction.y) * distance;
            _exit = default(BlockExitAnimation).Play(
                new BlockExitAnimation.Config(transform, end, duration, ease, _exitFinished));
        }

        /// <summary>Stops its tweens and returns every piece to its pool; the view is then free for reuse.</summary>
        internal void ReleaseParts()
        {
            _exit?.Kill();
            _move?.Pause();
            foreach ((PartPool pool, GameObject part) in _parts)
                pool.Release(part);
            _parts.Clear();
        }

        void OnExitFinished() => _onExitFinished(this);

        void OnDestroy()
        {
            _move?.Kill();
            _exit?.Kill();
            Destroy(_outlineMesh);
        }
    }
}

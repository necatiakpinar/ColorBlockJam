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

        [Tooltip("Child that carries the pieces and the outline; the reject shake moves it, never the root, so "
            + "the shake cannot fight the follow and snap tweens on the root.")]
        [SerializeField]
        Transform _body;

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
        Sequence _reject;
        // This view's own copy of the exit material, made on its first exit and reused by every later one.
        Material _exitMaterial;
        Action<BlockView> _onExitFinished;
        TweenCallback _exitFinished;

        internal int PartCount => _parts.Count;
        internal bool IsOutlined => _outlineRenderer.enabled;

        /// <summary>Places the block at its start cells; <paramref name="pivot"/> is its centre in cells.</summary>
        internal void Show(Vector3 pivot, float snapDuration, Ease snapEase, float followDuration, Ease followEase)
        {
            _pivot = pivot;
            _exit?.Kill();
            transform.localPosition = pivot;
            transform.localScale = Vector3.one;
            _move ??= default(BlockMoveAnimation).Create(transform, snapDuration, snapEase);
            _move.Pause();
            _follow ??= default(BlockMoveAnimation).Create(transform, followDuration, followEase);
            _follow.Pause();
            _reject?.Kill();
            _body.localPosition = Vector3.zero;
        }

        /// <summary>Knocks the block toward a door it cannot use and lets it spring back.</summary>
        internal void PlayReject(Vector2Int direction, float strength, float duration, int vibrato)
        {
            _reject?.Kill();
            var punch = new Vector3(direction.x, 0f, direction.y) * strength;
            _reject = default(BlockRejectAnimation).Play(new BlockRejectAnimation.Config(_body, punch, duration, vibrato));
        }

        /// <summary>Traces the outline around the block's start cells; hidden until <see cref="SetOutlined"/>.</summary>
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

        internal void AddPart(PartPool pool, GameObject part, Renderer mesh, Vector3 cellPosition, float yaw,
            Vector3 scale)
        {
            part.transform.SetParent(_body, false);
            part.transform.localPosition = cellPosition - _pivot;
            part.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            part.transform.localScale = scale;
            _parts.Add((pool, part, mesh));
        }

        /// <summary>
        /// Moves the held block toward where the finger has it (fractional cells) with the short follow tween,
        /// retargeted on every drag, so the block trails the finger slightly and never jumps on a fast flick.
        /// </summary>
        internal void Follow(Vector2 offset)
        {
            _move.Pause();
            default(BlockMoveAnimation).Retarget(_follow, _pivot + new Vector3(offset.x, 0f, offset.y));
        }

        /// <summary>Settles the block onto its cell with the snap tween.</summary>
        internal void SnapTo(Vector2Int offset)
        {
            _follow.Pause();
            default(BlockMoveAnimation).Retarget(_move, _pivot + new Vector3(offset.x, 0f, offset.y));
        }

        /// <summary>
        /// Settles the block, from wherever it is drawn, lined up at the mouth of its door (never backing away
        /// from it), then slides it <paramref name="distance"/> cells past its last cell at
        /// <paramref name="speed"/>, switched to the exit material so the part past
        /// <paramref name="clipPlane"/> (world normal and distance, the board's edge) is cut away.
        /// </summary>
        /// <param name="offset">The cell offset the block left from, lined up with the door.</param>
        /// <param name="onFinished">Called when the block has vanished, to return it to its pool.</param>
        /// <returns>How long the slide through the door takes, after the settle.</returns>
        internal float PlayExit(Vector2Int offset, Vector2Int direction, float distance, float speed,
            float snapDuration, Ease snapEase, Ease ease, Material exitTemplate, Color color, Vector4 clipPlane,
            Action<BlockView> onFinished)
        {
            _move.Pause();
            _follow.Pause();
            _reject?.Kill();
            _body.localPosition = Vector3.zero;
            SetOutlined(false);
            UseExitMaterial(exitTemplate, color, clipPlane);
            _onExitFinished = onFinished;
            _exitFinished ??= OnExitFinished;

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

        /// <summary>Stops its tweens and returns every piece to its pool; the view is then free for reuse.</summary>
        internal void ReleaseParts()
        {
            _exit?.Kill();
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
            _reject?.Kill();
            _exit?.Kill();
            Destroy(_outlineMesh);
            Destroy(_exitMaterial);
        }
    }
}

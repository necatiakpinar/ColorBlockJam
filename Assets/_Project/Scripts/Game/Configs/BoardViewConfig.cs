using System;
using DG.Tweening;
using ColorBlockJam.Game.Levels;
using UnityEngine;
using UnityEngine.Serialization;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Board View Config", fileName = "BoardViewConfig")]
    public sealed class BoardViewConfig : ScriptableObject
    {
        [Serializable]
        public struct ColorEntry
        {
            [SerializeField, FormerlySerializedAs("Color")]
            BlockColor _color;

            [SerializeField, FormerlySerializedAs("Tint")]
            Color _tint;

            public BlockColor Color => _color;
            public Color Tint => _tint;
        }

        [Header("Space")]
        [Tooltip("World size of one board cell. 1 matches the 0.5 model import scale (art-pipeline.md §3).")]
        [SerializeField, Min(0.01f)]
        float _cellSize = 1f;

        [Tooltip("Height of the invisible collider that catches board touches.")]
        [SerializeField, Min(0.01f)]
        float _inputHitAreaHeight = 0.1f;

        [Tooltip("Thickness of the wall ring around the board, in cells; matches the wall meshes.")]
        [SerializeField, Min(0f)]
        float _wallThickness = 0.5f;

        [Tooltip("How far the grid base sits below the floor tiles, in cells, so the tiles never flicker on it.")]
        [SerializeField, Min(0.001f)]
        float _baseDepth = 0.01f;

        [Header("Board parts")]
        [SerializeField]
        GameObject _boardBase;

        [SerializeField]
        GameObject _floorTile;

        [SerializeField]
        GameObject _wallSegment;

        [SerializeField]
        GameObject _wallCorner;

        [SerializeField]
        GameObject _doorSegment;

        [Header("Block parts")]
        [SerializeField]
        GameObject _blockCenter;

        [SerializeField]
        GameObject _blockEdge;

        [SerializeField]
        GameObject _blockOuterCorner;

        [SerializeField]
        GameObject _blockInnerCorner;

        [SerializeField]
        Material _blockMaterial;

        [SerializeField]
        Material _doorMaterial;

        [SerializeField]
        ColorEntry[] _palette = Array.Empty<ColorEntry>();

        [Header("Camera")]
        [SerializeField]
        FramingMargins _framing = new(0.06f, 0.22f, 0.16f);

        [Tooltip("Height above the floor that must stay in frame: the tallest part (block studs), in cells.")]
        [SerializeField, Min(0f)]
        float _framedHeight = 0.5f;

        [Header("Controls")]
        [Tooltip("A held block pushed into a one-cell gap slips sideways into it when it is at most this many cells off the cell line.")]
        [SerializeField, Range(0f, 0.5f)]
        float _cornerTolerance = 0.4f;

        [Tooltip("How far a held block may lean toward the finger past what it hits, in cells; drawn only, it snaps back on release.")]
        [SerializeField, Range(0f, 0.3f)]
        float _holdGive = 0.12f;

        [Header("Selection outline")]
        [Tooltip("Width of the outline drawn around a held block, in cells.")]
        [SerializeField, Min(0.005f)]
        float _outlineWidth = 0.06f;

        [Tooltip("Height of the outline above the floor, in cells: just over the block's studs.")]
        [SerializeField, Min(0f)]
        float _outlineHeight = 0.5f;

        [Header("Motion")]
        [Tooltip("How long a released block takes to settle on its cell.")]
        [SerializeField, Min(0.01f), FormerlySerializedAs("_moveDuration")]
        float _snapDuration = 0.12f;

        [SerializeField, FormerlySerializedAs("_moveEase")]
        Ease _snapEase = Ease.OutBack;

        [Tooltip("How far an exiting block slides past its wall, in cells, while it shrinks away.")]
        [SerializeField, Min(0f)]
        float _exitDistance = 1.5f;

        [SerializeField, Min(0.01f)]
        float _exitDuration = 0.3f;

        [SerializeField]
        Ease _exitEase = Ease.InQuad;

        public float CellSize => _cellSize;
        public float InputHitAreaHeight => _inputHitAreaHeight;
        public float WallThickness => _wallThickness;
        public float BaseDepth => _baseDepth;
        public GameObject BoardBase => _boardBase;
        public GameObject FloorTile => _floorTile;
        public GameObject WallSegment => _wallSegment;
        public GameObject WallCorner => _wallCorner;
        public GameObject DoorSegment => _doorSegment;
        public GameObject BlockCenter => _blockCenter;
        public GameObject BlockEdge => _blockEdge;
        public GameObject BlockOuterCorner => _blockOuterCorner;
        public GameObject BlockInnerCorner => _blockInnerCorner;
        public Material BlockMaterial => _blockMaterial;
        public Material DoorMaterial => _doorMaterial;
        public FramingMargins Framing => _framing;
        public float FramedHeight => _framedHeight;
        public float CornerTolerance => _cornerTolerance;
        public float HoldGive => _holdGive;
        public float OutlineWidth => _outlineWidth;
        public float OutlineHeight => _outlineHeight;
        public float SnapDuration => _snapDuration;
        public Ease SnapEase => _snapEase;
        public float ExitDistance => _exitDistance;
        public float ExitDuration => _exitDuration;
        public Ease ExitEase => _exitEase;

        public Color TintOf(BlockColor color)
        {
            foreach (ColorEntry entry in _palette)
                if (entry.Color == color)
                    return entry.Tint;
            throw new ArgumentOutOfRangeException(nameof(color), color, $"{name} has no palette entry for {color}.");
        }
    }
}

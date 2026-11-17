using System;
using DG.Tweening;
using ColorBlockJam.Game.Gameplay.View;
using ColorBlockJam.Game.Gameplay.View.Blockers;
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

        [Tooltip("Arrow on top of a door, one per door cell, pointing out of the board (its tip toward local -z).")]
        [SerializeField]
        GameObject _doorArrow;

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

        [Tooltip("A press that misses every block still picks the nearest one when it is at most this many "
            + "cells away, so a finger need not hit a block exactly.")]
        [SerializeField, Range(0f, 0.5f)]
        float _touchTolerance = 0.35f;

        [Tooltip("How far a held block lined up with its own door must be pushed into it, in cells, to leave.")]
        [SerializeField, Range(0.01f, 0.5f)]
        float _exitPush = 0.15f;

        [Header("Selection outline")]
        [Tooltip("Width of the outline band drawn around a held block, in cells.")]
        [SerializeField, Min(0.005f)]
        float _outlineWidth = 0.06f;

        [Tooltip("Height of the outline along the sides the camera sees, in cells: the block's bottom edge, "
            + "where it meets the floor.")]
        [SerializeField, Min(0f)]
        float _outlineBaseHeight = 0.01f;

        [Tooltip("Height of the rim of a block's top face, in cells (ModularBlock_* parts: 0.33). The outline "
            + "runs there along the sides turned away from the camera, so it traces the block's silhouette.")]
        [SerializeField, Min(0f)]
        float _blockTopHeight = 0.33f;

        [Tooltip("Space between the block's side and the outline band, in cells.")]
        [SerializeField, Min(0f)]
        float _outlineGap = 0.03f;

        [Tooltip("Corner radius of the block models, in cells (ModularBlock_* parts: 0.27); the outline "
            + "rounds its corners to match.")]
        [SerializeField, Min(0f)]
        float _blockCornerRadius = 0.27f;

        [Tooltip("Straight pieces per rounded outline corner.")]
        [SerializeField, Range(1, 16)]
        int _outlineCornerSegments = 6;

        [Header("Motion")]
        [Tooltip("How long a held block takes to catch up with the finger; retargeted on every drag, so the "
            + "block trails the finger by about this much and glides instead of jumping on a fast flick.")]
        [SerializeField, Min(0.01f)]
        float _followDuration = 0.12f;

        [SerializeField]
        Ease _followEase = Ease.OutQuad;

        [Tooltip("How long a released block takes to settle on its cell.")]
        [SerializeField, Min(0.01f), FormerlySerializedAs("_moveDuration")]
        float _snapDuration = 0.12f;

        [SerializeField, FormerlySerializedAs("_moveEase")]
        Ease _snapEase = Ease.OutBack;

        [Tooltip("How long an exiting block takes to settle at the mouth of its door before it slides in.")]
        [SerializeField, Min(0f)]
        float _exitSnapDuration = 0.08f;

        [SerializeField]
        Ease _exitSnapEase = Ease.OutQuad;

        [Tooltip("How fast an exiting block slides into its door, in cells per second.")]
        [SerializeField, Min(0.1f)]
        float _exitSpeed = 6f;

        [Tooltip("How far past its own length an exiting block slides, in cells, so it is fully gone.")]
        [SerializeField, Min(0f)]
        float _exitClearance = 0.1f;

        [SerializeField]
        Ease _exitEase = Ease.Linear;

        [Tooltip("How far past the board's edge an exiting block is cut, in cells: a little inside the door, so "
            + "the block covers the floor's edge that the sinking door uncovers.")]
        [SerializeField, Min(0f)]
        float _exitCutOffset = 0.15f;

        [Tooltip("Material an exiting block switches to (ColorBlockJam/Block Exit): lit like M_Block, cut at "
            + "the board's edge.")]
        [SerializeField]
        Material _blockExitMaterial;

        [Header("Door gate")]
        [Tooltip("How far a door sinks into the floor to let a block through, in cells.")]
        [SerializeField, Min(0f)]
        float _doorGateDepth = 0.35f;

        [SerializeField, Min(0.01f)]
        float _doorGateCloseDuration = 0.15f;

        [Header("Exit debris")]
        [Tooltip("The spray of small cubes out of a door while a block passes it (BlockExitBurst.prefab).")]
        [SerializeField]
        ExitBurstView _exitBurst;

        [Tooltip("Cubes thrown at once when a block reaches its door, for each cell of its width.")]
        [SerializeField, Min(0f)]
        float _exitDebrisBurst = 24f;

        [Tooltip("Cubes sprayed per second for each cell of the block's width across the door.")]
        [SerializeField, Min(0f)]
        float _exitDebrisRate = 70f;

        [Tooltip("Blocks leaving through the same door within this many seconds share one spray: the later "
            + "ones add their cubes to it instead of starting another particle system.")]
        [SerializeField, Min(0f)]
        float _exitBurstCoalesceWindow = 0.15f;

        [Tooltip("Height of the spray's source above the floor, in cells (about the block's own height).")]
        [SerializeField, Min(0f)]
        float _exitDebrisHeight = 0.3f;

        [Header("Ice blocker")]
        [Tooltip("Material of a frozen block (M_Ice: the ice texture), tinted with its colour.")]
        [SerializeField]
        Material _iceMaterial;

        [Tooltip("How far a frozen block's colour is pulled toward white; 1 shows the ice texture as it is.")]
        [SerializeField, Range(0f, 1f)]
        float _iceWhiten = 1f;

        [Tooltip("A thawing block swells to this scale and back...")]
        [SerializeField, Min(1f)]
        float _thawSwell = 1.12f;

        [Tooltip("...in this many seconds.")]
        [SerializeField, Min(0.01f)]
        float _thawDuration = 0.3f;

        [Tooltip("Ice shards a thaw throws, per cell.")]
        [SerializeField, Min(0f)]
        float _thawDebrisPerCell = 14f;

        [Tooltip("The arrow an arrow block shows on top, along its axis (Blockers/ArrowBlocker prefab).")]
        [SerializeField]
        ArrowBlockerView _arrowBlocker;

        [Tooltip("The count a frozen block shows on top (Blockers/IceBlocker prefab).")]
        [SerializeField]
        IceBlockerView _iceBlocker;

        [Tooltip("The shards a thaw throws: the ice texture on tumbling cubes (Blockers/IceShatter prefab).")]
        [SerializeField]
        ExitBurstView _iceShatter;

        public Material IceMaterial => _iceMaterial;
        public float IceWhiten => _iceWhiten;
        public float ThawSwell => _thawSwell;
        public float ThawDuration => _thawDuration;
        public float ThawDebrisPerCell => _thawDebrisPerCell;
        public ExitBurstView IceShatter => _iceShatter;
        public IceBlockerView IceBlocker => _iceBlocker;
        public ArrowBlockerView ArrowBlocker => _arrowBlocker;

        [Header("Booster removal")]
        [Tooltip("A smashed block (hammer, or a rocket's last cell) first swells to this scale...")]
        [SerializeField, Min(1f)]
        float _smashSwell = 1.2f;

        [Tooltip("...then shrinks away; seconds for both.")]
        [SerializeField, Min(0.01f)]
        float _smashDuration = 0.3f;

        [Tooltip("Cubes a smash or a blasted cell throws, per cell.")]
        [SerializeField, Min(0f)]
        float _smashDebrisPerCell = 10f;

        [Tooltip("Seconds a vacuumed block takes to lift, spin and shrink away.")]
        [SerializeField, Min(0.01f)]
        float _vacuumDuration = 0.45f;

        [Tooltip("Seconds between the vacuumed blocks, in board order.")]
        [SerializeField, Min(0f)]
        float _vacuumStagger = 0.08f;

        [Tooltip("Cells a vacuumed block rises while it goes.")]
        [SerializeField, Min(0f)]
        float _vacuumLift = 2f;

        public float SmashSwell => _smashSwell;
        public float SmashDuration => _smashDuration;
        public float SmashDebrisPerCell => _smashDebrisPerCell;
        public float VacuumDuration => _vacuumDuration;
        public float VacuumStagger => _vacuumStagger;
        public float VacuumLift => _vacuumLift;

        public float CellSize => _cellSize;
        public float InputHitAreaHeight => _inputHitAreaHeight;
        public float WallThickness => _wallThickness;
        public float BaseDepth => _baseDepth;
        public GameObject BoardBase => _boardBase;
        public GameObject FloorTile => _floorTile;
        public GameObject WallSegment => _wallSegment;
        public GameObject WallCorner => _wallCorner;
        public GameObject DoorSegment => _doorSegment;
        public GameObject DoorArrow => _doorArrow;
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
        public float ExitPush => _exitPush;
        public float TouchTolerance => _touchTolerance;
        public float DoorGateDepth => _doorGateDepth;
        public float DoorGateCloseDuration => _doorGateCloseDuration;
        public float ExitBurstCoalesceWindow => _exitBurstCoalesceWindow;
        public float OutlineWidth => _outlineWidth;
        public float OutlineBaseHeight => _outlineBaseHeight;
        public float BlockTopHeight => _blockTopHeight;
        public float OutlineGap => _outlineGap;
        public float BlockCornerRadius => _blockCornerRadius;
        public int OutlineCornerSegments => _outlineCornerSegments;
        public float FollowDuration => _followDuration;
        public Ease FollowEase => _followEase;
        public float SnapDuration => _snapDuration;
        public Ease SnapEase => _snapEase;
        public float ExitSnapDuration => _exitSnapDuration;
        public Ease ExitSnapEase => _exitSnapEase;
        public float ExitSpeed => _exitSpeed;
        public float ExitClearance => _exitClearance;
        public Ease ExitEase => _exitEase;
        public float ExitCutOffset => _exitCutOffset;
        public Material BlockExitMaterial => _blockExitMaterial;
        public ExitBurstView ExitBurst => _exitBurst;
        public float ExitDebrisRate => _exitDebrisRate;
        public float ExitDebrisBurst => _exitDebrisBurst;
        public float ExitDebrisHeight => _exitDebrisHeight;

        public Color TintOf(BlockColor color)
        {
            foreach (ColorEntry entry in _palette)
                if (entry.Color == color)
                    return entry.Tint;
            throw new ArgumentOutOfRangeException(nameof(color), color, $"{name} has no palette entry for {color}.");
        }
    }
}

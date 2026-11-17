using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
    /// <summary>
    /// One mesh piece of a block, in cell units relative to the block layer's origin. A negative scale axis
    /// mirrors the authored piece into another corner; Unity flips the face winding to match.
    /// </summary>
    public readonly struct BlockPiece
    {
        public readonly BlockPieceKind Kind;
        public readonly Vector3 Position;
        public readonly float Yaw;
        public readonly Vector3 Scale;

        public BlockPiece(BlockPieceKind kind, Vector3 position, float yaw, Vector3 scale)
        {
            Kind = kind;
            Position = position;
            Yaw = yaw;
            Scale = scale;
        }
    }
}

using UnityEngine;

namespace ColorBlockJam.Game.Gameplay.View
{
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

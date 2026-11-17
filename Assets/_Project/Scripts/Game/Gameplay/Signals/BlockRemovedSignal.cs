using ColorBlockJam.Game.Gameplay.Board;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockRemovedSignal
    {
        public readonly int BlockId;
        public readonly BlockRemovalKind Kind;
        public readonly int Order;

        public BlockRemovedSignal(int blockId, BlockRemovalKind kind, int order)
        {
            BlockId = blockId;
            Kind = kind;
            Order = order;
        }
    }
}

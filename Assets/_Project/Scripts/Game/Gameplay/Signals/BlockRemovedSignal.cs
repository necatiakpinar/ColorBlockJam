using ColorBlockJam.Game.Gameplay.Board;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// A booster took a whole block off the board (DA-36). <see cref="Order"/> numbers the blocks one use removes
    /// together (the vacuum), so the view can stagger them.
    /// </summary>
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

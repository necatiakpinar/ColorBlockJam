namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>
    /// The rocket split a block apart and one piece became a block of its own (DA-36): <see cref="BlockId"/> is the new
    /// block, <see cref="FromBlockId"/> the block it broke off.
    /// </summary>
    public readonly struct BlockSplitOffSignal
    {
        public readonly int BlockId;
        public readonly int FromBlockId;

        public BlockSplitOffSignal(int blockId, int fromBlockId)
        {
            BlockId = blockId;
            FromBlockId = fromBlockId;
        }
    }
}

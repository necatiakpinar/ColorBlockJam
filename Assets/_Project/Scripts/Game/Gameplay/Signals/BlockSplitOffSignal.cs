namespace ColorBlockJam.Game.Gameplay.Signals
{
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

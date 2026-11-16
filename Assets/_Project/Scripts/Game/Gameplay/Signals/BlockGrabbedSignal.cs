namespace RollicCase.Game.Gameplay.Signals
{
    /// <summary>A finger picked up a block; it stays held until it is dropped or exits.</summary>
    public readonly struct BlockGrabbedSignal
    {
        public readonly int BlockId;

        public BlockGrabbedSignal(int blockId)
        {
            BlockId = blockId;
        }
    }
}

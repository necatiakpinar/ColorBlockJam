namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockGrabbedSignal
    {
        public readonly int BlockId;

        public BlockGrabbedSignal(int blockId)
        {
            BlockId = blockId;
        }
    }
}

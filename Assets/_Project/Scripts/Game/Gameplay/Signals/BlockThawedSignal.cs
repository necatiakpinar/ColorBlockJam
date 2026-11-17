namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BlockThawedSignal
    {
        public readonly int BlockId;

        public BlockThawedSignal(int blockId) => BlockId = blockId;
    }
}

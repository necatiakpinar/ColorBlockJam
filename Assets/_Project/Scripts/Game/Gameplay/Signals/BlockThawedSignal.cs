namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>A frozen block's ice broke (its count reached 0, or the hammer hit it): it can move now (T-34).</summary>
    public readonly struct BlockThawedSignal
    {
        public readonly int BlockId;

        public BlockThawedSignal(int blockId) => BlockId = blockId;
    }
}

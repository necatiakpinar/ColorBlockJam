namespace ColorBlockJam.Game.Boosters
{
    /// <summary>The player's stock of one booster changed and is already saved.</summary>
    public readonly struct BoosterCountChangedSignal
    {
        public readonly BoosterType Type;
        public readonly int Count;

        public BoosterCountChangedSignal(BoosterType type, int count)
        {
            Type = type;
            Count = count;
        }
    }
}

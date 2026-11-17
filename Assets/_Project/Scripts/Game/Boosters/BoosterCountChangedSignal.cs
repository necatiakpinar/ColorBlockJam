namespace ColorBlockJam.Game.Boosters
{
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

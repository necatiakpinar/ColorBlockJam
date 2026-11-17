using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BoosterUsedSignal
    {
        public readonly BoosterType Type;

        public BoosterUsedSignal(BoosterType type) => Type = type;
    }
}

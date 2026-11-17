using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    public readonly struct BoosterTargetingChangedSignal
    {
        public readonly BoosterType Type;
        public readonly bool Active;

        public BoosterTargetingChangedSignal(BoosterType type, bool active)
        {
            Type = type;
            Active = active;
        }
    }
}

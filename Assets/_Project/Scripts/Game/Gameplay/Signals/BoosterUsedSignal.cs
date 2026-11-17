using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>A booster was spent and has done its work on the board or the timer.</summary>
    public readonly struct BoosterUsedSignal
    {
        public readonly BoosterType Type;

        public BoosterUsedSignal(BoosterType type) => Type = type;
    }
}

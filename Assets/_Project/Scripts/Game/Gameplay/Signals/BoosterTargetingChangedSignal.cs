using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Signals
{
    /// <summary>A booster started (<see cref="Active"/>) or stopped waiting for the player to tap a block.</summary>
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

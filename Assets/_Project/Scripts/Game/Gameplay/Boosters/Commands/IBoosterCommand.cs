using ColorBlockJam.Game.Boosters;

namespace ColorBlockJam.Game.Gameplay.Boosters.Commands
{
    /// <summary>
    /// What one booster does to the level (boosters.md §2). <see cref="BoosterService"/> owns the stock, the pause
    /// and the targeting; a command owns only its effect, so a new booster is a new command, never a new branch.
    /// </summary>
    public interface IBoosterCommand
    {
        BoosterType Type { get; }

        /// <summary>The booster waits for the player to tap a block before it runs.</summary>
        bool NeedsTarget { get; }

        /// <summary>The level can take this booster now (the freeze refuses while one is running).</summary>
        bool CanExecute { get; }

        /// <summary>Applies the effect; <paramref name="target"/> is meaningful only when <see cref="NeedsTarget"/>.</summary>
        void Execute(in BoosterTarget target);
    }
}

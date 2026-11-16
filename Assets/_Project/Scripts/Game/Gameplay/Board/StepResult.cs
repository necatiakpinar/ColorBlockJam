namespace RollicCase.Game.Gameplay.Board
{
    /// <summary>What happened when a block was pushed one cell.</summary>
    public enum StepResult
    {
        /// <summary>The block moved one cell.</summary>
        Moved = 1000,

        /// <summary>A wall, a wrong door or another block stopped it; nothing changed.</summary>
        Blocked = 2000,

        /// <summary>The block left through a door and is off the board.</summary>
        Exited = 3000,
    }
}

namespace ColorBlockJam.Game.Gameplay.Board
{
    /// <summary>How a booster took a block off the board (DA-36); the view picks its animation by it.</summary>
    public enum BlockRemovalKind
    {
        /// <summary>Hammer: the whole block breaks.</summary>
        Smashed = 1000,

        /// <summary>Rocket: its last cell was blasted.</summary>
        Blasted = 2000,

        /// <summary>Vacuum: every block of its colour is sucked up.</summary>
        Vacuumed = 3000,
    }
}

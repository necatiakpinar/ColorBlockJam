namespace ColorBlockJam.Game.Boosters
{
    /// <summary>The four boosters of the gameplay booster bar, left to right (DA-36).</summary>
    public enum BoosterType
    {
        /// <summary>Stops the level timer for a while.</summary>
        Freeze = 1000,

        /// <summary>Blasts one cell out of a block.</summary>
        Rocket = 2000,

        /// <summary>Breaks one whole block.</summary>
        Hammer = 3000,

        /// <summary>Removes every block of one colour.</summary>
        Vacuum = 4000,
    }
}

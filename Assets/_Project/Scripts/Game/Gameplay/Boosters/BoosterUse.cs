namespace ColorBlockJam.Game.Gameplay.Boosters
{
    /// <summary>What <see cref="BoosterService.Activate"/> did.</summary>
    public enum BoosterUse
    {
        /// <summary>It worked at once (the freeze).</summary>
        Used = 1000,

        /// <summary>It waits for the player to tap a block.</summary>
        Targeting = 2000,

        /// <summary>The player has none left: offer one for coins.</summary>
        NeedsPurchase = 3000,

        /// <summary>The level cannot take it now; nothing changed.</summary>
        Refused = 4000,
    }
}

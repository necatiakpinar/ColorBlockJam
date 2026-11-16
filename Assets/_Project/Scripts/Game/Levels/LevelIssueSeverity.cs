namespace RollicCase.Game.Levels
{
    public enum LevelIssueSeverity
    {
        /// <summary>The level cannot be played as authored; the level editor refuses to save it.</summary>
        Error = 1000,

        /// <summary>Playable but probably a mistake; the level editor asks the designer to confirm.</summary>
        Warning = 2000,
    }
}

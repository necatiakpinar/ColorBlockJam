namespace ColorBlockJam.Game.Levels
{
    public readonly struct LevelIssue
    {
        public readonly LevelIssueSeverity Severity;
        public readonly string Message;

        public LevelIssue(LevelIssueSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public override string ToString() => $"{Severity}: {Message}";
    }
}

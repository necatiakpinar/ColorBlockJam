namespace ColorBlockJam.Core.Lifecycle
{
    /// <summary>The app went to the background (<see cref="IsPaused"/> true) or came back (false).</summary>
    public readonly struct ApplicationPausedSignal
    {
        public readonly bool IsPaused;

        public ApplicationPausedSignal(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}

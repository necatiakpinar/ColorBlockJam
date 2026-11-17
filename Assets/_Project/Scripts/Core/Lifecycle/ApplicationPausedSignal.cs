namespace ColorBlockJam.Core.Lifecycle
{
    public readonly struct ApplicationPausedSignal
    {
        public readonly bool IsPaused;

        public ApplicationPausedSignal(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}

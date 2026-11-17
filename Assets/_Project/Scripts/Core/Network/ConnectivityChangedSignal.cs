namespace ColorBlockJam.Core.Network
{
    /// <summary>The game went online or offline. Fired on the first probe's answer and on every change after.</summary>
    public readonly struct ConnectivityChangedSignal
    {
        public readonly bool IsOnline;

        public ConnectivityChangedSignal(bool isOnline)
        {
            IsOnline = isOnline;
        }
    }
}

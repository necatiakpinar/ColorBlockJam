namespace ColorBlockJam.Core.Network
{
    public readonly struct ConnectivityChangedSignal
    {
        public readonly bool IsOnline;

        public ConnectivityChangedSignal(bool isOnline)
        {
            IsOnline = isOnline;
        }
    }
}

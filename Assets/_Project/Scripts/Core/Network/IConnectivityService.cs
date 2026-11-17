using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Network
{
    /// <summary>
    /// Whether the game can reach the internet, decided by real requests (network.md). Changes are announced
    /// with <see cref="ConnectivityChangedSignal"/>.
    /// </summary>
    public interface IConnectivityService
    {
        /// <summary>The last probe succeeded. False until the first probe answers.</summary>
        bool IsOnline { get; }

        /// <summary>The first probe has answered, so <see cref="IsOnline"/> means something.</summary>
        bool HasProbed { get; }

        /// <summary>Probes now instead of at the next interval (Retry); completes with the result.</summary>
        UniTask<bool> ProbeNowAsync();
    }
}

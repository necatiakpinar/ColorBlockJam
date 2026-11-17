using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Network
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }

        bool HasProbed { get; }

        UniTask<bool> ProbeNowAsync();
    }
}

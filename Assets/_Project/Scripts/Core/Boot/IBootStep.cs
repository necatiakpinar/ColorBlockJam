using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Boot
{
    public interface IBootStep
    {
        float Weight { get; }

        UniTask ExecuteAsync(CancellationToken cancellationToken);
    }
}

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.Boot
{
    /// <summary>One unit of work the app must finish before it leaves the splash.</summary>
    public interface IBootStep
    {
        /// <summary>Relative share of the loading bar this step owns. Must be greater than zero.</summary>
        float Weight { get; }

        UniTask ExecuteAsync(CancellationToken cancellationToken);
    }
}

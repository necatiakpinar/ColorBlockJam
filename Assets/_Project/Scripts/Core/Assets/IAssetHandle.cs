using UnityEngine;

namespace ColorBlockJam.Core.Assets
{
    public interface IAssetHandle
    {
        Object Asset { get; }

        void Release();
    }
}

using System;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    /// <summary>The saved wallet. A new player starts with no coins (DA-7).</summary>
    [Serializable]
    public sealed class WalletData
    {
        [SerializeField]
        int _coins;

        public int Coins
        {
            get => _coins;
            set => _coins = value;
        }
    }
}

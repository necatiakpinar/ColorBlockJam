using System;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
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

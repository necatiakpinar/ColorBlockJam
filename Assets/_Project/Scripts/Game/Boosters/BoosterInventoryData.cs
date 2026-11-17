using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Boosters
{
    /// <summary>The saved booster stock: one entry per booster the player has ever had (persistence.md).</summary>
    [Serializable]
    public sealed class BoosterInventoryData
    {
        [SerializeField]
        List<BoosterStock> _stocks = new();

        public List<BoosterStock> Stocks => _stocks;
    }

    /// <summary>How many of one booster the player owns.</summary>
    [Serializable]
    public sealed class BoosterStock
    {
        [SerializeField]
        BoosterType _type;

        [SerializeField]
        int _count;

        public BoosterStock(BoosterType type, int count)
        {
            _type = type;
            _count = count;
        }

        public BoosterType Type => _type;

        public int Count
        {
            get => _count;
            set => _count = value;
        }
    }
}

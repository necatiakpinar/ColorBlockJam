using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Boosters
{
    [Serializable]
    public sealed class BoosterInventoryData
    {
        [SerializeField]
        List<BoosterStock> _stocks = new();

        public List<BoosterStock> Stocks => _stocks;
    }

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

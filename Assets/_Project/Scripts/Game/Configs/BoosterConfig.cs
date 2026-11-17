using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Boosters;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Booster Config", fileName = "BoosterConfig")]
    public sealed class BoosterConfig : ScriptableObject
    {
        [Tooltip("One entry per booster; a booster with no entry is a setup bug and throws when used.")]
        [SerializeField]
        BoosterSettings[] _boosters =
        {
            new(BoosterType.Freeze, 1, 40),
            new(BoosterType.Rocket, 1, 60),
            new(BoosterType.Hammer, 1, 80),
            new(BoosterType.Vacuum, 1, 120),
        };

        [Tooltip("Seconds the freeze booster holds the level timer still.")]
        [SerializeField, Min(1f)]
        float _freezeSeconds = 10f;

        public IReadOnlyList<BoosterSettings> Boosters => _boosters;
        public float FreezeSeconds => _freezeSeconds;

        public BoosterSettings For(BoosterType type)
        {
            foreach (BoosterSettings settings in _boosters)
                if (settings.Type == type)
                    return settings;
            throw new InvalidOperationException($"{name} has no entry for the {type} booster.");
        }
    }

    [Serializable]
    public sealed class BoosterSettings
    {
        [SerializeField]
        BoosterType _type;

        [Tooltip("How many a new player owns.")]
        [SerializeField, Min(0)]
        int _startingCount;

        [Tooltip("Coins one more costs.")]
        [SerializeField, Min(1)]
        int _price;

        [Tooltip("Name on the purchase popup and the hint banner.")]
        [SerializeField]
        string _title;

        [Tooltip("What to tap, shown in the hint banner while choosing a target.")]
        [SerializeField]
        string _hint;

        [Tooltip("What it does, shown on the purchase popup.")]
        [SerializeField, TextArea]
        string _description;

        [SerializeField]
        Sprite _icon;

        public BoosterSettings(BoosterType type, int startingCount, int price)
        {
            _type = type;
            _startingCount = startingCount;
            _price = price;
        }

        public BoosterType Type => _type;
        public int StartingCount => _startingCount;
        public int Price => _price;
        public string Title => _title;
        public string Hint => _hint;
        public string Description => _description;
        public Sprite Icon => _icon;
    }
}

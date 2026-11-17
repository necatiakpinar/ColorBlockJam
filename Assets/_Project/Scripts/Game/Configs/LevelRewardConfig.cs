using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
    /// <summary>Coins a win pays for each level difficulty (economy.md, DA-7). Read through <c>LevelRewardProvider</c>.</summary>
    [CreateAssetMenu(menuName = "ColorBlockJam/Configs/Level Reward Config", fileName = "LevelRewardConfig")]
    public sealed class LevelRewardConfig : ScriptableObject
    {
        [Tooltip("One entry per difficulty; a level whose difficulty has no entry cannot pay out.")]
        [SerializeField]
        DifficultyReward[] _rewards =
        {
            new(LevelDifficulty.Normal, 10),
            new(LevelDifficulty.Hard, 25),
            new(LevelDifficulty.VeryHard, 50),
        };

        public IReadOnlyList<DifficultyReward> Rewards => _rewards;
    }

    /// <summary>The coins a win at one difficulty pays.</summary>
    [Serializable]
    public struct DifficultyReward
    {
        [SerializeField]
        LevelDifficulty _difficulty;

        [SerializeField, Min(1)]
        int _coins;

        public DifficultyReward(LevelDifficulty difficulty, int coins)
        {
            _difficulty = difficulty;
            _coins = coins;
        }

        public LevelDifficulty Difficulty => _difficulty;
        public int Coins => _coins;
    }
}

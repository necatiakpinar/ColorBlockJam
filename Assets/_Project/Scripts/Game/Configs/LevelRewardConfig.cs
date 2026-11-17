using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using UnityEngine;

namespace ColorBlockJam.Game.Configs
{
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

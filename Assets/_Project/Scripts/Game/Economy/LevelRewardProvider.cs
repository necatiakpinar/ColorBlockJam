using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;

namespace ColorBlockJam.Game.Economy
{
    /// <summary>Maps a level to the coins its win pays, by its difficulty (economy.md). Reads <see cref="LevelRewardConfig"/>.</summary>
    public sealed class LevelRewardProvider
    {
        readonly LevelRewardConfig _config;

        public LevelRewardProvider(LevelRewardConfig config)
        {
            _config = config;
        }

        public int CoinsFor(LevelData level) => CoinsFor(level.Difficulty);

        public int CoinsFor(LevelDifficulty difficulty)
        {
            IReadOnlyList<DifficultyReward> rewards = _config.Rewards;
            for (int i = 0; i < rewards.Count; i++)
                if (rewards[i].Difficulty == difficulty)
                    return rewards[i].Coins;
            throw new InvalidOperationException($"{nameof(LevelRewardConfig)} has no reward for {difficulty} levels.");
        }
    }
}

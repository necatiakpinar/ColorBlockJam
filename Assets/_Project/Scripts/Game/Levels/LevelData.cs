using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// One level in the single format the editor writes and the game reads (DA-13). Plain data in cell
    /// coordinates: cell (0, 0) is bottom-left. Rules live in the board model, checks in
    /// <see cref="LevelDefinitionValidator"/>.
    /// </summary>
    [Serializable]
    public sealed class LevelData
    {
        [SerializeField]
        int _width;

        [SerializeField]
        int _height;

        [SerializeField]
        int _timerSeconds;

        [Tooltip("Sets the win reward (LevelRewardConfig).")]
        [SerializeField]
        LevelDifficulty _difficulty = LevelDifficulty.Normal;

        [SerializeField]
        List<BlockData> _blocks;

        [SerializeField]
        List<DoorData> _doors;

        // Unity calls this when it creates the object for an asset that has no data yet.
        LevelData()
        {
            _blocks = new List<BlockData>();
            _doors = new List<DoorData>();
        }

        public LevelData(int width, int height, int timerSeconds, IEnumerable<BlockData> blocks,
            IEnumerable<DoorData> doors, LevelDifficulty difficulty = LevelDifficulty.Normal)
        {
            _width = width;
            _height = height;
            _timerSeconds = timerSeconds;
            _difficulty = difficulty;
            _blocks = new List<BlockData>(blocks);
            _doors = new List<DoorData>(doors);
        }

        public int Width => _width;
        public int Height => _height;
        public int TimerSeconds => _timerSeconds;
        public LevelDifficulty Difficulty => _difficulty;
        public IReadOnlyList<BlockData> Blocks => _blocks;
        public IReadOnlyList<DoorData> Doors => _doors;

        /// <summary>Number of board cells along a wall: the width for Bottom/Top, the height for Left/Right.</summary>
        public int WallLength(WallSide side) => side is WallSide.Bottom or WallSide.Top ? _width : _height;
    }
}

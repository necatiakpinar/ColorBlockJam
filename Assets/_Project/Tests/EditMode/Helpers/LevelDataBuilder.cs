using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Tests.EditMode.Helpers
{
    public sealed class LevelDataBuilder
    {
        readonly int _width;
        readonly int _height;
        readonly List<BlockData> _blocks = new();
        readonly List<DoorData> _doors = new();
        int _timerSeconds = 60;
        LevelDifficulty _difficulty = LevelDifficulty.Normal;

        public LevelDataBuilder(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public LevelDataBuilder Timer(int seconds)
        {
            _timerSeconds = seconds;
            return this;
        }

        public LevelDataBuilder Difficulty(LevelDifficulty difficulty)
        {
            _difficulty = difficulty;
            return this;
        }

        public LevelDataBuilder Block(BlockColor color, params int[] cells)
        {
            var points = new List<Vector2Int>();
            for (int i = 0; i < cells.Length; i += 2)
                points.Add(new Vector2Int(cells[i], cells[i + 1]));
            _blocks.Add(new BlockData(color, points));
            return this;
        }

        public LevelDataBuilder FrozenBlock(BlockColor color, int ice, params int[] cells)
        {
            var points = new List<Vector2Int>();
            for (int i = 0; i < cells.Length; i += 2)
                points.Add(new Vector2Int(cells[i], cells[i + 1]));
            _blocks.Add(new BlockData(color, points, new BlockerSet(new IceBlockerData(ice))));
            return this;
        }

        public LevelDataBuilder Blocked(BlockColor color, BlockerSet blockers, params int[] cells)
        {
            var points = new List<Vector2Int>();
            for (int i = 0; i < cells.Length; i += 2)
                points.Add(new Vector2Int(cells[i], cells[i + 1]));
            _blocks.Add(new BlockData(color, points, blockers));
            return this;
        }

        public LevelDataBuilder Door(WallSide side, int start, int length, BlockColor color)
        {
            _doors.Add(new DoorData(side, start, length, color));
            return this;
        }

        public LevelData Build() => new(_width, _height, _timerSeconds, _blocks, _doors, _difficulty);
    }
}

using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// Checks a level against the structural rules in level-data.md §2 and reports every broken one.
    /// Editor-time code: clarity over allocation.
    /// </summary>
    public sealed class LevelDefinitionValidator
    {
        static readonly Vector2Int[] Neighbours = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        readonly LevelRulesConfig _rules;

        public LevelDefinitionValidator(LevelRulesConfig rules)
        {
            _rules = rules;
        }

        public IReadOnlyList<LevelIssue> Validate(LevelData level)
        {
            var issues = new List<LevelIssue>();
            CheckBoard(level, issues);
            CheckBlocks(level, issues);
            CheckDoors(level, issues);
            CheckEveryBlockCanLeave(level, issues);
            CheckEveryDoorIsUsed(level, issues);
            return issues;
        }

        public bool HasErrors(IReadOnlyList<LevelIssue> issues)
        {
            for (int i = 0; i < issues.Count; i++)
                if (issues[i].Severity == LevelIssueSeverity.Error)
                    return true;
            return false;
        }

        void CheckBoard(LevelData level, List<LevelIssue> issues)
        {
            if (!IsBoardSizeAllowed(level.Width) || !IsBoardSizeAllowed(level.Height))
                Error(issues, $"The board is {level.Width} x {level.Height}; width and height must be between " +
                    $"{_rules.MinBoardSize} and {_rules.MaxBoardSize}.");

            if (level.TimerSeconds < _rules.MinTimerSeconds || level.TimerSeconds > _rules.MaxTimerSeconds)
                Error(issues, $"The timer is {level.TimerSeconds} s; it must be between {_rules.MinTimerSeconds} " +
                    $"and {_rules.MaxTimerSeconds} s.");

            if (level.Blocks.Count == 0)
                Error(issues, "The level has no blocks; it would be won before the player touches it.");

            if (!Enum.IsDefined(typeof(LevelDifficulty), level.Difficulty))
                Error(issues, "The level has no difficulty chosen; it sets the win reward.");
        }

        bool IsBoardSizeAllowed(int size) => size >= _rules.MinBoardSize && size <= _rules.MaxBoardSize;

        void CheckBlocks(LevelData level, List<LevelIssue> issues)
        {
            var owner = new Dictionary<Vector2Int, int>();
            for (int b = 0; b < level.Blocks.Count; b++)
            {
                BlockData block = level.Blocks[b];
                string name = BlockName(block, b);

                if (!Enum.IsDefined(typeof(BlockColor), block.Color))
                    Error(issues, $"{name} has no colour chosen.");

                if (block.Cells.Count == 0)
                {
                    Error(issues, $"{name} has no cells.");
                    continue;
                }

                foreach (Vector2Int cell in block.Cells)
                {
                    if (!IsInside(level, cell))
                        Error(issues, $"{name} has cell {cell} outside the {level.Width} x {level.Height} board.");
                    else if (owner.TryGetValue(cell, out int other))
                        Error(issues, other == b
                            ? $"{name} lists cell {cell} twice."
                            : $"{name} overlaps {BlockName(level.Blocks[other], other)} at {cell}.");
                    else
                        owner.Add(cell, b);
                }

                if (!IsConnected(block))
                    Error(issues, $"{name} is split into separate pieces; every cell must touch another by an edge.");

                if (block.Ice < 0)
                    Error(issues, $"{name} has ice {block.Ice}; ice counts exits and cannot be negative.");
                else if (block.Ice > level.Blocks.Count - 1)
                    Error(issues, $"{name} thaws after {block.Ice} other blocks leave, but the level has only " +
                        $"{level.Blocks.Count - 1} other blocks; it would stay frozen.");
            }
        }

        void CheckDoors(LevelData level, List<LevelIssue> issues)
        {
            for (int d = 0; d < level.Doors.Count; d++)
            {
                DoorData door = level.Doors[d];
                string name = DoorName(door, d);

                if (!Enum.IsDefined(typeof(BlockColor), door.Color))
                    Error(issues, $"{name} has no colour chosen.");

                if (!Enum.IsDefined(typeof(WallSide), door.Side))
                {
                    Error(issues, $"{name} has no wall chosen.");
                    continue;
                }

                int wallLength = level.WallLength(door.Side);
                if (door.Length < 1)
                {
                    Error(issues, $"{name} has length {door.Length}; a door spans at least one cell.");
                    continue;
                }

                if (door.Start < 0 || door.End >= wallLength)
                    Error(issues, $"{name} spans cells {door.Start}-{door.End}, outside the {wallLength} cells of " +
                        $"the {door.Side} wall.");

                for (int other = 0; other < d; other++)
                {
                    DoorData earlier = level.Doors[other];
                    // An earlier door with no length was already reported and spans nothing.
                    if (earlier.Length < 1)
                        continue;
                    if (earlier.Side == door.Side && earlier.Start <= door.End && door.Start <= earlier.End)
                        Error(issues, $"{name} overlaps {DoorName(earlier, other)}.");
                }
            }
        }

        void CheckEveryBlockCanLeave(LevelData level, List<LevelIssue> issues)
        {
            for (int b = 0; b < level.Blocks.Count; b++)
            {
                BlockData block = level.Blocks[b];
                if (block.Cells.Count > 0 && !HasFittingDoor(level, block))
                    Error(issues, $"{BlockName(block, b)} can never leave: no {block.Color} door is as wide as the " +
                        "block along its wall.");
            }
        }

        void CheckEveryDoorIsUsed(LevelData level, List<LevelIssue> issues)
        {
            for (int d = 0; d < level.Doors.Count; d++)
            {
                DoorData door = level.Doors[d];
                bool used = false;
                foreach (BlockData block in level.Blocks)
                    used |= block.Color == door.Color;

                if (!used)
                    issues.Add(new LevelIssue(LevelIssueSeverity.Warning,
                        $"{DoorName(door, d)} has no {door.Color} block to let out."));
            }
        }

        bool HasFittingDoor(LevelData level, BlockData block)
        {
            foreach (DoorData door in level.Doors)
                if (door.Color == block.Color && door.Length >= SpanAlong(block, door.Side))
                    return true;
            return false;
        }

        /// <summary>How many cells the block covers along a wall: its width for Bottom/Top, height for Left/Right.</summary>
        int SpanAlong(BlockData block, WallSide side)
        {
            bool alongX = side is WallSide.Bottom or WallSide.Top;
            int min = int.MaxValue, max = int.MinValue;
            foreach (Vector2Int cell in block.Cells)
            {
                int value = alongX ? cell.x : cell.y;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            return max - min + 1;
        }

        bool IsConnected(BlockData block)
        {
            var cells = new HashSet<Vector2Int>(block.Cells);
            var reached = new HashSet<Vector2Int> { block.Cells[0] };
            var frontier = new Queue<Vector2Int>(reached);
            while (frontier.Count > 0)
            {
                Vector2Int cell = frontier.Dequeue();
                foreach (Vector2Int step in Neighbours)
                {
                    Vector2Int next = cell + step;
                    if (cells.Contains(next) && reached.Add(next))
                        frontier.Enqueue(next);
                }
            }

            return reached.Count == cells.Count;
        }

        bool IsInside(LevelData level, Vector2Int cell) =>
            cell.x >= 0 && cell.y >= 0 && cell.x < level.Width && cell.y < level.Height;

        string BlockName(BlockData block, int index) => $"Block {index + 1} ({block.Color})";

        string DoorName(DoorData door, int index) => $"Door {index + 1} ({door.Color}, {door.Side} wall)";

        void Error(List<LevelIssue> issues, string message) =>
            issues.Add(new LevelIssue(LevelIssueSeverity.Error, message));
    }
}

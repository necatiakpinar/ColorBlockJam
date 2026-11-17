using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    [Serializable]
    public sealed class DraftBlock
    {
        public BlockColor Color;
        public List<Vector2Int> Cells = new();

        public BlockerSet Blockers = new();
    }

    [Serializable]
    public sealed class DraftDoor
    {
        public WallSide Side;
        public int Start;
        public int Length = 1;
        public BlockColor Color;

        public int End => Start + Length - 1;
        public bool Covers(WallSide side, int index) => Side == side && index >= Start && index <= End;
    }

    [Serializable]
    public sealed class LevelDraft
    {
        public int Width = 4;
        public int Height = 4;
        public int TimerSeconds = 60;
        public LevelDifficulty Difficulty = LevelDifficulty.Normal;
        public List<DraftBlock> Blocks = new();
        public List<DraftDoor> Doors = new();

        public static LevelDraft From(LevelData level)
        {
            var draft = new LevelDraft
            {
                Width = level.Width,
                Height = level.Height,
                TimerSeconds = level.TimerSeconds,
                Difficulty = level.Difficulty,
            };
            foreach (BlockData block in level.Blocks)
                draft.Blocks.Add(new DraftBlock { Color = block.Color, Cells = new List<Vector2Int>(block.Cells), Blockers = block.Blockers.Clone() });
            foreach (DoorData door in level.Doors)
                draft.Doors.Add(new DraftDoor { Side = door.Side, Start = door.Start, Length = door.Length, Color = door.Color });
            return draft;
        }

        public LevelData ToLevel()
        {
            var blocks = new List<BlockData>();
            foreach (DraftBlock block in Blocks)
                blocks.Add(new BlockData(block.Color, block.Cells, block.Blockers.Clone()));
            var doors = new List<DoorData>();
            foreach (DraftDoor door in Doors)
                doors.Add(new DoorData(door.Side, door.Start, door.Length, door.Color));
            return new LevelData(Width, Height, TimerSeconds, blocks, doors, Difficulty);
        }

        public int WallLength(WallSide side) => side is WallSide.Bottom or WallSide.Top ? Width : Height;

        public bool IsInside(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

        public int BlockAt(Vector2Int cell)
        {
            for (int i = 0; i < Blocks.Count; i++)
                if (Blocks[i].Cells.Contains(cell))
                    return i;
            return -1;
        }

        public int DoorAt(WallSide side, int index)
        {
            for (int i = 0; i < Doors.Count; i++)
                if (Doors[i].Covers(side, index))
                    return i;
            return -1;
        }

        public bool CanPlace(IReadOnlyList<Vector2Int> cells, int ignore = -1)
        {
            var seen = new HashSet<Vector2Int>();
            foreach (Vector2Int cell in cells)
            {
                if (!IsInside(cell) || !seen.Add(cell))
                    return false;
                int owner = BlockAt(cell);
                if (owner >= 0 && owner != ignore)
                    return false;
            }

            return cells.Count > 0;
        }

        public bool IsConnected(IReadOnlyList<Vector2Int> cells)
        {
            if (cells.Count == 0)
                return false;
            var all = new HashSet<Vector2Int>(cells);
            var reached = new HashSet<Vector2Int> { cells[0] };
            var frontier = new Queue<Vector2Int>();
            frontier.Enqueue(cells[0]);
            while (frontier.Count > 0)
            {
                Vector2Int cell = frontier.Dequeue();
                foreach (Vector2Int next in new[] { cell + Vector2Int.right, cell + Vector2Int.left, cell + Vector2Int.up, cell + Vector2Int.down })
                    if (all.Contains(next) && reached.Add(next))
                        frontier.Enqueue(next);
            }

            return reached.Count == all.Count;
        }

        public bool TryAddBlock(BlockColor color, IReadOnlyList<Vector2Int> cells)
        {
            if (!CanPlace(cells) || !IsConnected(cells))
                return false;
            Blocks.Add(new DraftBlock { Color = color, Cells = new List<Vector2Int>(cells) });
            return true;
        }

        public bool TryMoveBlock(int index, Vector2Int delta)
        {
            var moved = new List<Vector2Int>();
            foreach (Vector2Int cell in Blocks[index].Cells)
                moved.Add(cell + delta);
            if (!CanPlace(moved, index))
                return false;
            Blocks[index].Cells = moved;
            return true;
        }

        public bool TryRotateBlock(int index)
        {
            List<Vector2Int> cells = Blocks[index].Cells;
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            foreach (Vector2Int cell in cells)
                min = Vector2Int.Min(min, cell);
            var shape = new BlockShape("block", cells.ToArray()).Rotated();
            foreach (Vector2Int nudge in new[] { Vector2Int.zero, Vector2Int.left, Vector2Int.down, new Vector2Int(-1, -1),
                         new Vector2Int(-2, 0), new Vector2Int(0, -2) })
            {
                List<Vector2Int> turned = shape.At(min + nudge);
                if (!CanPlace(turned, index))
                    continue;
                Blocks[index].Cells = turned;
                return true;
            }

            return false;
        }

        public bool TryAddDoor(WallSide side, int start, int length, BlockColor color)
        {
            var door = new DraftDoor { Side = side, Start = start, Length = length, Color = color };
            if (!FitsWall(door, -1))
                return false;
            Doors.Add(door);
            return true;
        }

        public bool TrySetDoorSpan(int index, int start, int length)
        {
            DraftDoor door = Doors[index];
            var probe = new DraftDoor { Side = door.Side, Start = start, Length = length, Color = door.Color };
            if (!FitsWall(probe, index))
                return false;
            door.Start = start;
            door.Length = length;
            return true;
        }

        bool FitsWall(DraftDoor door, int ignore)
        {
            if (door.Length < 1 || door.Start < 0 || door.End >= WallLength(door.Side))
                return false;
            for (int i = 0; i < Doors.Count; i++)
                if (i != ignore && Doors[i].Side == door.Side && Doors[i].Start <= door.End && Doors[i].End >= door.Start)
                    return false;
            return true;
        }

        public (int Blocks, int Doors) Resize(int width, int height)
        {
            Width = width;
            Height = height;
            int blocks = Blocks.RemoveAll(block => !block.Cells.TrueForAll(IsInside));
            int doors = Doors.RemoveAll(door => door.End >= WallLength(door.Side));
            return (blocks, doors);
        }

        public (int Blocks, int Doors) WouldLoseOnResize(int width, int height)
        {
            int blocks = 0;
            foreach (DraftBlock block in Blocks)
                if (!block.Cells.TrueForAll(cell => cell.x < width && cell.y < height))
                    blocks++;
            int doors = 0;
            foreach (DraftDoor door in Doors)
                if (door.End >= (door.Side is WallSide.Bottom or WallSide.Top ? width : height))
                    doors++;
            return (blocks, doors);
        }
    }
}

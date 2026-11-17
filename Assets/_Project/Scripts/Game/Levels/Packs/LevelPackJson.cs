using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Levels.Blockers;
using UnityEngine;

namespace ColorBlockJam.Game.Levels.Packs
{
    public sealed class LevelPackJson
    {
        public const int Format = 1;

        public string Write(LevelPack pack, bool readable = true)
        {
            var dto = new PackDto { format = Format, revision = pack.Revision, levels = new List<LevelDto>() };
            foreach (LevelData level in pack.Levels)
                dto.levels.Add(ToDto(level));
            return JsonUtility.ToJson(dto, readable);
        }

        public bool TryRead(string json, out LevelPack pack, out string error)
        {
            pack = null;
            PackDto dto;
            try
            {
                dto = string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<PackDto>(json);
            }
            catch (ArgumentException exception)
            {
                error = $"Not valid JSON: {exception.Message}";
                return false;
            }

            if (dto == null)
                return Fail("The pack is empty.", out error);
            if (dto.format != Format)
                return Fail($"Pack format {dto.format} is not {Format}, the one this build reads.", out error);
            if (dto.levels == null || dto.levels.Count == 0)
                return Fail("The pack has no levels.", out error);

            var levels = new List<LevelData>(dto.levels.Count);
            for (int i = 0; i < dto.levels.Count; i++)
            {
                if (!TryFromDto(dto.levels[i], out LevelData level, out string problem))
                    return Fail($"Level {i + 1}: {problem}", out error);
                levels.Add(level);
            }

            pack = new LevelPack(dto.revision, levels);
            error = null;
            return true;
        }

        static bool Fail(string problem, out string error)
        {
            error = problem;
            return false;
        }

        static LevelDto ToDto(LevelData level)
        {
            var dto = new LevelDto
            {
                width = level.Width,
                height = level.Height,
                timer = level.TimerSeconds,
                difficulty = level.Difficulty.ToString(),
                doors = new List<DoorDto>(),
                blocks = new List<BlockDto>(),
            };
            foreach (DoorData door in level.Doors)
                dto.doors.Add(new DoorDto { side = door.Side.ToString(), start = door.Start, length = door.Length, color = door.Color.ToString() });
            foreach (BlockData block in level.Blocks)
            {
                var cells = new List<CellDto>(block.Cells.Count);
                foreach (Vector2Int cell in block.Cells)
                    cells.Add(new CellDto { x = cell.x, y = cell.y });
                dto.blocks.Add(new BlockDto
                {
                    color = block.Color.ToString(),
                    cells = cells,
                    ice = block.Blockers.Ice.Exits,
                    arrow = block.Blockers.Arrow.Axis.ToString(),
                });
            }

            return dto;
        }

        static bool TryFromDto(LevelDto dto, out LevelData level, out string problem)
        {
            level = null;
            if (dto == null)
                return Fail("missing.", out problem);
            if (!TryName(dto.difficulty, out LevelDifficulty difficulty))
                return Fail($"unknown difficulty '{dto.difficulty}'.", out problem);

            var doors = new List<DoorData>();
            for (int i = 0; dto.doors != null && i < dto.doors.Count; i++)
            {
                DoorDto door = dto.doors[i];
                if (!TryName(door.side, out WallSide side))
                    return Fail($"door {i + 1} has an unknown side '{door.side}'.", out problem);
                if (!TryName(door.color, out BlockColor color))
                    return Fail($"door {i + 1} has an unknown colour '{door.color}'.", out problem);
                doors.Add(new DoorData(side, door.start, door.length, color));
            }

            var blocks = new List<BlockData>();
            for (int i = 0; dto.blocks != null && i < dto.blocks.Count; i++)
            {
                BlockDto block = dto.blocks[i];
                if (!TryName(block.color, out BlockColor color))
                    return Fail($"block {i + 1} has an unknown colour '{block.color}'.", out problem);
                ArrowAxis axis = ArrowAxis.None;
                if (!string.IsNullOrEmpty(block.arrow) && !TryName(block.arrow, out axis))
                    return Fail($"block {i + 1} has an unknown arrow '{block.arrow}'.", out problem);
                var cells = new List<Vector2Int>();
                for (int c = 0; block.cells != null && c < block.cells.Count; c++)
                    cells.Add(new Vector2Int(block.cells[c].x, block.cells[c].y));
                blocks.Add(new BlockData(color, cells, new BlockerSet(new IceBlockerData(block.ice), new ArrowBlockerData(axis))));
            }

            level = new LevelData(dto.width, dto.height, dto.timer, blocks, doors, difficulty);
            problem = null;
            return true;
        }

        static bool TryName<T>(string name, out T value) where T : struct, Enum
        {
            value = default;
            return !string.IsNullOrEmpty(name) && char.IsLetter(name[0]) && Enum.TryParse(name, false, out value) &&
                   Enum.IsDefined(typeof(T), value);
        }

#pragma warning disable IDE1006
        [Serializable]
        sealed class PackDto
        {
            public int format;
            public int revision;
            public List<LevelDto> levels;
        }

        [Serializable]
        sealed class LevelDto
        {
            public int width;
            public int height;
            public int timer;
            public string difficulty;
            public List<DoorDto> doors;
            public List<BlockDto> blocks;
        }

        [Serializable]
        sealed class DoorDto
        {
            public string side;
            public int start;
            public int length;
            public string color;
        }

        [Serializable]
        sealed class BlockDto
        {
            public string color;
            public List<CellDto> cells;
            public int ice;
            public string arrow;
        }

        [Serializable]
        sealed class CellDto
        {
            public int x;
            public int y;
        }
#pragma warning restore IDE1006
    }
}

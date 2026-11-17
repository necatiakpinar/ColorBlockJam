using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Game.Levels
{
    /// <summary>
    /// A block outline in cells, anchored at (0, 0) at its bottom-left corner. The level editor stamps these and
    /// the generator builds levels from them (level-editor.md).
    /// </summary>
    public readonly struct BlockShape
    {
        public BlockShape(string name, params Vector2Int[] cells)
        {
            Name = name;
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            foreach (Vector2Int cell in cells)
                min = Vector2Int.Min(min, cell);
            var normalized = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
                normalized[i] = cells[i] - min;
            Cells = normalized;
        }

        public string Name { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }

        public int Width => Extent(true);
        public int Height => Extent(false);

        /// <summary>The same shape turned a quarter clockwise, still anchored at (0, 0).</summary>
        public BlockShape Rotated()
        {
            var turned = new Vector2Int[Cells.Count];
            for (int i = 0; i < turned.Length; i++)
                turned[i] = new Vector2Int(Cells[i].y, -Cells[i].x);
            return new BlockShape(Name, turned);
        }

        /// <summary>The board cells the shape covers with its anchor on <paramref name="origin"/>.</summary>
        public List<Vector2Int> At(Vector2Int origin)
        {
            var cells = new List<Vector2Int>(Cells.Count);
            foreach (Vector2Int cell in Cells)
                cells.Add(origin + cell);
            return cells;
        }

        int Extent(bool alongX)
        {
            int max = 0;
            foreach (Vector2Int cell in Cells)
                max = Mathf.Max(max, alongX ? cell.x : cell.y);
            return max + 1;
        }
    }

    /// <summary>The preset block outlines, in the order the editor offers them (Tab cycles them).</summary>
    public sealed class BlockShapeLibrary
    {
        readonly BlockShape[] _shapes =
        {
            new("1x1", new Vector2Int(0, 0)),
            new("1x2", new Vector2Int(0, 0), new Vector2Int(1, 0)),
            new("1x3", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)),
            new("2x2", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
            new("L", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)),
            new("T", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1)),
            new("S", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        };

        public IReadOnlyList<BlockShape> Shapes => _shapes;
    }
}

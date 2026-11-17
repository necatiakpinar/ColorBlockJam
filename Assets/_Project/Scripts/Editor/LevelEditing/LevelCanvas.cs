using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    public sealed class LevelCanvas
    {
        static readonly Color Background = new(0.16f, 0.17f, 0.22f);
        static readonly Color Wall = new(0.24f, 0.26f, 0.36f);
        static readonly Color Floor = new(0.78f, 0.82f, 0.92f);
        static readonly Color Selection = Color.white;

        readonly BoardViewConfig _colors;
        readonly HashSet<Vector2Int> _blockCells = new();
        GUIStyle _label;
        Rect _area;
        float _cell;
        Vector2 _origin;
        int _height;
        int _width;

        public LevelCanvas(BoardViewConfig colors)
        {
            _colors = colors;
        }

        public float CellSize => _cell;

        public Color ColorOf(BlockColor color) => _colors.TintOf(color);

        public void Layout(Rect area, int width, int height)
        {
            _area = area;
            _width = width;
            _height = height;
            _cell = Mathf.Floor(Mathf.Min(area.width / (width + 2), area.height / (height + 2)));
            _origin = new Vector2(area.x + (area.width - _cell * (width + 2)) * 0.5f,
                area.y + (area.height - _cell * (height + 2)) * 0.5f);
        }

        public Rect CellRect(int x, int y) =>
            new(_origin.x + (x + 1) * _cell, _origin.y + (_height - y) * _cell, _cell, _cell);

        public bool TryGetCell(Vector2 mouse, out Vector2Int cell)
        {
            cell = new Vector2Int(Mathf.FloorToInt((mouse.x - _origin.x) / _cell) - 1,
                _height - Mathf.FloorToInt((mouse.y - _origin.y) / _cell));
            return cell.x >= 0 && cell.y >= 0 && cell.x < _width && cell.y < _height;
        }

        public bool TryGetWall(Vector2 mouse, out WallSide side, out int index)
        {
            TryGetCell(mouse, out Vector2Int c);
            side = WallSide.Bottom;
            index = -1;
            if (c.y == -1 && c.x >= 0 && c.x < _width)
                (side, index) = (WallSide.Bottom, c.x);
            else if (c.y == _height && c.x >= 0 && c.x < _width)
                (side, index) = (WallSide.Top, c.x);
            else if (c.x == -1 && c.y >= 0 && c.y < _height)
                (side, index) = (WallSide.Left, c.y);
            else if (c.x == _width && c.y >= 0 && c.y < _height)
                (side, index) = (WallSide.Right, c.y);
            return index >= 0;
        }

        public Rect WallRect(WallSide side, int index) => side switch
        {
            WallSide.Bottom => CellRect(index, -1),
            WallSide.Top => CellRect(index, _height),
            WallSide.Left => CellRect(-1, index),
            _ => CellRect(_width, index),
        };

        public void DrawBoard(LevelDraft draft, int selectedDoor)
        {
            EditorGUI.DrawRect(_area, Background);
            for (int x = -1; x <= draft.Width; x++)
                for (int y = -1; y <= draft.Height; y++)
                {
                    bool inside = x >= 0 && y >= 0 && x < draft.Width && y < draft.Height;
                    EditorGUI.DrawRect(Inset(CellRect(x, y), inside ? 1f : 0f), inside ? Floor : Wall);
                }

            for (int i = 0; i < draft.Doors.Count; i++)
                DrawDoor(draft.Doors[i], i == selectedDoor);
        }

        void DrawDoor(DraftDoor door, bool selected)
        {
            for (int index = door.Start; index <= door.End; index++)
            {
                Rect rect = WallRect(door.Side, index);
                EditorGUI.DrawRect(Inset(rect, _cell * 0.12f), ColorOf(door.Color));
            }

            Rect first = WallRect(door.Side, door.Start);
            Rect last = WallRect(door.Side, door.End);
            Rect span = Rect.MinMaxRect(Mathf.Min(first.xMin, last.xMin), Mathf.Min(first.yMin, last.yMin),
                Mathf.Max(first.xMax, last.xMax), Mathf.Max(first.yMax, last.yMax));
            string arrow = door.Side switch
            {
                WallSide.Bottom => "▼",
                WallSide.Top => "▲",
                WallSide.Left => "◀",
                _ => "▶",
            };
            GUI.Label(span, arrow, CenteredLabel(Mathf.RoundToInt(_cell * 0.35f), Color.white));
            if (selected)
                Outline(span, Selection, 3f);
        }

        public void DrawBlock(IReadOnlyList<Vector2Int> cells, Vector2Int offset, Color color, bool selected, string label)
        {
            HashSet<Vector2Int> set = _blockCells;
            set.Clear();
            for (int i = 0; i < cells.Count; i++)
                set.Add(cells[i] + offset);
            float gap = Mathf.Max(2f, _cell * 0.08f);
            foreach (Vector2Int cell in set)
            {
                Rect rect = CellRect(cell.x, cell.y);
                EditorGUI.DrawRect(Inset(rect, gap), color);
                if (set.Contains(cell + Vector2Int.right))
                    EditorGUI.DrawRect(new Rect(rect.xMax - gap, rect.y + gap, gap * 2f, rect.height - gap * 2f), color);
                if (set.Contains(cell + Vector2Int.up))
                    EditorGUI.DrawRect(new Rect(rect.x + gap, rect.y - gap, rect.width - gap * 2f, gap * 2f), color);
                if (selected)
                    Outline(Inset(rect, gap), Selection, 2f);
            }

            if (label != null && set.Count > 0)
            {
                foreach (Vector2Int cell in set)
                {
                    GUI.Label(CellRect(cell.x, cell.y), label, CenteredLabel(Mathf.RoundToInt(_cell * 0.3f), new Color(0, 0, 0, 0.6f)));
                    break;
                }
            }
        }

        public void DrawGhost(IEnumerable<Vector2Int> cells, Color color, bool allowed)
        {
            Color tint = allowed ? new Color(color.r, color.g, color.b, 0.55f) : new Color(1f, 0.15f, 0.15f, 0.55f);
            foreach (Vector2Int cell in cells)
                EditorGUI.DrawRect(Inset(CellRect(cell.x, cell.y), _cell * 0.1f), tint);
        }

        public void DrawWallGhost(WallSide side, int from, int to, Color color, bool allowed)
        {
            Color tint = allowed ? new Color(color.r, color.g, color.b, 0.7f) : new Color(1f, 0.15f, 0.15f, 0.7f);
            for (int i = Mathf.Min(from, to); i <= Mathf.Max(from, to); i++)
                EditorGUI.DrawRect(Inset(WallRect(side, i), _cell * 0.12f), tint);
        }

        public void HighlightCell(Rect rect) => Outline(rect, new Color(1f, 1f, 1f, 0.5f), 1f);

        static Rect Inset(Rect rect, float by) => new(rect.x + by, rect.y + by, rect.width - by * 2f, rect.height - by * 2f);

        static void Outline(Rect rect, Color color, float width)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, width), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - width, rect.width, width), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, width, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - width, rect.y, width, rect.height), color);
        }

        GUIStyle CenteredLabel(int size, Color color)
        {
            _label ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            _label.fontSize = Mathf.Max(8, size);
            _label.normal.textColor = color;
            return _label;
        }
    }
}

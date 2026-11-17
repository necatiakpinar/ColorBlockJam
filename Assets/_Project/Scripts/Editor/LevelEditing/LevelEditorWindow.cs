using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Blockers;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Packs;
using ColorBlockJam.Game.Levels.Solving;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";
        const string LevelsFolder = "Assets/_Project/Levels";
        const string RulesPath = "Assets/_Project/Configs/LevelRulesConfig.asset";
        const string ViewConfigPath = "Assets/_Project/Configs/BoardViewConfig.asset";
        const string RewardsPath = "Assets/_Project/Configs/LevelRewardConfig.asset";
        const float ListWidth = 210f;
        const float InspectorWidth = 320f;
        const int GeneratorStateCap = 2000;
        const int HardenerStateCap = 5000;
        const int HardenerRounds = 25;
        const int SolveSettleMilliseconds = 250;
        static readonly Color IceTint = new(0.85f, 0.95f, 1f);

        enum Tool
        {
            Select = 1000,
            Block = 2000,
            Door = 3000,
            Erase = 4000,
        }

        static readonly BlockColor[] Colors = (BlockColor[])Enum.GetValues(typeof(BlockColor));

        LevelCatalogEditing _catalog;
        LevelRulesConfig _rules;
        LevelRewardConfig _rewards;
        LevelDefinitionValidator _validator;
        LevelChecker _checker;
        BlockShapeLibrary _shapes;
        LevelGenerator _generator;
        LevelHardener _hardener;
        LevelCanvas _canvas;
        LevelEditorState _state;

        int _level = -1;
        Tool _tool = Tool.Select;
        int _color;
        int _shape = -1;
        int _stampTurns;
        int _selectedBlock = -1;
        int _selectedDoor = -1;
        bool _showHelp = true;
        Vector2 _listScroll;
        Vector2 _inspectorScroll;

        readonly List<Vector2Int> _stroke = new();
        bool _dragging;
        Vector2Int _dragFrom;
        WallSide _doorSide;
        int _doorFrom = -1;
        int _doorTo = -1;

        int _previewStep;

        bool _isDirty;
        readonly List<string> _blockLabels = new();
        string[] _levelLabels = Array.Empty<string>();
        GUIContent[] _toolLabels;
        string[] _shapeNames;
        GUIStyle _levelStyle;
        GUIStyle _currentLevelStyle;
        int _hoverKey = int.MinValue;
        bool _generating;
        CancellationTokenSource _generation;

        int _genWidth = 6;
        int _genHeight = 7;
        int _genColors = 4;
        int _genBlocks = 6;
        LevelDifficulty _genDifficulty = LevelDifficulty.Hard;
        int _genSeed = 1;
        string _genMessage;

        [MenuItem("ColorBlockJam/Level Editor %#l")]
        static void Open() => GetWindow<LevelEditorWindow>("Level Editor").Show();

        [MenuItem("ColorBlockJam/Write Level Pack (levels.json)")]
        static void WritePack() =>
            new LevelCatalogEditing(AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath), LevelsFolder).WritePack();

        [MenuItem("ColorBlockJam/Copy Level Pack for Remote Config")]
        static void CopyPackForRemote()
        {
            var json = new LevelPackJson();
            string path = $"{LevelsFolder}/{LevelCatalogEditing.PackFile}";
            if (!json.TryRead(System.IO.File.ReadAllText(path), out LevelPack pack, out string error))
                throw new InvalidOperationException($"{path} does not read: {error}");
            EditorGUIUtility.systemCopyBuffer = json.Write(pack, readable: false);
            EditorUtility.DisplayDialog("Level pack copied",
                $"Revision {pack.Revision}, {pack.Levels.Count} levels, on the clipboard as compact JSON.", "OK");
        }

        LevelDraft Draft => _state.Draft;

        void OnEnable()
        {
            _catalog = new LevelCatalogEditing(AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath), LevelsFolder);
            _rules = AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath);
            _rewards = AssetDatabase.LoadAssetAtPath<LevelRewardConfig>(RewardsPath);
            _validator = new LevelDefinitionValidator(_rules);
            _checker = new LevelChecker(_validator, _rules.SolverStateCap, SolveSettleMilliseconds);
            _checker.Solved += Repaint;
            _shapes = new BlockShapeLibrary();
            _generator = new LevelGenerator(_validator, GeneratorStateCap, _shapes);
            _hardener = new LevelHardener(_validator, HardenerStateCap);
            _toolLabels = new[]
            {
                new GUIContent("Select", "Pick and drag blocks, pick doors (1)."), new GUIContent("Block", "Stamp or paint blocks (2)."),
                new GUIContent("Door", "Drag along a wall to add a door (3)."), new GUIContent("Erase", "Click a block or door to remove it (4)."),
            };
            var shapeNames = new List<string> { "Free" };
            foreach (BlockShape shape in _shapes.Shapes)
                shapeNames.Add(shape.Name);
            _shapeNames = shapeNames.ToArray();
            _canvas = new LevelCanvas(AssetDatabase.LoadAssetAtPath<BoardViewConfig>(ViewConfigPath));
            _state = CreateInstance<LevelEditorState>();
            _state.hideFlags = HideFlags.HideAndDontSave;
            wantsMouseMove = true;
            Undo.undoRedoPerformed += OnUndoRedo;
            RefreshLevelList();
            if (_catalog.Catalog.Levels.Count > 0)
                Load(0);
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            _checker.Solved -= Repaint;
            _checker.Dispose();
            _generation?.Cancel();
            Undo.ClearUndo(_state);
            DestroyImmediate(_state);
        }

        void OnUndoRedo()
        {
            ClampSelection();
            OnDraftChanged();
            Repaint();
        }

        void OnDraftChanged()
        {
            if (_level < 0)
                return;
            LevelData level = Draft.ToLevel();
            _checker.Check(level);
            _isDirty = JsonUtility.ToJson(level) != JsonUtility.ToJson(_catalog.Catalog.Levels[_level].Data);
            _previewStep = 0;
            _blockLabels.Clear();
            for (int i = 0; i < Draft.Blocks.Count; i++)
                _blockLabels.Add(MakeBlockLabel(i));
        }

        void CheckNow()
        {
            if (_level >= 0)
                _checker.Check(Draft.ToLevel(), now: true);
        }

        void RefreshLevelList()
        {
            IReadOnlyList<LevelDefinition> levels = _catalog.Catalog.Levels;
            _levelLabels = new string[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                LevelData data = levels[i].Data;
                _levelLabels[i] = $"{i + 1}. {data.Width}x{data.Height} · {data.Blocks.Count} blocks · {data.Difficulty}";
            }
        }

        SolveResult Solution => _checker.Solution;

        void OnGUI()
        {
            HandleShortcuts(Event.current);
            DrawToolbar();
            float top = EditorGUIUtility.singleLineHeight + 6f;
            var list = new Rect(0, top, ListWidth, position.height - top);
            var inspector = new Rect(position.width - InspectorWidth, top, InspectorWidth, position.height - top);
            var canvas = new Rect(ListWidth, top, position.width - ListWidth - InspectorWidth, position.height - top);
            DrawLevelList(list);
            DrawCanvas(canvas);
            DrawInspector(inspector);
        }

        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(_level >= 0 ? (_isDirty ? $"Level {_level + 1} *" : $"Level {_level + 1}") : "No level", EditorStyles.boldLabel,
                GUILayout.Width(110));
            using (new EditorGUI.DisabledScope(_level < 0))
            {
                if (GUILayout.Button(new GUIContent("Save", "Write the level to its asset (Ctrl+S)."), EditorStyles.toolbarButton))
                    Save();
                if (GUILayout.Button(new GUIContent("Revert", "Drop the unsaved changes."), EditorStyles.toolbarButton))
                    Load(_level);
                if (GUILayout.Button(new GUIContent("Check", "Validate and solve now (C)."), EditorStyles.toolbarButton))
                    CheckNow();
                if (GUILayout.Button(new GUIContent("▶ Play", "Save and play this level in the game (P)."), EditorStyles.toolbarButton))
                    PlayTest();
            }

            GUILayout.FlexibleSpace();
            _showHelp = GUILayout.Toggle(_showHelp, new GUIContent("Help", "Show the guide and the shortcuts (F1)."),
                EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
        }

        void DrawLevelList(Rect area)
        {
            GUILayout.BeginArea(area, EditorStyles.helpBox);
            GUILayout.Label("Levels", EditorStyles.boldLabel);
            _listScroll = GUILayout.BeginScrollView(_listScroll);
            IReadOnlyList<LevelDefinition> levels = _catalog.Catalog.Levels;
            if (levels.Count == 0)
                EditorGUILayout.HelpBox("The catalog is empty. Press New to make the first level.", MessageType.Info);
            _levelStyle ??= new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleLeft };
            _currentLevelStyle ??= new GUIStyle(_levelStyle) { fontStyle = FontStyle.Bold };
            for (int i = 0; i < _levelLabels.Length; i++)
                if (GUILayout.Toggle(i == _level, _levelLabels[i], i == _level ? _currentLevelStyle : _levelStyle) && i != _level)
                    TrySwitchTo(i);

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("New", "Add an empty level at the end (N).")))
                NewLevel();
            using (new EditorGUI.DisabledScope(_level < 0))
            {
                if (GUILayout.Button(new GUIContent("Copy", "Duplicate this level after itself (Shift+D).")))
                    DuplicateLevel();
                if (GUILayout.Button(new GUIContent("Delete", "Delete this level and its asset.")))
                    DeleteLevel();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_level <= 0))
                if (GUILayout.Button(new GUIContent("▲ Up", "Play this level earlier.")))
                    MoveLevel(-1);
            using (new EditorGUI.DisabledScope(_level < 0 || _level >= levels.Count - 1))
                if (GUILayout.Button(new GUIContent("▼ Down", "Play this level later.")))
                    MoveLevel(1);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawCanvas(Rect area)
        {
            if (_level < 0)
            {
                GUI.Label(area, "Select or create a level.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            _canvas.Layout(new Rect(area.x + 8, area.y + 8, area.width - 16, area.height - 16), Draft.Width, Draft.Height);
            _canvas.DrawBoard(Draft, _selectedDoor);
            if (_previewStep > 0 && Solution != null && Solution.Status == SolveStatus.Solvable)
                DrawSolutionPreview();
            else
                for (int i = 0; i < Draft.Blocks.Count; i++)
                    _canvas.DrawBlock(Draft.Blocks[i].Cells, Vector2Int.zero, BlockTint(Draft.Blocks[i]), i == _selectedBlock,
                        BlockLabel(i));

            Event e = Event.current;
            if (area.Contains(e.mousePosition))
            {
                DrawHover(e.mousePosition);
                HandleCanvasMouse(e);
            }
        }

        void DrawHover(Vector2 mouse)
        {
            if (_canvas.TryGetCell(mouse, out Vector2Int cell))
                _canvas.HighlightCell(_canvas.CellRect(cell.x, cell.y));
            Color color = _canvas.ColorOf(Colors[_color]);
            if (_tool == Tool.Block)
            {
                if (_stroke.Count > 0)
                    _canvas.DrawGhost(_stroke, color, Draft.CanPlace(_stroke) && Draft.IsConnected(_stroke));
                else if (_shape >= 0 && Draft.IsInside(cell))
                {
                    List<Vector2Int> stamp = Stamp(cell);
                    _canvas.DrawGhost(stamp, color, Draft.CanPlace(stamp));
                }
            }

            if (_tool == Tool.Door)
            {
                if (_doorFrom >= 0)
                    _canvas.DrawWallGhost(_doorSide, _doorFrom, _doorTo, color, CanAddDoor());
                else if (_canvas.TryGetWall(mouse, out WallSide side, out int index))
                    _canvas.DrawWallGhost(side, index, index, color, Draft.DoorAt(side, index) < 0);
            }
        }

        void HandleCanvasMouse(Event e)
        {
            bool onCell = _canvas.TryGetCell(e.mousePosition, out Vector2Int cell);
            bool onWall = _canvas.TryGetWall(e.mousePosition, out WallSide side, out int index);
            switch (e.type)
            {
                case EventType.MouseMove:
                    int hover = onWall ? -1 - (int)side - index : onCell ? 1 + cell.x * 1000 + cell.y : 0;
                    if (hover != _hoverKey)
                    {
                        _hoverKey = hover;
                        Repaint();
                    }

                    break;
                case EventType.MouseDown when e.button == 0:
                    _previewStep = 0;
                    OnPress(onCell, cell, onWall, side, index);
                    e.Use();
                    break;
                case EventType.MouseDrag when e.button == 0:
                    OnDrag(onCell, cell, onWall, side, index);
                    e.Use();
                    break;
                case EventType.MouseUp when e.button == 0:
                    OnRelease();
                    e.Use();
                    break;
            }
        }

        void OnPress(bool onCell, Vector2Int cell, bool onWall, WallSide side, int index)
        {
            int door = onWall ? Draft.DoorAt(side, index) : -1;
            int block = onCell ? Draft.BlockAt(cell) : -1;
            switch (_tool)
            {
                case Tool.Select:
                    Select(block, door);
                    _dragging = block >= 0;
                    _dragFrom = cell;
                    break;
                case Tool.Erase:
                    if (block >= 0)
                        Change("Erase block", () => Draft.Blocks.RemoveAt(block));
                    else if (door >= 0)
                        Change("Erase door", () => Draft.Doors.RemoveAt(door));
                    Select(-1, -1);
                    break;
                case Tool.Block when onCell && _shape >= 0:
                    List<Vector2Int> stamp = Stamp(cell);
                    if (Draft.CanPlace(stamp))
                        Change("Add block", () => Draft.TryAddBlock(Colors[_color], stamp));
                    break;
                case Tool.Block when onCell:
                    _stroke.Clear();
                    _stroke.Add(cell);
                    break;
                case Tool.Door when door >= 0:
                    Select(-1, door);
                    break;
                case Tool.Door when onWall:
                    _doorSide = side;
                    _doorFrom = _doorTo = index;
                    break;
            }
        }

        void OnDrag(bool onCell, Vector2Int cell, bool onWall, WallSide side, int index)
        {
            if (_tool == Tool.Select && _dragging && onCell && cell != _dragFrom)
            {
                Vector2Int delta = cell - _dragFrom;
                int block = _selectedBlock;
                if (TryChange("Move block", () => Draft.TryMoveBlock(block, delta)))
                    _dragFrom = cell;
            }
            else if (_tool == Tool.Block && _shape < 0 && _stroke.Count > 0 && onCell && !_stroke.Contains(cell))
                _stroke.Add(cell);
            else if (_tool == Tool.Door && _doorFrom >= 0 && onWall && side == _doorSide)
                _doorTo = index;
            Repaint();
        }

        void OnRelease()
        {
            if (_tool == Tool.Block && _stroke.Count > 0)
            {
                var cells = new List<Vector2Int>(_stroke);
                if (Draft.CanPlace(cells) && Draft.IsConnected(cells))
                    Change("Paint block", () => Draft.TryAddBlock(Colors[_color], cells));
                _stroke.Clear();
            }

            if (_tool == Tool.Door && _doorFrom >= 0)
            {
                if (CanAddDoor())
                {
                    int start = Mathf.Min(_doorFrom, _doorTo);
                    int length = Mathf.Abs(_doorTo - _doorFrom) + 1;
                    Change("Add door", () => Draft.TryAddDoor(_doorSide, start, length, Colors[_color]));
                    Select(-1, Draft.Doors.Count - 1);
                }

                _doorFrom = _doorTo = -1;
            }

            _dragging = false;
            Repaint();
        }

        bool CanAddDoor()
        {
            int start = Mathf.Min(_doorFrom, _doorTo);
            int end = Mathf.Max(_doorFrom, _doorTo);
            for (int i = start; i <= end; i++)
                if (Draft.DoorAt(_doorSide, i) >= 0)
                    return false;
            return true;
        }

        List<Vector2Int> Stamp(Vector2Int cell)
        {
            BlockShape shape = _shapes.Shapes[_shape];
            for (int i = 0; i < _stampTurns; i++)
                shape = shape.Rotated();
            return shape.At(cell);
        }

        void DrawSolutionPreview()
        {
            var offsets = new Vector2Int[Draft.Blocks.Count];
            var exited = new bool[Draft.Blocks.Count];
            int count = Mathf.Min(_previewStep, Solution.Steps.Count);
            for (int i = 0; i < count; i++)
            {
                SolveStep step = Solution.Steps[i];
                offsets[step.BlockId] = step.Offset;
                exited[step.BlockId] = step.Exits;
            }

            int moving = Solution.Steps[count - 1].BlockId;
            for (int i = 0; i < Draft.Blocks.Count; i++)
                if (!exited[i] || i == moving)
                    _canvas.DrawBlock(Draft.Blocks[i].Cells, offsets[i], BlockTint(Draft.Blocks[i]) * (exited[i] ? 0.45f : 1f),
                        i == moving, BlockLabel(i));
        }

        Color BlockTint(DraftBlock block) =>
            block.Blockers.Ice.IsActive ? Color.Lerp(_canvas.ColorOf(block.Color), IceTint, 0.6f) : _canvas.ColorOf(block.Color);

        string BlockLabel(int index) => index < _blockLabels.Count ? _blockLabels[index] : MakeBlockLabel(index);

        string MakeBlockLabel(int index)
        {
            BlockerSet blockers = Draft.Blocks[index].Blockers;
            string label = (index + 1).ToString();
            if (blockers.Arrow.Axis == ArrowAxis.Horizontal)
                label += " ↔";
            else if (blockers.Arrow.Axis == ArrowAxis.Vertical)
                label += " ↕";
            return blockers.Ice.IsActive ? $"{label} · ice {blockers.Ice.Exits}" : label;
        }

        void DrawInspector(Rect area)
        {
            GUILayout.BeginArea(area, EditorStyles.helpBox);
            _inspectorScroll = GUILayout.BeginScrollView(_inspectorScroll);
            if (_showHelp)
                DrawHelp();
            if (_level >= 0)
            {
                DrawTools();
                DrawLevelProperties();
                DrawSelection();
                DrawChecks();
            }

            DrawGenerator();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawHelp()
        {
            EditorGUILayout.HelpBox(
                "Make a level in 5 steps:\n" +
                "1. Set the board size, timer and difficulty below.\n" +
                "2. Door tool (3): drag along a wall to add a door in the chosen colour.\n" +
                "3. Block tool (2): click to stamp a shape, or pick Free and drag over cells.\n" +
                "4. Fix every red line under Checks; it must say Solvable.\n" +
                "5. Save (Ctrl+S) and press ▶ Play (P) to try it.\n\n" +
                "Or let Generate build one and edit it.\n\n" +
                "A frozen block (Ice above 0 on a selected block) cannot move until that many other blocks have left.\n\n" +
                "Shortcuts: 1 Select · 2 Block · 3 Door · 4 Erase · Q/E colour · Tab shape · R rotate · I / Shift+I ice · A arrow · arrows move · " +
                "Del remove · Esc deselect · Ctrl+Z / Ctrl+Y undo / redo · Ctrl+S save · N new · Shift+D copy · " +
                "C check · Space step the solution · G generate · H make harder · P play · [ ] previous / next level · F1 help",
                MessageType.None);
        }

        void DrawTools()
        {
            Header("Tool");
            int tool = GUILayout.Toolbar(ToolIndex(_tool), _toolLabels);
            _tool = ToolAt(tool);

            Header("Colour (Q / E)");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Colors.Length; i++)
            {
                Rect swatch = GUILayoutUtility.GetRect(26, 26, GUILayout.Width(26));
                EditorGUI.DrawRect(swatch, _canvas.ColorOf(Colors[i]));
                if (i == _color)
                    EditorGUI.DrawRect(new Rect(swatch.x, swatch.yMax - 4, swatch.width, 4), Color.white);
                if (GUI.Button(swatch, new GUIContent("", Colors[i].ToString()), GUIStyle.none))
                    SetColor(i);
            }

            GUILayout.EndHorizontal();

            Header("Shape (Tab, R turns)");
            _shape = GUILayout.SelectionGrid(_shape + 1, _shapeNames, 4) - 1;
        }

        void DrawLevelProperties()
        {
            Header("Level");
            int width = EditorGUILayout.IntSlider(new GUIContent("Width", "Board columns."), Draft.Width, _rules.MinBoardSize, _rules.MaxBoardSize);
            int height = EditorGUILayout.IntSlider(new GUIContent("Height", "Board rows."), Draft.Height, _rules.MinBoardSize, _rules.MaxBoardSize);
            if (width != Draft.Width || height != Draft.Height)
                Resize(width, height);
            int timer = EditorGUILayout.IntSlider(new GUIContent("Timer (s)", "Seconds the player has."), Draft.TimerSeconds,
                _rules.MinTimerSeconds, _rules.MaxTimerSeconds);
            EditorGUILayout.LabelField(" ", $"{timer / 60:00}:{timer % 60:00}");
            if (timer != Draft.TimerSeconds)
                Change("Timer", () => Draft.TimerSeconds = timer);
            var difficulty = (LevelDifficulty)EditorGUILayout.EnumPopup(new GUIContent("Difficulty", "Sets the win reward."), Draft.Difficulty);
            if (difficulty != Draft.Difficulty)
                Change("Difficulty", () => Draft.Difficulty = difficulty);
            EditorGUILayout.LabelField("Win reward", $"{RewardFor(Draft.Difficulty)} coins");
        }

        void DrawSelection()
        {
            if (_selectedBlock >= 0)
            {
                Header($"Block {_selectedBlock + 1}");
                DraftBlock block = Draft.Blocks[_selectedBlock];
                var color = (BlockColor)EditorGUILayout.EnumPopup("Colour", block.Color);
                if (color != block.Color)
                    Change("Recolour block", () => block.Color = color);
                EditorGUILayout.LabelField("Cells", block.Cells.Count.ToString());
                DrawBlockers(block);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Rotate", "Turn a quarter clockwise (R).")))
                    RotateSelected();
                if (GUILayout.Button(new GUIContent("Delete", "Remove the block (Del).")))
                    DeleteSelected();
                GUILayout.EndHorizontal();
            }
            else if (_selectedDoor >= 0)
            {
                Header($"Door on the {Draft.Doors[_selectedDoor].Side} wall");
                DraftDoor door = Draft.Doors[_selectedDoor];
                var color = (BlockColor)EditorGUILayout.EnumPopup("Colour", door.Color);
                if (color != door.Color)
                    Change("Recolour door", () => door.Color = color);
                int wall = Draft.WallLength(door.Side);
                int start = EditorGUILayout.IntSlider(new GUIContent("Start", "First wall cell it covers."), door.Start, 0, wall - 1);
                int length = EditorGUILayout.IntSlider(new GUIContent("Length", "Cells it spans."), door.Length, 1, wall);
                if ((start != door.Start || length != door.Length) && !TryChange("Door span", () => Draft.TrySetDoorSpan(_selectedDoor, start, length)))
                    ShowNotification(new GUIContent("That span leaves the wall or covers another door."));
                if (GUILayout.Button(new GUIContent("Delete", "Remove the door (Del).")))
                    DeleteSelected();
            }
        }

        void DrawChecks()
        {
            Header("Checks");
            if (_checker.Issues.Count == 0)
                EditorGUILayout.HelpBox("Structure: no problems.", MessageType.Info);
            foreach (LevelIssue issue in _checker.Issues)
                EditorGUILayout.HelpBox(issue.Message, issue.Severity == LevelIssueSeverity.Error ? MessageType.Error : MessageType.Warning);

            if (_checker.IsSolving)
            {
                EditorGUILayout.HelpBox("Solving…", MessageType.None);
                return;
            }

            SolveResult solution = Solution;
            if (solution == null)
            {
                EditorGUILayout.HelpBox("Fix the errors above to see whether the level can be finished.", MessageType.None);
                return;
            }

            switch (solution.Status)
            {
                case SolveStatus.Solvable:
                    EditorGUILayout.HelpBox($"Solvable: {solution.Rounds} exit rounds, {solution.Moves} room-making moves " +
                                            $"(score {solution.Score}, plays like {DifficultyFor(solution.Score)}).", MessageType.Info);
                    break;
                case SolveStatus.Unsolvable:
                    EditorGUILayout.HelpBox("Unsolvable: some block can never reach a door of its colour.", MessageType.Error);
                    return;
                default:
                    EditorGUILayout.HelpBox($"Unknown: gave up after {solution.StatesSearched} board states.", MessageType.Warning);
                    return;
            }

            GUILayout.Label("Solution (Space steps through it)", EditorStyles.miniBoldLabel);
            _previewStep = EditorGUILayout.IntSlider("Show step", _previewStep, 0, solution.Steps.Count);
            for (int i = 0; i < solution.Steps.Count; i++)
            {
                SolveStep step = solution.Steps[i];
                string text = step.Exits
                    ? $"{i + 1}. Block {step.BlockId + 1} leaves {Arrow(step.ExitDirection)}"
                    : $"{i + 1}. Block {step.BlockId + 1} makes room";
                GUILayout.Label(text, i + 1 == _previewStep ? EditorStyles.boldLabel : EditorStyles.label);
            }
        }

        void DrawGenerator()
        {
            Header("Generate (G)");
            _genWidth = EditorGUILayout.IntSlider("Width", _genWidth, _rules.MinBoardSize, _rules.MaxBoardSize);
            _genHeight = EditorGUILayout.IntSlider("Height", _genHeight, _rules.MinBoardSize, _rules.MaxBoardSize);
            _genColors = EditorGUILayout.IntSlider(new GUIContent("Colours", "Block colours, one door each."), _genColors, 1, Colors.Length);
            _genBlocks = EditorGUILayout.IntSlider("Blocks", _genBlocks, _genColors, 20);
            _genDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", _genDifficulty);
            GUILayout.BeginHorizontal();
            _genSeed = EditorGUILayout.IntField(new GUIContent("Seed", "The same seed gives the same level."), _genSeed);
            if (GUILayout.Button("Random", GUILayout.Width(60)))
                _genSeed = UnityEngine.Random.Range(1, 1_000_000);
            GUILayout.EndHorizontal();
            using (new EditorGUI.DisabledScope(_generating))
            {
                if (GUILayout.Button(new GUIContent(_generating ? "Working…" : _level >= 0 ? "Generate into this level" : "Generate a new level",
                        "Replaces the canvas with a generated level; undo brings the old one back.")))
                    Generate();
                using (new EditorGUI.DisabledScope(_level < 0))
                    if (GUILayout.Button(new GUIContent("Make harder (H)",
                            "Moves blocks and doors and sets arrows and ice so the level needs more room-making moves; undo brings the old one back.")))
                        Harden();
            }

            if (!string.IsNullOrEmpty(_genMessage))
                EditorGUILayout.HelpBox(_genMessage, MessageType.None);
        }

        void HandleShortcuts(Event e)
        {
            if (e.type != EventType.KeyDown || _level < 0 && e.keyCode != KeyCode.N)
                return;
            bool ctrl = e.control || e.command;
            bool used = true;
            switch (e.keyCode)
            {
                case KeyCode.Alpha1: _tool = Tool.Select; break;
                case KeyCode.Alpha2: _tool = Tool.Block; break;
                case KeyCode.Alpha3: _tool = Tool.Door; break;
                case KeyCode.Alpha4: _tool = Tool.Erase; break;
                case KeyCode.Q: SetColor((_color + Colors.Length - 1) % Colors.Length); break;
                case KeyCode.E: SetColor((_color + 1) % Colors.Length); break;
                case KeyCode.Tab: _shape = (_shape + (e.shift ? _shapes.Shapes.Count : 2)) % (_shapes.Shapes.Count + 1) - 1; break;
                case KeyCode.R: RotateSelected(); break;
                case KeyCode.I: AddIce(e.shift ? -1 : 1); break;
                case KeyCode.A when !ctrl: CycleArrow(); break;
                case KeyCode.LeftArrow: Nudge(Vector2Int.left); break;
                case KeyCode.RightArrow: Nudge(Vector2Int.right); break;
                case KeyCode.UpArrow: Nudge(Vector2Int.up); break;
                case KeyCode.DownArrow: Nudge(Vector2Int.down); break;
                case KeyCode.Delete:
                case KeyCode.Backspace: DeleteSelected(); break;
                case KeyCode.Escape: Select(-1, -1); break;
                case KeyCode.S when ctrl: Save(); break;
                case KeyCode.N when !ctrl: NewLevel(); break;
                case KeyCode.D when e.shift: DuplicateLevel(); break;
                case KeyCode.C when !ctrl: CheckNow(); break;
                case KeyCode.Space: StepPreview(); break;
                case KeyCode.G when !ctrl: Generate(); break;
                case KeyCode.H when !ctrl: Harden(); break;
                case KeyCode.P when !ctrl: PlayTest(); break;
                case KeyCode.LeftBracket: TrySwitchTo(Mathf.Max(0, _level - 1)); break;
                case KeyCode.RightBracket: TrySwitchTo(Mathf.Min(_catalog.Catalog.Levels.Count - 1, _level + 1)); break;
                case KeyCode.F1: _showHelp = !_showHelp; break;
                default: used = false; break;
            }

            if (!used)
                return;
            e.Use();
            Repaint();
        }

        void SetColor(int index)
        {
            _color = index;
            if (_selectedBlock >= 0)
                Change("Recolour block", () => Draft.Blocks[_selectedBlock].Color = Colors[index]);
            else if (_selectedDoor >= 0)
                Change("Recolour door", () => Draft.Doors[_selectedDoor].Color = Colors[index]);
        }

        void RotateSelected()
        {
            if (_selectedBlock >= 0)
            {
                int block = _selectedBlock;
                if (!TryChange("Rotate block", () => Draft.TryRotateBlock(block)))
                    ShowNotification(new GUIContent("No room to turn it here."));
            }
            else
                _stampTurns = (_stampTurns + 1) % 4;
        }

        void DrawBlockers(DraftBlock block)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Blockers", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            int ice = EditorGUILayout.IntSlider(new GUIContent("Ice",
                    "Frozen until this many other blocks have left; 0 = no ice (I / Shift+I)."),
                block.Blockers.Ice.Exits, 0, Mathf.Max(0, Draft.Blocks.Count - 1));
            if (ice != block.Blockers.Ice.Exits)
                Change("Set ice", () => block.Blockers.Ice.Exits = ice);
            var axis = (ArrowAxis)EditorGUILayout.EnumPopup(new GUIContent("Arrow",
                "Moves, and leaves, only along this axis; None = free (A cycles it)."), block.Blockers.Arrow.Axis);
            if (axis != block.Blockers.Arrow.Axis)
                Change("Set arrow", () => block.Blockers.Arrow.Axis = axis);
            EditorGUI.indentLevel--;
        }

        void AddIce(int delta)
        {
            if (_selectedBlock < 0)
                return;
            DraftBlock block = Draft.Blocks[_selectedBlock];
            int ice = Mathf.Clamp(block.Blockers.Ice.Exits + delta, 0, Mathf.Max(0, Draft.Blocks.Count - 1));
            if (ice != block.Blockers.Ice.Exits)
                Change("Set ice", () => block.Blockers.Ice.Exits = ice);
        }

        void CycleArrow()
        {
            if (_selectedBlock < 0)
                return;
            DraftBlock block = Draft.Blocks[_selectedBlock];
            ArrowAxis next = block.Blockers.Arrow.Axis switch
            {
                ArrowAxis.None => ArrowAxis.Horizontal,
                ArrowAxis.Horizontal => ArrowAxis.Vertical,
                _ => ArrowAxis.None,
            };
            Change("Set arrow", () => block.Blockers.Arrow.Axis = next);
        }

        void Nudge(Vector2Int delta)
        {
            int block = _selectedBlock;
            if (block >= 0)
                TryChange("Move block", () => Draft.TryMoveBlock(block, delta));
        }

        void DeleteSelected()
        {
            int block = _selectedBlock;
            int door = _selectedDoor;
            if (block >= 0)
                Change("Delete block", () => Draft.Blocks.RemoveAt(block));
            else if (door >= 0)
                Change("Delete door", () => Draft.Doors.RemoveAt(door));
            Select(-1, -1);
        }

        void Select(int block, int door)
        {
            _selectedBlock = block;
            _selectedDoor = door;
            if (block >= 0)
                _color = Array.IndexOf(Colors, Draft.Blocks[block].Color);
            else if (door >= 0)
                _color = Array.IndexOf(Colors, Draft.Doors[door].Color);
        }

        void ClampSelection()
        {
            if (_selectedBlock >= Draft.Blocks.Count)
                _selectedBlock = -1;
            if (_selectedDoor >= Draft.Doors.Count)
                _selectedDoor = -1;
        }

        void Resize(int width, int height)
        {
            (int blocks, int doors) = Draft.WouldLoseOnResize(width, height);
            if ((blocks > 0 || doors > 0) && !EditorUtility.DisplayDialog("Shrink the board?",
                    $"{blocks} block(s) and {doors} door(s) no longer fit and will be removed.", "Shrink", "Cancel"))
                return;
            Change("Resize board", () => Draft.Resize(width, height));
            ClampSelection();
        }

        void Change(string name, Action change)
        {
            Undo.RecordObject(_state, name);
            change();
            OnDraftChanged();
            Repaint();
        }

        bool TryChange(string name, Func<bool> change)
        {
            string before = JsonUtility.ToJson(Draft);
            Undo.RecordObject(_state, name);
            bool done = change();
            if (done)
                OnDraftChanged();
            else
                JsonUtility.FromJsonOverwrite(before, Draft);
            Repaint();
            return done;
        }

        void Load(int index)
        {
            _level = index;
            Undo.RecordObject(_state, "Open level");
            _state.Draft = LevelDraft.From(_catalog.Catalog.Levels[index].Data);
            Select(-1, -1);
            OnDraftChanged();
            Repaint();
        }

        bool IsDirty => _level >= 0 && _isDirty;

        bool ConfirmLeave()
        {
            if (!IsDirty)
                return true;
            int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", $"Level {_level + 1} has unsaved changes.",
                "Save", "Cancel", "Discard");
            if (choice == 0)
                Save();
            return choice != 1;
        }

        void TrySwitchTo(int index)
        {
            if (index != _level && ConfirmLeave())
                Load(index);
        }

        void Save()
        {
            if (_level < 0)
                return;
            if (_validator.HasErrors(_checker.Issues))
            {
                EditorUtility.DisplayDialog("Cannot save", "Fix the errors under Checks first; the game cannot load this level.", "OK");
                return;
            }

            if (Solution != null && Solution.Status != SolveStatus.Solvable &&
                !EditorUtility.DisplayDialog("Save anyway?", "The solver could not finish this level. Players may get stuck.", "Save", "Cancel"))
                return;
            _catalog.Save(_level, Draft);
            _isDirty = false;
            RefreshLevelList();
            ShowNotification(new GUIContent($"Level {_level + 1} saved"));
        }

        void NewLevel()
        {
            if (!ConfirmLeave())
                return;
            var empty = new LevelData(5, 5, 60, new List<BlockData>(), new List<DoorData>());
            _catalog.Append(empty);
            RefreshLevelList();
            Load(_catalog.Catalog.Levels.Count - 1);
        }

        void DuplicateLevel()
        {
            if (_level < 0 || !ConfirmLeave())
                return;
            _catalog.Duplicate(_level);
            RefreshLevelList();
            Load(_level + 1);
        }

        void DeleteLevel()
        {
            if (!EditorUtility.DisplayDialog("Delete level?", $"Level {_level + 1} and its asset will be deleted.", "Delete", "Cancel"))
                return;
            int deleted = _level;
            _catalog.Delete(deleted);
            RefreshLevelList();
            int count = _catalog.Catalog.Levels.Count;
            _level = -1;
            if (count > 0)
                Load(Mathf.Min(count - 1, deleted));
        }

        void MoveLevel(int by)
        {
            if (!ConfirmLeave())
                return;
            _catalog.Move(_level, _level + by);
            RefreshLevelList();
            Load(_level + by);
        }

        void PlayTest()
        {
            if (_level < 0)
                return;
            Save();
            if (!IsDirty)
                LevelPlayTest.Play(_level + 1);
        }

        void Generate()
        {
            if (!_generating)
                GenerateAsync().Forget();
        }

        async UniTaskVoid GenerateAsync()
        {
            _generating = true;
            _genMessage = "Generating…";
            _generation = new CancellationTokenSource();
            CancellationToken token = _generation.Token;
            var request = new LevelGenerationRequest(_genWidth, _genHeight, _genColors, _genBlocks, _genDifficulty, _genSeed, 300);
            LevelGenerationResult result;
            try
            {
                result = await UniTask.RunOnThreadPool(() => _generator.Generate(request, token), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                _generating = false;
            }

            _generation.Dispose();
            _generation = null;
            ApplyGenerated(result);
            Repaint();
        }

        void Harden()
        {
            if (!_generating && _level >= 0 && !_validator.HasErrors(_checker.Issues))
                HardenAsync().Forget();
        }

        async UniTaskVoid HardenAsync()
        {
            _generating = true;
            _genMessage = "Making it harder…";
            _generation = new CancellationTokenSource();
            CancellationToken token = _generation.Token;
            LevelData level = Draft.ToLevel();
            var request = new LevelHardeningRequest(_genSeed, HardenerRounds, int.MaxValue, true, true);
            LevelHardeningResult result;
            try
            {
                result = await UniTask.RunOnThreadPool(() => _hardener.Harden(level, request, token), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                _generating = false;
            }

            _generation.Dispose();
            _generation = null;
            if (result == null)
                _genMessage = "The solver cannot finish this level within its cap; make it solvable first.";
            else
            {
                Change("Make harder", () => _state.Draft = LevelDraft.From(result.Level));
                _genMessage = $"Now {result.Solution.Moves} room-making moves (score {result.Solution.Score}). Press again to push further.";
                _genSeed++;
                Select(-1, -1);
            }

            Repaint();
        }

        void ApplyGenerated(LevelGenerationResult result)
        {
            if (!result.HasLevel)
            {
                _genMessage = result.Failure;
                return;
            }

            if (_level < 0)
            {
                _catalog.Append(result.Level);
                RefreshLevelList();
                Load(_catalog.Catalog.Levels.Count - 1);
            }
            else
                Change("Generate level", () => _state.Draft = LevelDraft.From(result.Level));
            _genMessage = (result.MatchesDifficulty ? "Generated" : "Closest match (no candidate hit the difficulty exactly)") +
                          $" after {result.Attempts} tries: score {result.Solution.Score}. Edit it, then Save.";
            _genSeed++;
            Select(-1, -1);
        }

        void StepPreview()
        {
            if (Solution == null || Solution.Status != SolveStatus.Solvable)
                return;
            _previewStep = (_previewStep + 1) % (Solution.Steps.Count + 1);
        }

        int RewardFor(LevelDifficulty difficulty)
        {
            foreach (DifficultyReward reward in _rewards.Rewards)
                if (reward.Difficulty == difficulty)
                    return reward.Coins;
            return 0;
        }

        LevelDifficulty DifficultyFor(int score)
        {
            foreach (LevelDifficulty difficulty in Enum.GetValues(typeof(LevelDifficulty)))
            {
                (int min, int max) = _generator.ScoreFor(difficulty);
                if (score >= min && score <= max)
                    return difficulty;
            }

            return LevelDifficulty.Normal;
        }

        static string Arrow(Vector2Int direction) =>
            direction == Vector2Int.left ? "◀ left" : direction == Vector2Int.right ? "▶ right" : direction == Vector2Int.up ? "▲ up" : "▼ down";

        static int ToolIndex(Tool tool) => tool switch { Tool.Select => 0, Tool.Block => 1, Tool.Door => 2, _ => 3 };

        static Tool ToolAt(int index) => index switch { 0 => Tool.Select, 1 => Tool.Block, 2 => Tool.Door, _ => Tool.Erase };

        static void Header(string title)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }
    }
}

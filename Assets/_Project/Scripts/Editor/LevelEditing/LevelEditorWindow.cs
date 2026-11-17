using System;
using System.Collections.Generic;
using ColorBlockJam.Game.Configs;
using ColorBlockJam.Game.Levels;
using ColorBlockJam.Game.Levels.Generation;
using ColorBlockJam.Game.Levels.Solving;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    /// <summary>
    /// The level editor (T-26 – T-31, T-33, T-47; level-editor.md): the catalog on the left, the level drawn
    /// top-down in the middle, tools, properties, checks, the solution and the generator on the right. Every
    /// tool has a shortcut (DA-34). It edits a <see cref="LevelDraft"/> and writes it to the level's asset on
    /// save; the rules come from the game's validator and solver, never a copy.
    /// </summary>
    public sealed class LevelEditorWindow : EditorWindow
    {
        const string CatalogPath = "Assets/_Project/Levels/LevelCatalog.asset";
        const string LevelsFolder = "Assets/_Project/Levels";
        const string RulesPath = "Assets/_Project/Configs/LevelRulesConfig.asset";
        const string ViewConfigPath = "Assets/_Project/Configs/BoardViewConfig.asset";
        const string RewardsPath = "Assets/_Project/Configs/LevelRewardConfig.asset";
        const float ListWidth = 210f;
        const float InspectorWidth = 320f;
        // The generator's own search cap: candidates past it are dropped, not waited on.
        const int GeneratorStateCap = 2000;
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
        LevelSolver _solver;
        BlockShapeLibrary _shapes;
        LevelGenerator _generator;
        LevelCanvas _canvas;
        LevelEditorState _state;

        int _level = -1;
        Tool _tool = Tool.Select;
        int _color;
        // -1 is free painting; 0 and up are the preset shapes.
        int _shape = -1;
        int _stampTurns;
        int _selectedBlock = -1;
        int _selectedDoor = -1;
        bool _showHelp = true;
        Vector2 _listScroll;
        Vector2 _inspectorScroll;

        // Mouse gestures in progress.
        readonly List<Vector2Int> _stroke = new();
        bool _dragging;
        Vector2Int _dragFrom;
        WallSide _doorSide;
        int _doorFrom = -1;
        int _doorTo = -1;

        // Check results, refreshed after every change.
        string _checkedJson;
        IReadOnlyList<LevelIssue> _issues = Array.Empty<LevelIssue>();
        SolveResult _solution;
        int _previewStep;

        // Generator settings.
        int _genWidth = 6;
        int _genHeight = 7;
        int _genColors = 4;
        int _genBlocks = 6;
        LevelDifficulty _genDifficulty = LevelDifficulty.Hard;
        int _genSeed = 1;
        string _genMessage;

        [MenuItem("ColorBlockJam/Level Editor %#l")]
        static void Open() => GetWindow<LevelEditorWindow>("Level Editor").Show();

        LevelDraft Draft => _state.Draft;

        void OnEnable()
        {
            _catalog = new LevelCatalogEditing(AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath), LevelsFolder);
            _rules = AssetDatabase.LoadAssetAtPath<LevelRulesConfig>(RulesPath);
            _rewards = AssetDatabase.LoadAssetAtPath<LevelRewardConfig>(RewardsPath);
            _validator = new LevelDefinitionValidator(_rules);
            _solver = new LevelSolver(_rules.SolverStateCap);
            _shapes = new BlockShapeLibrary();
            _generator = new LevelGenerator(_validator, new LevelSolver(GeneratorStateCap), _shapes);
            _canvas = new LevelCanvas(AssetDatabase.LoadAssetAtPath<BoardViewConfig>(ViewConfigPath));
            _state = CreateInstance<LevelEditorState>();
            _state.hideFlags = HideFlags.HideAndDontSave;
            wantsMouseMove = true;
            Undo.undoRedoPerformed += OnUndoRedo;
            if (_catalog.Catalog.Levels.Count > 0)
                Load(0);
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.ClearUndo(_state);
            DestroyImmediate(_state);
        }

        void OnUndoRedo()
        {
            ClampSelection();
            Repaint();
        }

        // ---------------------------------------------------------------- layout

        void OnGUI()
        {
            HandleShortcuts(Event.current);
            RefreshChecks();
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
            GUILayout.Label(_level >= 0 ? $"Level {_level + 1}{(IsDirty ? " *" : "")}" : "No level", EditorStyles.boldLabel,
                GUILayout.Width(110));
            using (new EditorGUI.DisabledScope(_level < 0))
            {
                if (GUILayout.Button(new GUIContent("Save", "Write the level to its asset (Ctrl+S)."), EditorStyles.toolbarButton))
                    Save();
                if (GUILayout.Button(new GUIContent("Revert", "Drop the unsaved changes."), EditorStyles.toolbarButton))
                    Load(_level);
                if (GUILayout.Button(new GUIContent("Check", "Validate and solve now (C)."), EditorStyles.toolbarButton))
                    RefreshChecks(true);
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
            for (int i = 0; i < levels.Count; i++)
            {
                LevelData data = levels[i].Data;
                string text = $"{i + 1}. {data.Width}x{data.Height} · {data.Blocks.Count} blocks · {data.Difficulty}";
                var style = new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleLeft, fontStyle = i == _level ? FontStyle.Bold : FontStyle.Normal };
                if (GUILayout.Toggle(i == _level, text, style) && i != _level)
                    TrySwitchTo(i);
            }

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

        // ---------------------------------------------------------------- canvas

        void DrawCanvas(Rect area)
        {
            if (_level < 0)
            {
                GUI.Label(area, "Select or create a level.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            _canvas.Layout(new Rect(area.x + 8, area.y + 8, area.width - 16, area.height - 16), Draft.Width, Draft.Height);
            _canvas.DrawBoard(Draft, _selectedDoor);
            if (_previewStep > 0 && _solution != null && _solution.Status == SolveStatus.Solvable)
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
                    Repaint();
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
            int count = Mathf.Min(_previewStep, _solution.Steps.Count);
            for (int i = 0; i < count; i++)
            {
                SolveStep step = _solution.Steps[i];
                offsets[step.BlockId] = step.Offset;
                exited[step.BlockId] = step.Exits;
            }

            int moving = _solution.Steps[count - 1].BlockId;
            for (int i = 0; i < Draft.Blocks.Count; i++)
                if (!exited[i] || i == moving)
                    _canvas.DrawBlock(Draft.Blocks[i].Cells, offsets[i], BlockTint(Draft.Blocks[i]) * (exited[i] ? 0.45f : 1f),
                        i == moving, BlockLabel(i));
        }

        /// <summary>A frozen block is drawn paler, like ice over its colour.</summary>
        Color BlockTint(DraftBlock block) =>
            block.Ice > 0 ? Color.Lerp(_canvas.ColorOf(block.Color), IceTint, 0.6f) : _canvas.ColorOf(block.Color);

        string BlockLabel(int index)
        {
            int ice = Draft.Blocks[index].Ice;
            return ice > 0 ? $"{index + 1} · ice {ice}" : (index + 1).ToString();
        }

        // ---------------------------------------------------------------- inspector

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
                "Shortcuts: 1 Select · 2 Block · 3 Door · 4 Erase · Q/E colour · Tab shape · R rotate · I / Shift+I ice · arrows move · " +
                "Del remove · Esc deselect · Ctrl+Z / Ctrl+Y undo / redo · Ctrl+S save · N new · Shift+D copy · " +
                "C check · Space step the solution · G generate · P play · [ ] previous / next level · F1 help",
                MessageType.None);
        }

        void DrawTools()
        {
            Header("Tool");
            var labels = new[] { new GUIContent("Select", "Pick and drag blocks, pick doors (1)."), new GUIContent("Block", "Stamp or paint blocks (2)."),
                new GUIContent("Door", "Drag along a wall to add a door (3)."), new GUIContent("Erase", "Click a block or door to remove it (4).") };
            int tool = GUILayout.Toolbar(ToolIndex(_tool), labels);
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
            var shapeNames = new List<string> { "Free" };
            foreach (BlockShape shape in _shapes.Shapes)
                shapeNames.Add(shape.Name);
            _shape = GUILayout.SelectionGrid(_shape + 1, shapeNames.ToArray(), 4) - 1;
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
                int ice = EditorGUILayout.IntSlider(new GUIContent("Ice",
                        "Other blocks that must leave before it thaws and can move; 0 = not frozen (I / Shift+I)."),
                    block.Ice, 0, Mathf.Max(0, Draft.Blocks.Count - 1));
                if (ice != block.Ice)
                    Change("Set ice", () => block.Ice = ice);
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
            if (_issues.Count == 0)
                EditorGUILayout.HelpBox("Structure: no problems.", MessageType.Info);
            foreach (LevelIssue issue in _issues)
                EditorGUILayout.HelpBox(issue.Message, issue.Severity == LevelIssueSeverity.Error ? MessageType.Error : MessageType.Warning);

            if (_solution == null)
            {
                EditorGUILayout.HelpBox("Fix the errors above to see whether the level can be finished.", MessageType.None);
                return;
            }

            switch (_solution.Status)
            {
                case SolveStatus.Solvable:
                    EditorGUILayout.HelpBox($"Solvable: {_solution.Rounds} exit rounds, {_solution.Moves} room-making moves " +
                                            $"(score {_solution.Score}, plays like {DifficultyFor(_solution.Score)}).", MessageType.Info);
                    break;
                case SolveStatus.Unsolvable:
                    EditorGUILayout.HelpBox("Unsolvable: some block can never reach a door of its colour.", MessageType.Error);
                    return;
                default:
                    EditorGUILayout.HelpBox($"Unknown: gave up after {_solution.StatesSearched} board states.", MessageType.Warning);
                    return;
            }

            GUILayout.Label("Solution (Space steps through it)", EditorStyles.miniBoldLabel);
            _previewStep = EditorGUILayout.IntSlider("Show step", _previewStep, 0, _solution.Steps.Count);
            for (int i = 0; i < _solution.Steps.Count; i++)
            {
                SolveStep step = _solution.Steps[i];
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
            if (GUILayout.Button(new GUIContent(_level >= 0 ? "Generate into this level" : "Generate a new level",
                    "Replaces the canvas with a generated level; undo brings the old one back.")))
                Generate();
            if (!string.IsNullOrEmpty(_genMessage))
                EditorGUILayout.HelpBox(_genMessage, MessageType.None);
        }

        // ---------------------------------------------------------------- actions

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
                case KeyCode.C when !ctrl: RefreshChecks(true); break;
                case KeyCode.Space: StepPreview(); break;
                case KeyCode.G when !ctrl: Generate(); break;
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

        /// <summary>More or less ice on the selected block, never below 0 or above the other blocks' count.</summary>
        void AddIce(int delta)
        {
            if (_selectedBlock < 0)
                return;
            DraftBlock block = Draft.Blocks[_selectedBlock];
            int ice = Mathf.Clamp(block.Ice + delta, 0, Mathf.Max(0, Draft.Blocks.Count - 1));
            if (ice != block.Ice)
                Change("Set ice", () => block.Ice = ice);
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

        /// <summary>Records undo, then applies the change.</summary>
        void Change(string name, Action change)
        {
            Undo.RecordObject(_state, name);
            change();
            _previewStep = 0;
            Repaint();
        }

        /// <summary>Records undo and applies the change; a refused change leaves nothing to undo.</summary>
        bool TryChange(string name, Func<bool> change)
        {
            string before = JsonUtility.ToJson(Draft);
            Undo.RecordObject(_state, name);
            bool done = change();
            if (!done)
                JsonUtility.FromJsonOverwrite(before, Draft);
            _previewStep = 0;
            Repaint();
            return done;
        }

        void Load(int index)
        {
            _level = index;
            Undo.RecordObject(_state, "Open level");
            _state.Draft = LevelDraft.From(_catalog.Catalog.Levels[index].Data);
            Select(-1, -1);
            _checkedJson = null;
            _previewStep = 0;
            Repaint();
        }

        bool IsDirty => _level >= 0 && JsonUtility.ToJson(Draft.ToLevel()) != JsonUtility.ToJson(_catalog.Catalog.Levels[_level].Data);

        /// <summary>Asks before dropping unsaved changes; true when it is fine to leave this level.</summary>
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
            if (_validator.HasErrors(_validator.Validate(Draft.ToLevel())))
            {
                EditorUtility.DisplayDialog("Cannot save", "Fix the errors under Checks first; the game cannot load this level.", "OK");
                return;
            }

            RefreshChecks(true);
            if (_solution != null && _solution.Status != SolveStatus.Solvable &&
                !EditorUtility.DisplayDialog("Save anyway?", "The solver could not finish this level. Players may get stuck.", "Save", "Cancel"))
                return;
            _catalog.Save(_level, Draft);
            ShowNotification(new GUIContent($"Level {_level + 1} saved"));
        }

        void NewLevel()
        {
            if (!ConfirmLeave())
                return;
            var empty = new LevelData(5, 5, 60, new List<BlockData>(), new List<DoorData>());
            _catalog.Append(empty);
            Load(_catalog.Catalog.Levels.Count - 1);
        }

        void DuplicateLevel()
        {
            if (_level < 0 || !ConfirmLeave())
                return;
            _catalog.Duplicate(_level);
            Load(_level + 1);
        }

        void DeleteLevel()
        {
            if (!EditorUtility.DisplayDialog("Delete level?", $"Level {_level + 1} and its asset will be deleted.", "Delete", "Cancel"))
                return;
            int deleted = _level;
            _catalog.Delete(deleted);
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
            LevelGenerationResult result = _generator.Generate(new LevelGenerationRequest(_genWidth, _genHeight, _genColors,
                _genBlocks, _genDifficulty, _genSeed, 300));
            if (!result.HasLevel)
            {
                _genMessage = result.Failure;
                return;
            }

            if (_level < 0)
            {
                _catalog.Append(result.Level);
                Load(_catalog.Catalog.Levels.Count - 1);
            }
            else
                Change("Generate level", () => _state.Draft = LevelDraft.From(result.Level));
            _genMessage = (result.MatchesDifficulty ? "Generated" : "Closest match (no candidate hit the difficulty exactly)") +
                          $" after {result.Attempts} tries: score {result.Solution.Score}. Edit it, then Save.";
            _genSeed++;
            Select(-1, -1);
            RefreshChecks(true);
        }

        void StepPreview()
        {
            if (_solution == null || _solution.Status != SolveStatus.Solvable)
                return;
            _previewStep = (_previewStep + 1) % (_solution.Steps.Count + 1);
        }

        /// <summary>Re-validates and re-solves when the level changed (or when forced).</summary>
        void RefreshChecks(bool force = false)
        {
            if (_level < 0)
                return;
            LevelData level = Draft.ToLevel();
            string json = JsonUtility.ToJson(level);
            if (!force && json == _checkedJson)
                return;
            _checkedJson = json;
            _issues = _validator.Validate(level);
            _solution = _validator.HasErrors(_issues) ? null : _solver.Solve(level);
            if (_solution == null || _previewStep > _solution.Steps.Count)
                _previewStep = 0;
        }

        // ---------------------------------------------------------------- small helpers

        int RewardFor(LevelDifficulty difficulty)
        {
            foreach (DifficultyReward reward in _rewards.Rewards)
                if (reward.Difficulty == difficulty)
                    return reward.Coins;
            return 0;
        }

        /// <summary>The difficulty whose score band (DA-33) holds <paramref name="score"/>.</summary>
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

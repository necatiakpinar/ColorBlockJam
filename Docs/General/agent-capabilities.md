# Agent Capabilities

Verified facts about what agents can and cannot do in this repository's environment. Read before probing
a tool; update through the `record-agent-capabilities` skill. Never record secrets.

Updated: 2026-11-16

## 1. Unity

- Several Unity editors can run at once; the MCP for Unity server routes to one instance. Check which
  project it serves with `execute_code` → `return Application.dataPath;` before any scene or asset
  operation (2026-11-16).
- DOTween is not on OpenUPM under its official name (`com.demigiant.dotween` returns no package); the
  official zip from `https://dotween.demigiant.com/download.php` is the source (2026-11-16).
- DOTween's setup panel deletes a hand-written `DOTween.Modules.asmdef` when `DOTweenSettings.asset`
  has `createASMDEF: 0`; symptom `CS1929 'CanvasGroup' does not contain a definition for 'DOFade'`.
  Fix: restore the asmdef and set `createASMDEF: 1` (2026-11-16).
- OpenUPM metadata (versions and publish dates) is readable without auth at
  `https://package.openupm.com/<package-name>` (2026-11-16).
- With two editors open, every `unity-mcp` call fails with "Multiple Unity instances are connected and
  none is selected" until `set_active_instance` gets `RollicCaseStudy_NecatiAkpinar@<hash>` (the hash is
  in the error). `refresh_unity` (force, assets) then imports new files and writes their `.meta` (2026-11-16).

- `execute_code` compiles with CodeDom (C# 6): no tuples, `out var` or pattern matching; write
  `UnityEngine.Object` explicitly (`Object` is ambiguous). `AssetDatabase.DeleteAsset` and `File.*` need
  `safety_checks: false` (2026-11-16).
- Test Framework 1.1 (Unity 2022.3) fires `ICallbacks.RunStarted` only after entering Play Mode, so it
  cannot be used to undo a forced `playModeStartScene`; a forced start scene makes Play Mode test runs
  hang (headless) or time out (`run_tests`) (2026-11-16).
- Batch-mode Unity cannot open the project while an editor holds it; MCP `run_tests` works with the
  editor open (2026-11-16).

- `EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) }` can store an all-zero
  GUID for a scene created in the same session, even with the `GUID` constructor. Fix: write the GUID
  through `SerializedObject` on `ProjectSettings/EditorBuildSettings.asset` (`m_Scenes[i].guid.data[0..3]`,
  each 8-hex chunk nibble-reversed, set with `uintValue`) and check the YAML (2026-11-16).
- MCP `run_tests` Play Mode runs work in the open editor once nothing forces a start scene; each test that
  loads Splash must wait for Splash before waiting for Home, or the previous test's Home passes the wait
  (2026-11-16).

- Seeing the game: in Play Mode, `execute_code` with `ScreenCapture.CaptureScreenshot(path)` writes the
  Game view (UI included) a frame later; rendering a camera into a `RenderTexture` and `EncodeToPNG`
  previews models or a scene without UI, also in Edit Mode. Read the PNG to look at it (2026-11-16).
- DOTween: `Tweener.ChangeEndValue(object, bool)` boxes a `Vector3` on every call; keep the
  `TweenerCore<Vector3, Vector3, VectorOptions>` from `DOLocalMove` to reach the typed overload
  (found by `Is.Not.AllocatingGCMemory`) (2026-11-16).

## 2. Shell (Windows, Git Bash)

- Long multi-file `cat <<'EOF'` heredoc chains in one Bash call can fail with "unexpected EOF while
  looking for matching `'`"; write files with the editor tool instead (2026-11-16). This also hits a
  single `python - <<'EOF'` whose body holds many quotes: write the script to the scratchpad and run it
  with `python -I <file> <args>` (2026-11-16).
- The Read tool cannot render PDF pages here (`pdftoppm` is missing). Text extraction works with
  `python -m pip install pypdf` then `pypdf.PdfReader(path).pages[i].extract_text()`; `python -I` hides the
  user-site install (so does `-s`), so run plain `python` from the scratchpad. `strings` is not installed; read binary names
  with a Python regex instead (2026-11-16).

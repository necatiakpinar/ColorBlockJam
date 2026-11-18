# Agent Capabilities

Verified facts about what agents can and cannot do in this repository's environment. Read before probing
a tool; update through the `record-agent-capabilities` skill. Never record secrets.

Updated: 2026-11-18

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
  none is selected" until `set_active_instance` gets `<project folder name>@<hash>` (the hash is
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
- URP renderer features can be added from `execute_code`: create the feature with
  `ScriptableObject.CreateInstance`, `AssetDatabase.AddObjectToAsset` it to the renderer data, add it to
  `rendererFeatures`, then fill `m_RendererFeatureMap` with each feature's local file id
  (`AssetDatabase.TryGetGUIDAndLocalFileIdentifier`) through `SerializedObject`. URP 14's `RenderObjects`
  lives in `UnityEngine.Experimental.Rendering.Universal` and defaults to `overrideMode: Material` (2026-11-17).
- Unity overwrites a hand-edited `ProjectSettings/*.asset` value with its in-memory copy on the next save
  while the editor is open; change such values through `SerializedObject` in `execute_code` (2026-11-17).
- MCP `manage_camera` `screenshot` (no camera) captures the Game view in Play Mode and returns the image
  inline; give `output_folder: "Temp/Screenshots"` so nothing lands under `Assets/` (2026-11-17).
- MCP `run_tests` and `manage_editor play` leave `ProjectSettings/EditorSettings.asset` with
  `m_EnterPlayModeOptions: 1`; restore it with `git checkout HEAD -- ProjectSettings/EditorSettings.asset`
  before committing (2026-11-17).
- Splitting one file's edits across commits: build the staged text from `git show :<path>` plus only the
  wanted hunks, then `git hash-object -w --no-filters --stdin` and `git update-index --cacheinfo` (the
  index stores LF) (2026-11-17).

- Platform-only code (`#if UNITY_ANDROID && !UNITY_EDITOR`) does not compile in the editor; check it with
  `PlayerBuildInterface.CompilePlayerScripts` (target Android) from `execute_code`, a few seconds. The iOS
  build module is not installed on this machine, so iOS-only code cannot be compiled here (2026-11-17).
- `ProjectSettings.asset` `projectName` keeps coming back as the old cloud project name after Unity saves;
  set it through `SerializedObject` and check the YAML before committing (2026-11-17).
- `manage_camera screenshot` renders the camera only, without Screen Space – Overlay UI; to see the HUD,
  call `ScreenCapture.CaptureScreenshot("Temp/Screenshots/x.png")` from `execute_code` in play mode and
  read the file a moment later (2026-11-17).
- `TMP_Text.SetCharArray` allocates in the editor only (it mirrors the chars into `text` under
  `#if UNITY_EDITOR`), so `Is.Not.AllocatingGCMemory()` cannot prove a TMP label update allocation-free
  (2026-11-17).
- Never capture an editor window with `InternalEditorUtility.ReadScreenPixel`: it reads the desktop, so a
  window behind another app returns that app's pixels. Capture the window's own buffer instead: get the
  `m_Parent` field of the `EditorWindow` by reflection, call `RepaintImmediately` (non-public) on the window,
  then `GrabPixels(RenderTexture, Rect)` on the parent and `ReadPixels`; the image comes out upside down
  and, in this Linear project, too light (raising each channel to the power 2.2 gives a close look)
  (2026-11-17).
- MCP `generate_audio` (fal.ai) answers `configured: false` until a fal key is entered in the editor's
  MCP for Unity Asset Generation tab; only the developer can enter it (2026-11-17).
- `execute_code`: `??` on a Unity component returns the destroyed-or-missing "fake null" and the next call
  throws `MissingComponentException`; test `== null` explicitly (2026-11-17).
- `refresh_unity` with `scope: scripts` does not import a new `.cs` file (`CS0246` on its type); use
  `scope: all` after adding files (2026-11-17).

- MCP `manage_packages` `search_packages` / `add_package` run as jobs; poll with `status` and the `job_id`.
  On Unity 2022.3.62f2 the registry offers `com.unity.addressables` 1.29.0; the add finished in about 10 s
  (2026-11-18).
- `AddressableAssetSettingsDefaultObject.GetSettings(true)` from `execute_code` creates
  `Assets/AddressableAssetsData/` (`Built In Data` and `Default Local Group`) and adds the
  `com.unity.addressableassets` config object to `ProjectSettings/EditorBuildSettings.asset` (2026-11-18).
- Addressables 1.29 logs an unknown key as `LogType.Error` (`InvalidKeyException`), not `LogType.Exception`;
  a test that provokes it needs `LogAssert.Expect(LogType.Error, ...)` (2026-11-18).
- UniTask 2.5.11: a `Preserve()`d task still pending accepts only one awaiter; a second throws "Already
  continuation registered, can not await twice". Share one pending result through a
  `UniTaskCompletionSource<T>` instead (2026-11-18).
- `AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult)` from `execute_code` (CodeDom;
  Roslyn is not installed) built the Android content in 201 s on the first run and froze the editor meanwhile.
  It leaves `Assets/AddressableAssetsData/Android/addressables_content_state.bin` (gitignored) and an
  untracked `Android.meta`; delete that folder after a test build (2026-11-18).
- Changing a serialized `AudioClip`/`TextAsset` field to `AssetReferenceT<T>` keeps the old YAML until the asset
  is saved; migrate by writing each old GUID to `<field>.m_AssetGUID` through `SerializedObject` right after the
  compile, then check the `.asset` diff (2026-11-18).

- The Sprite Editor (nine-slice borders) needs `com.unity.2d.sprite`; without it Unity shows "No Sprite
  Editor Window registered". From code, set `TextureImporter.spriteBorder` (x left, y bottom, z right,
  w top) and `SaveAndReimport` (2026-11-18).
- `ScreenCapture.CaptureScreenshot` with a relative path wrote nothing here; pass
  `System.IO.Path.GetFullPath("Temp/Screenshots/x.png")` and wait a second before reading it (2026-11-18).
- `PrefabUtility.ConvertToPrefabInstance(go, prefabAsset, settings, InteractionMode.AutomatedAction)` (2022.3)
  turns matching scene objects into instances of a prefab and keeps every reference to them; the differences
  (icon sprite, an enum) become overrides. For a prefab nested in another prefab instance,
  `GetCorrespondingObjectFromSource` returns the outer prefab's copy; `GetCorrespondingObjectFromOriginalSource`
  returns the nested prefab (2026-11-18).
- `RestartResetsStateTests.Restart_ShouldLeaveTheSameStateAsAFreshStart_WhenRepeatedTenTimes` depends on the
  saved level: it drags the first block one cell right, so it fails on a level where that block cannot move
  (seen on levels 33 and 36, with and without the change under test) (2026-11-18).

## 2. Shell (Windows, Git Bash)

- Long multi-file `cat <<'EOF'` heredoc chains in one Bash call can fail with "unexpected EOF while
  looking for matching `'`"; write files with the editor tool instead (2026-11-16). This also hits a
  single `python - <<'EOF'` whose body holds many quotes: write the script to the scratchpad and run it
  with `python -I <file> <args>` (2026-11-16).
- The Read tool cannot render PDF pages here (`pdftoppm` is missing). Text extraction works with
  `python -m pip install pypdf` then `pypdf.PdfReader(path).pages[i].extract_text()`; `python -I` hides the
  user-site install (so does `-s`), so run plain `python` from the scratchpad. `strings` is not installed; read binary names
  with a Python regex instead (2026-11-16).

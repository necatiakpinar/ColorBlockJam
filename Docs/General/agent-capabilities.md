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

## 2. Shell (Windows, Git Bash)

- Long multi-file `cat <<'EOF'` heredoc chains in one Bash call can fail with "unexpected EOF while
  looking for matching `'`"; write files with the editor tool instead (2026-11-16).
- The Read tool cannot render PDF pages here (`pdftoppm` is missing). Text extraction works with
  `python -m pip install pypdf` then `pypdf.PdfReader(path).pages[i].extract_text()`; `python -I` hides the
  user-site install (so does `-s`), so run plain `python` from the scratchpad. `strings` is not installed; read binary names
  with a Python regex instead (2026-11-16).

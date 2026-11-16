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

## 2. Shell (Windows, Git Bash)

- Long multi-file `cat <<'EOF'` heredoc chains in one Bash call can fail with "unexpected EOF while
  looking for matching `'`"; write files with the editor tool instead (2026-11-16).

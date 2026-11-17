# Debug Tools

> **Binding agent rules.** Read this page before touching `Scripts/Game/Debugging/` or
> `Scripts/Editor/Debugging/`, or before adding a debug shortcut.

Two kinds of checking keep the game honest: the **automated test suite** (Edit Mode and Play Mode,
[testing.md](testing.md)) and the **debug window** for checking by hand without playing a level to the end.

## 1. Debug window

**ColorBlockJam > Debug Window** (`Ctrl+Shift+D`, `GameDebugWindow`) is the game's own debug panel, in the
spirit of SRDebugger but ours.

| Section | Shortcuts |
| --- | --- |
| Level | state readout (level, state, time left, coins); **Win now**, **Run out of time**, **Restart** |
| Timer | set the seconds left; 3 s, 10 s, 30 s left; +30 s |
| Coins | add or spend any amount; +100, +1000, +10000 |
| Levels | play any level number; next level |
| Saves (edit mode) | clear the player's saves: progress, coins, settings |
| Log | a text area listing what was done, and why a shortcut was refused |

The level shortcuts need the Gameplay scene in play mode; outside it the window says so. A refused shortcut
(winning a paused level, spending more coins than the player has) changes nothing and is logged.

## 2. How it works

- **`GameplayDebugCommands`** (Game assembly, Gameplay scope) holds the shortcuts. Each one goes through the
  service that owns the change, so the game reacts exactly as it does in play: **Win now** announces the
  board as cleared (the reward is paid, the win popup opens), **Run out of time** empties the timer and
  fails the session (the out-of-time popup opens), coins go through `IWalletService`, levels through
  `IPlayerProgressService.SetCurrentLevel` and a scene reload, the timer through `LevelTimer.SetSecondsLeft`.
- **`GameDebugWindow`** (Editor assembly) only finds the Gameplay scope (`LifetimeScope.Find`), resolves the
  commands, calls them and repaints the readout a few times a second. It owns no game logic.
- Because the shortcuts live in the Game assembly, an in-game panel for device builds can call the same
  `GameplayDebugCommands` later.

## 3. Adding a shortcut

1. Add a method to `GameplayDebugCommands` that calls the owning service; refuse (return false) when the
   game is in a state where it would not happen in play.
2. Test it in `GameplayDebugCommandsTests`.
3. Add its button to `GameDebugWindow` and its row to §1.

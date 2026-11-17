# Lives

> **Binding agent rules.** Read this page before touching `Scripts/Game/Lives/`, `LevelAttemptLives`,
> `GameplayLivesFlow`, the Home lives bar or the lives popup.

The player holds up to five lives (T-48, DA-38). Playing a level needs a life; leaving a started attempt
costs one; a missing life comes back after 30 minutes of trusted time; coins fill them all at once.

## 1. The rules

| Event | Lives |
| --- | --- |
| New player | full (5) |
| A level is won | none taken |
| An attempt the player has started (first grab) is given up (out of time, then close), restarted, or left for Home | one taken, once per attempt; Home asks first ("Leave Level?", `LeaveLevelPopup`) |
| An attempt is left before the first grab | none taken |
| 30 minutes of trusted time after a life went (one countdown at a time) | one back, up to the max |
| Refill (100 coins) | every missing life back |
| Play with no lives (Home level button, Retry, restart) | the lives popup opens instead |

## 2. The parts

- **`LivesService`** (Root scope, `lives_v1`, [persistence.md](persistence.md)) saves how many lives are
  missing and when the next one comes back, in **trusted UTC ticks** (`ITrustedClock`, [network.md](network.md)):
  the device clock never counts (R-44), and the countdown keeps running while the app is closed. A life lost
  before the trusted time is known starts its countdown once it is. `TryLoseOne`, `TryRefill` (pays through
  `IWalletService`), `Refresh` (gives back what the time owes), `SecondsToNextLife`. Every change is saved
  at once and fires `LivesChangedSignal(count, max, secondsToNextLife)`.
- **`LivesClock`** (Root entry point) ticks once a second through `IGameClock` delays while a life is missing:
  it refreshes and announces, so every view's countdown moves.
- **`LivesConfig`** (Root, `Configs/LivesConfig.asset`): max lives, refill minutes, refill price.
- **`LevelAttemptLives`** (Gameplay scope) knows whether the attempt has started (`BlockGrabbedSignal`) and
  was settled (won, or its life already taken); `Abandon` takes the life once.
- **`GameplayLivesFlow`** (Gameplay): `PlayAgain` restarts with a life left, or pauses and opens the lives
  popup; Refill restarts, close goes Home. `GameplayFlow` (restart, pause → Home) and `FailFlow` (give up,
  Retry) call `Abandon` and `PlayAgain`. Pause → Home asks first when `LevelAttemptLives.WouldCostLife`:
  the leave popup's Leave takes the life and goes Home, its close goes back to the pause popup.
- **`HomeFlow`**: the level button plays only with a life; otherwise, like the lives bar's "+", it opens the
  lives popup.

## 3. On screen

| Piece | Where | Shows |
| --- | --- | --- |
| `LivesBarView` | Home `TopBar/Lives` | the lives in the heart; "Full" or the `mm:ss` to the next life; "+" opens the popup |
| `OutOfLivesPopupView` | `OutOfLivesPopup.prefab`, in both `HomePopupConfig` and `GameplayPopupConfig` | "Out of Lives" with none left, "Lives" otherwise; the heart with the count, "Next life in mm:ss" (or "Your lives are full!"), Refill with the price, disabled when full or short of coins |

Countdowns are written into char buffers (`TMP_Text.SetCharArray`), so a tick allocates nothing on device.

## 4. Tests

`LivesServiceTests`, `LevelAttemptLivesTests`, `HomeFlowTests`, `GameplayFlowTests`, `LevelResultFlowTests`
(Edit Mode). Manual checks: [lives-qa.md](../testing/lives-qa.md).

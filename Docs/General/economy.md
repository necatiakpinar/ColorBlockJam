# Economy

> **Binding agent rules.** Read this page before touching `Scripts/Game/Economy/`, `LevelRewardConfig`
> or a level's difficulty.

## 1. Earning coins

Coins are earned only by winning a level (DA-7). The amount depends on the level's difficulty:

| `LevelDifficulty` | Coins per win |
| --- | --- |
| `Normal` | 10 |
| `Hard` | 25 |
| `VeryHard` | 50 |

- Each level carries its `Difficulty` in its data ([level-data.md](level-data.md) §1). The shipped
  levels are 1–2 Normal, 3–4 Hard, 5 Very Hard (`ShippedLevelsTests` keeps the difficulty from falling).
- The table lives in `LevelRewardConfig` (`Assets/_Project/Configs/LevelRewardConfig.asset`, Gameplay
  scope). Nothing reads it directly: **`LevelRewardProvider`** maps a level, or a difficulty, to its coins,
  the same way `LevelProvider` maps a level number to its data. A difficulty without an entry throws:
  that is a config bug, not a zero reward.

## 2. Paying a win

**`LevelRewardService`** (plain class, Gameplay scope, built by the scope's build callback) adds the
reward to `IWalletService` on `LevelCompletedSignal`, before anything presents the win (T-16):

- It looks the level up through `LevelProvider`, so a looped level number pays its layout's reward.
- The session announces a win once per attempt, so a win pays once.
- Failing, restarting and leaving a level never touch the wallet.

The wallet saves at once ([persistence.md](persistence.md)), so the balance survives leaving and restarting.
The HUD and Home coin counters bind `CoinsChangedSignal` with T-18 and T-23.

## 3. Spending coins

Coins buy three things, each paid with `IWalletService.TrySpend` before anything is given:

- more time after the timer runs out: `GameplayConfig.ContinueSeconds` (20) for `ContinueCost` (50) coins
  (DA-30). `LevelContinueService` pays, then `LevelSession.Continue` lets the level play on
  ([gameplay.md](gameplay.md) §7);
- one booster when none are left: Freeze 40, Rocket 60, Hammer 80, Vacuum 120 coins (`BoosterConfig`,
  DA-36) through `BoosterShop` ([boosters.md](boosters.md) §1);
- every missing life at once: 100 coins (`LivesConfig`, DA-38) through `LivesService.TryRefill`
  ([lives.md](lives.md)).

## 4. Tests

`LevelRewardTests` cover the table, a missing entry, the shipped config rising with difficulty, paying
by difficulty and on a looped level, and nothing on a loss or after disposal. `CoinPersistenceTests`
(Play Mode) win in the real scene, then leave and come back.

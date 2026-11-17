# Persistence

> **Binding agent rules.** Read this page before touching `Scripts/Core/Persistence/` or adding anything
> the player keeps between sessions.

## 1. What is saved

Everything the player keeps lives on the device in `IKeyValueStorage` (`PlayerPrefsStorage`; tests use
`InMemoryKeyValueStorage`). Each save is one JSON document under one versioned key, read and written by
a `JsonRecord<T>`.

| Service (Root scope) | Key | Data | New player |
| --- | --- | --- | --- |
| `IPlayerProgressService` | `player_progress_v1` | `PlayerProgressData`: current level (1-based, DA-2) | level 1 |
| `IWalletService` | `wallet_v1` | `WalletData`: coins | 0 coins (DA-7) |
| `ISettingsService` | `settings_v1` | `SettingsData`: music, sound, vibration toggles (`GameSetting`) | all on (DA-10) |

## 2. Rules

1. **Load once, at boot.** `LoadPlayerDataStep` (Splash) loads all three before Home shows anything.
   Reading a service before its `Load` throws, so a missing load is a loud setup bug.
2. **Save on every change.** Each change writes its record and flushes at once, so a killed app keeps it.
   Nothing waits for the app to go to the background, so there is no save-on-pause observer.
3. **Announce after saving.** `CoinsChangedSignal(coins, delta)` and `SettingChangedSignal(setting, isOn)`
   fire after the record is written. Setting a toggle to the value it has does nothing.
4. **Never trust stored data.** A missing record starts fresh. Corrupt JSON starts fresh and logs an error.
   Values that fail the service's check (a level below 1, a negative balance) start fresh and log a warning.
5. **Reject bad changes.** `IWalletService.Add` and `TrySpend` throw on zero or fewer coins. `TrySpend`
   returns false and changes nothing when the player has too few; a spend announces a negative delta.
6. **Versioned keys.** A change to a record's shape that old saves cannot read gets a new key (`_v2`) and
   a migration from the old one in that service's `Load`.

## 3. Tests

`PlayerProgressServiceTests`, `WalletServiceTests` and `SettingsServiceTests` cover fresh starts,
saving, reloading, corrupt and invalid saves, and the signals. `CoinPersistenceTests` (Play Mode) checks
the balance survives leaving a level and coming back.

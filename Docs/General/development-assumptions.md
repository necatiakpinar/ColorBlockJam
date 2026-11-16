# Development Assumptions

> **Binding agent rules.** A player-visible decision the case brief does not settle is registered here
> before it ships, and its id (`DA-n`) is cited in the code comment and the commit footer.

Each entry: the decision, why it was needed, what would change if the reviewer decides otherwise, and
its status (`Open`, `Confirmed <date>`, `Withdrawn <date>`).

| Id | Decision | Why | If decided otherwise | Status |
| --- | --- | --- | --- | --- |
| DA-1 | Portrait orientation, 1080x1920 canvas reference | Mobile casual default; the brief has not arrived | Change Player orientation and every CanvasScaler | Open |
| DA-2 | Level numbers are 1-based and persist locally (PlayerPrefs) | Needed to show "Level N" before the brief | Swap `IKeyValueStorage` implementation; `PlayerProgressService` unchanged | Open |
| DA-3 | Splash stays up at least 1.5 s | Avoids a flash on fast devices | Tune `SplashConfig.MinimumDisplaySeconds` | Open |

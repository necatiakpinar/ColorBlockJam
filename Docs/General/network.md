# Network

> **Binding agent rules.** Read this page before touching `Scripts/Core/Network/`, `NetworkConfig` or
> anything that reads the time of day.

The game is online only (R-43, [Design §2](../Design/README.md#2-scope-decisions) item 9). This page covers
how it decides it is online and where trusted time comes from.

## 1. Connectivity

**`ConnectivityService`** (Core, Root scope, `IConnectivityService`) decides online or offline by real
requests. `Application.internetReachability` only reports a network interface, so Wi-Fi without internet
would pass it.

- **Probe:** `IProbeRequest` sends one `GET` to `NetworkConfig.Connectivity.ProbeUrl`. Any 2xx answer is
  online. No connection, a timeout (`ProbeTimeout`, 5 s) or an error status is offline
  (`UnityWebRequestProbe`). The URL is `https://www.google.com/generate_204` until the game has its own
  server (DA-25). A probe carries no player data.
- **Loop:** the Root scope starts the loop when it is built. It probes every `OnlineInterval` (10 s) while
  online and every `OfflineInterval` (3 s) while offline. It probes at once when the app comes back from
  the background (`ApplicationPausedSignal`) and on `ProbeNowAsync` (Retry), which completes with that
  probe's answer. Waits are `IGameClock` delays; nothing runs per frame.
- **Signal:** `ConnectivityChangedSignal(isOnline)` fires on the first answer and on every change after,
  never twice for the same state. `IsOnline` is false and `HasProbed` false until the first answer.
- Each change is logged once (`Connectivity: online` / `offline`).

## 2. Trusted time

**`TrustedClock`** (Core, Root scope, `ITrustedClock`) is the time of day for wall-clock features such as
lives refill and daily rewards (R-44). It never reads the device clock, so moving the device clock changes
nothing.

- Every successful probe hands its HTTP `Date` header (RFC 1123, one-second precision) to
  `TrustedClock.TryRecord`. A missing or unparsable header keeps the last time.
- `TryGetUtcNow` returns that server time plus the monotonic time (`IGameClock.Now`) since it arrived. It
  returns false until the first successful probe; features that need it wait (DA-27).
- The level timer does not use it. It counts monotonic real time, which the device clock does not move
  (R-45, gameplay.md §6 Timer).

## 3. Tests

`ConnectivityServiceTests` answer probes by hand (`FakeProbeRequest`) on a `FakeGameClock`.
`TrustedClockTests` cover the date parse and the monotonic advance. The real request is checked by hand
([network-qa.md](../testing/network-qa.md)).

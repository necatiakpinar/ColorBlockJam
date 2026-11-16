# Pre-Case Readiness

What the skeleton still lacks before the case brief arrives, ranked by how much a reviewer would notice
it against how long it takes. Nothing here is gameplay: the case decides that. Each item is generic
enough that almost any mobile casual brief will use it.

Status: proposal (2026-11-16). Pick items, then each becomes a task in [Case/README.md](../Case/README.md).

Legend — **Impact**: what a reviewer gains from it · **Cost**: S < 1 h, M 1–3 h, L > 3 h.

## 1. Gaps in what exists today

These are small defects or loose ends in the committed skeleton. Fix first.

| # | Gap | Why it matters | Fix | Cost |
| --- | --- | --- | --- | --- |
| 1.1 | Player identity is template default: `companyName: DefaultCompany`, bundle id `com.UnityTechnologies.com.unity.template.urpblank` | First thing visible in a build; looks unfinished | Set company, product name, `com.<you>.rolliccase`, version `0.1.0` | S |
| 1.2 | Android build settings unchecked: scripting backend, ARM64, min/target API | A reviewer may build an APK; Mono/ARMv7 looks careless | IL2CPP, ARM64 only, min API 24+, target = latest | S |
| 1.3 | `PC_RPAsset` / `PC_Renderer` and the PC quality level are still there | Dead assets in a mobile-only project | Keep Mobile only, point every quality level to it | S |
| 1.4 | `InputSystem_Actions.inputactions` is the untouched template asset | Unused template content | Replace with the project's own actions (see 2.1) or delete | S |
| 1.5 | DOTween runs with `useSafeMode: true` | Safe mode swallows exceptions inside tweens, hiding bugs the rules want to surface | Safe mode off in editor and development builds, on in release | S |
| 1.6 | Canvases do not respect the safe area yet, although `CLAUDE.md` requires it | A rule the code doesn't follow is worse than no rule | `SafeAreaFitter` component on each canvas root + test | S |
| 1.7 | `CLAUDE.md` bans `Camera.main` but offers no replacement | Rule without a tool | `CameraService` registered per scene scope from a serialized camera | S |
| 1.8 | No test enforces the rules (no `Update`, Core ↛ Game, no `Debug.Log`) | Rules are only as good as their enforcement | Architecture tests (see 3.1) | M |

## 2. Foundations almost every casual brief needs

| # | Item | What it is | Impact | Cost |
| --- | --- | --- | --- | --- |
| 2.1 | **Input service** | `IInputService` over the Input System firing tap / drag start / drag / release as events (no polling); UI-over-world blocking; a fake for tests | Every hyper/hybrid-casual game is touch-driven; showing event-driven input fits the "no Update" rule | M |
| 2.2 | **Level lifecycle** | Level state machine `Loading → Playing → Won/Lost`, signals `LevelLoaded/Completed/Failed`, `ILevelService` that commits progress *before* the result is shown, restart and next-level routes | The case will almost certainly be level based; this is the spine features hang on | M |
| 2.3 | **Level data** | `LevelDefinition` ScriptableObject + `LevelCatalog`, loop or randomize after the last level | Data-driven levels are what publishers expect | S |
| 2.4 | **Screen / popup navigation** | `IScreenService` that opens authored prefabs (win, fail, settings), one at a time, with input lock while open | Win/fail screens are guaranteed in any brief | M |
| 2.5 | **Object pooling** | Generic pool built on `IObjectResolver.Instantiate` so pooled objects get injection | GC rules in `CLAUDE.md` demand it; gameplay spawns will need it | S |
| 2.6 | **App lifecycle** | Save on `OnApplicationPause/Focus`, pause gameplay time when backgrounded | Classic mobile bug reviewers check | S |
| 2.7 | **Audio + haptics** | `IAudioService` (SFX by id from a config, pooled sources), `IHapticService` (light/medium/heavy), both behind settings toggles | Rollic games lean heavily on feel; haptics are cheap and expected | M |
| 2.8 | **Settings** | Sound / haptics toggles persisted through `IKeyValueStorage`, settings popup | Small, but completes the "real game" shell | S |

## 3. Quality signals for reviewers

| # | Item | What it is | Impact | Cost |
| --- | --- | --- | --- | --- |
| 3.1 | **Architecture tests** | Reflection tests: no `Update`/`LateUpdate`/`FixedUpdate`/`ITickable` in `RollicCase.*`; Core does not reference Game; every `IBootStep` weight > 0; every `MonoBehaviour` serialized reference in scenes assigned | Turns written rules into enforced rules — strong senior signal | M |
| 3.2 | **CI on GitHub Actions** | GameCI `unity-test-runner` for Edit + Play Mode on every push/PR; a light job validating skills and `CLAUDE.md`==`AGENTS.md` | Green badge on the README; proves tests actually run | M (needs a Unity license secret) |
| 3.3 | **Code coverage** | `com.unity.testtools.codecoverage`, report % for Core and Game | A number reviewers can read | S |
| 3.4 | **Scene integrity tests** | Load each build scene in Edit Mode, assert its scope exists and every serialized field is set | Catches broken references before Play | S |
| 3.5 | **Debug tools (dev builds only)** | Behind a `DEV_TOOLS` define: skip / restart level, FPS counter, reset progress | Speeds up testing; shows awareness of build hygiene | S |
| 3.6 | **Performance baseline** | Profiler check of Gameplay at 0 B GC per frame, a note in docs with the numbers, `ProfilerMarker`s on hot paths | The GC rule becomes a measured fact | S |
| 3.7 | **PR template** | `.github/pull_request_template.md`: summary, task id, tests, screenshots | Only matters if the repo is shared on GitHub | S |

## 4. Publisher awareness (Rollic is a publisher)

| # | Item | What it is | Impact | Cost |
| --- | --- | --- | --- | --- |
| 4.1 | **Analytics seam** | `IAnalyticsService` with a logging implementation; level start / complete / fail events with event names as constants | Shows you know publishers live on these events, without adding an SDK | S |
| 4.2 | **Ads seam (optional)** | `IAdService` stub (interstitial between levels, rewarded continue) with a fake that completes instantly | Only if the brief hints at monetization; otherwise skip | S |
| 4.3 | **Remote-tunable configs** | Note in docs on how configs would be swapped by remote config; no SDK | Low cost talking point | S |

## 5. Delivery

| # | Item | What it is | Cost |
| --- | --- | --- | --- |
| 5.1 | **Submission checklist** | Android APK (and/or WebGL) build, README "how to play / how to run", known issues, time spent | S |
| 5.2 | **Gameplay GIF/video in README** | Recorded with Unity Recorder at the end | S |
| 5.3 | **Decision log** | Short list of trade-offs made under time pressure (lives in `Case/README.md` §2) | S |

## 6. Recommendation

Do now, before the brief (≈ 1 working day): **1.1–1.8, 2.1, 2.2, 2.3, 2.4, 2.6, 3.1, 3.4**.
They are needed by nearly any brief and make the existing rules true.

Prepare but keep minimal: **2.5, 2.7, 2.8, 4.1** — small, generic, easy to wire when the case needs them.

Decide with the brief: **3.2** (needs a license secret and a hosted repo), **4.2**, **5.x**.

Avoid before the brief: Addressables, localization, any third-party SDK, gameplay-specific systems.

## 7. Questions for the developer

1. Will the repository be shared as a GitHub link (then CI + PR template are worth it) or as a zip?
2. Target platform for the submitted build: Android APK, iOS, WebGL, or editor only?
3. Should DA-1 (portrait, 1080x1920) stand, or wait for the brief?
4. Is there a time budget for the case (hours/days)? It decides how much of §2 to build up front.

# Audio

> **Binding agent rules.** Read this page before touching `Scripts/Game/Audio/`, `AudioConfig`, a sound
> clip, `GameplaySounds`, `ButtonTapSounds` or `tools/synth_sfx.py`.

The game plays short sound effects (T-53, DA-43) and a low-time tick (T-54, DA-42). There is no music yet;
the music toggle only stores its state (DA-10).

## 1. The parts

| Part | Scope | Does |
| --- | --- | --- |
| `IAudioService` / `AudioService` | Root (`CoreServicesInstaller`) | `LoadAsync` (boot, `LoadAudioStep`) loads every cue's clip through the Root `AssetLoader`; `Play(AudioCue)` plays the cue's clip at its volume, or nothing when the sound toggle is off (`ISettingsService`); playing before the load throws |
| `AudioConfig` | Root (`ConfigProvider`), `Configs/AudioConfig.asset` | one entry per `AudioCue`: an addressable clip (`AssetReferenceT<AudioClip>`) and a volume; a cue without an entry throws |
| `IAudioOutput` / `SfxPlayer` | Root prefab child `Audio` | one 2D `AudioSource` (`PlayOneShot`, so effects overlap) and the session's only `AudioListener` |
| `GameplaySounds` | Gameplay (`GameplayInstaller`, built by the scope's build callback) | grab, exit, booster used, win, fail from their signals; the tick on every `TimerChangedSignal` in the low-time range |
| `ButtonTapSounds` | Home `HomeCanvas`, Gameplay `HudCanvas` | on `Start` adds the tap cue to every `Button` under the canvas, popups included (they are built under it before `Start`) |

## 2. Cues

| Cue | Plays on | Clip |
| --- | --- | --- |
| `Tap` | any button click | `sfx_tap` |
| `Grab` | `BlockGrabbedSignal` | `sfx_grab` |
| `Exit` | `BlockExitedSignal` | `sfx_exit` |
| `Booster` | `BoosterUsedSignal` | `sfx_booster` |
| `Win` | `LevelCompletedSignal` | `sfx_win` |
| `Fail` | `LevelFailedSignal` | `sfx_fail` |
| `Tick` | each second from `GameplayConfig` low time (10 s) down to 1 s | `sfx_tick` |

## 3. Rules

1. Play a sound only through `IAudioService`; never add an `AudioSource` or `AudioListener` to a scene. The
   listener lives on the Root prefab so every scene has exactly one.
2. A new cue gets an `AudioCue` value, an `AudioConfig` entry and an addressable clip
   ([addressables.md](addressables.md) §7); `AudioServiceTests` fails when the shipped config misses a cue
   and `AddressablesSetupTests` when the clip is not in the `Audio` group.
3. Sounds react to signals or button clicks; game rules never wait on a sound.
4. Playing allocates nothing: `PlayOneShot` on one source, the cue found by a loop over a short list.

## 4. Clips

The clips under `Assets/_Project/Audio/` are synthesized by `tools/synth_sfx.py` (sine, triangle and noise
partials with short envelopes; 22 050 Hz, 16-bit mono WAV), so they are license-free and reproducible.
Change a recipe and run `python tools/synth_sfx.py` from the repository root to replace them. Import: force
to mono, Decompress On Load, ADPCM ([art-pipeline.md](art-pipeline.md)).

## 5. Tests

`AudioServiceTests`, `GameplaySoundsTests`, `GameplayHudViewTests` (low-time flash), `InstallerResolutionTests`
(Edit Mode). Manual checks: [audio-qa.md](../testing/audio-qa.md).

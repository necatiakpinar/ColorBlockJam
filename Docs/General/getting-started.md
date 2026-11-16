# Getting Started

## 1. Requirements

- Unity **2022.3.62f2** with Android and/or iOS build support.
- Git with **Git LFS** (`git lfs install` once per machine).

## 2. Open the project

1. Clone, then open the repository root in Unity Hub.
2. Packages resolve automatically. Third-party sources:

| Package | Version | Source |
| --- | --- | --- |
| VContainer (`jp.hadashikick.vcontainer`) | 1.19.0 | OpenUPM scoped registry |
| UniTask (`com.cysharp.unitask`) | 2.5.11 | OpenUPM scoped registry |
| DOTween (free) | 1.3.045 | vendored in `Assets/Plugins/Demigiant/DOTween` |

3. DOTween's modules compile through the committed `DOTween.Modules.asmdef`.
   `Assets/Resources/DOTweenSettings.asset` keeps `createASMDEF: 1`: if it is 0, running DOTween's
   setup panel **deletes** that asmdef and every `DO*` UI extension (e.g. `CanvasGroup.DOFade`) stops
   compiling.
4. `UNITASK_DOTWEEN_SUPPORT` is defined for Android, iOS and Standalone in Player Settings; it enables
   `await tween.ToUniTask()`.

## 3. Editor tooling

| Menu | Does |
| --- | --- |
| **RollicCase > Play From Splash** | (default on) Play always starts from build scene 0, whatever scene is open; test runs and batch mode are exempt |
| **Window > General > Test Runner** | Edit Mode / Play Mode tests |

## 4. Android build

Player Settings are already set: bundle id `com.necatiakpinar.rolliccase`, portrait only, IL2CPP,
ARM64, min API 24, target API = highest installed, one Mobile quality level on `Mobile_RPAsset`.
`BuildSettingsTests` guards orientation and backend.

1. **File > Build Settings > Android > Switch Platform** (first switch reimports textures as ASTC).
2. Tick **Development Build** for a test build; **Build** writes the APK.

## 5. Troubleshooting

| Symptom | Fix |
| --- | --- |
| `DOTween.Modules` missing or duplicate | keep exactly one `DOTween.Modules.asmdef`, in `Plugins/Demigiant/DOTween/Modules` |
| `ToUniTask` not found on a tween | the define `UNITASK_DOTWEEN_SUPPORT` is missing for the active platform |
| Play Mode tests never start in the editor | `EditorSceneManager.playModeStartScene` is set; `PlayModeStartScene` clears it on returning to Edit Mode, so stop Play Mode once |
| Scene scope has no parent | `VContainerSettings` lost its Root prefab reference, or is missing from Preloaded Assets |

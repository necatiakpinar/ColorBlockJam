# Addressables QA

Verifies [addressables.md](../General/addressables.md). `⚙` = automated (test named), `👁` = manual.

## Test harness

- **Window > Asset Management > Addressables > Event Viewer** (enable **Send Profiler Events** in the settings
  asset) shows live handles and their reference counts.
- Device checks use an Android build; Addressables content builds with the player.

## Checklist

### Loader

- [x] ⚙ Repeat and concurrent loads share one backend load; the last release frees it — `AssetLoaderTests`
- [x] ⚙ A cancelled holder and a failed load leave nothing held; a release mid-load is refused — `AssetLoaderTests`
- [x] ⚙ Each scene scope gets its own loader, disposed with the scene — `InstallerResolutionTests`

### Real source

- [x] ⚙ The local catalog initializes without errors — `AddressablesAssetSourceTests`
- [x] ⚙ The boot step initializes the source — `InitializeAssetsStepTests`
- [x] ⚙ An unknown key throws — `AddressablesAssetSourceTests`

### Content (addressables.md §7)

- [x] ⚙ The atlas, every cue clip and the bundled pack are addressable; the atlas is not built into the player —
  `AddressablesSetupTests`
- [x] ⚙ Sounds play only after they load at boot; the bundled pack is released once read — `AudioServiceTests`,
  `LevelPackServiceTests`
- [x] ⚙ The real boot loads the clips and the pack and reaches Home — `NavigationFlowTests`
- [ ] 👁 On device, every UI sprite (Home, Gameplay, popups) shows from the first frame of Home, with no blank
  frame while the atlas binds.
- [ ] 👁 On device, every sound effect plays (tap, grab, exit, booster, win, fail, tick).
- [ ] 👁 On device, level 1 plays the bundled pack's first level.
- [ ] 👁 In the editor, the Event Viewer shows the seven clips held for the session and `Levels/BundledPack`
  released after boot.

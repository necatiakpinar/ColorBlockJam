# Addressables QA

Verifies [addressables.md](../General/addressables.md). `⚙` = automated (test named), `👁` = manual.

## Test harness

- **Window > Asset Management > Addressables > Event Viewer** (enable **Send Profiler Events** in the settings
  asset) shows live handles and their reference counts.
- No shipped asset is addressable yet. To check a real load, mark a small asset addressable as `QA/Probe`,
  load it through the Root scope's `AssetLoader`, then undo the change.

## Checklist

### Loader

- [x] ⚙ Repeat and concurrent loads share one backend load; the last release frees it — `AssetLoaderTests`
- [x] ⚙ A cancelled holder and a failed load leave nothing held; a release mid-load is refused — `AssetLoaderTests`
- [x] ⚙ Each scene scope gets its own loader, disposed with the scene — `InstallerResolutionTests`

### Real source

- [x] ⚙ The local catalog initializes without errors — `AddressablesAssetSourceTests`
- [x] ⚙ The boot step initializes the source — `InitializeAssetsStepTests`
- [x] ⚙ An unknown key throws — `AddressablesAssetSourceTests`
- [ ] 👁 In the editor, loading `QA/Probe` returns the asset; releasing it drops its handle in the Event Viewer.

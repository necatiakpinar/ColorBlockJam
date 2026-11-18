# Addressables

> **Binding agent rules.** Read this page before touching `Scripts/Core/Assets/`, `InitializeAssetsStep`,
> `Assets/AddressableAssetsData/` or code that loads an asset at runtime.

The game loads runtime assets through Unity's Addressables package (`com.unity.addressables` 1.29.0). Game
code never calls the `Addressables` API itself: it asks an `AssetLoader`, which shares loads, counts holders
and releases what its scope loaded.

## 1. Classes

All of them live in `Scripts/Core/Assets/` (namespace `ColorBlockJam.Core.Assets`).

| Type | Role | Registered |
| --- | --- | --- |
| `IAssetSource` | the seam to the asset backend: `InitializeAsync`, `LoadAsync<T>(key)` returning an `IAssetHandle` | Root, singleton |
| `AddressablesAssetSource` | the only production source; wraps `Addressables.InitializeAsync`, `LoadAssetAsync<T>` and releases a failed handle before rethrowing | as `IAssetSource` |
| `IAssetHandle` | one loaded asset: `Asset` and `Release()` | not registered |
| `AddressablesAssetHandle<T>` | holds one `AsyncOperationHandle<T>`; `Release` calls `Addressables.Release` | not registered |
| `AssetLoader` | what game code injects: `LoadAsync<T>`, `Release<T>`, by address string or `AssetReferenceT<T>` | Root, **scoped** |
| `InitializeAssetsStep` (Game/Boot) | first boot step; initializes the source before any other step can load | Splash, `IBootStep` |

```
consumer ──LoadAsync<T>(key)──▶ AssetLoader ──(first holder only)──▶ IAssetSource ──▶ Addressables
          ◀── T ───────────────  lease: holders, handle  ◀── IAssetHandle ──────────
consumer ──Release<T>(key)───▶ AssetLoader ──(last holder only)──▶ IAssetHandle.Release()
scope ends ──Dispose()──────▶ AssetLoader ──▶ releases every handle it still holds
```

## 2. Rules

1. **Inject `AssetLoader`** to load an asset at runtime. Never call `Addressables.*`, `Resources.Load` or
   `AssetDatabase` from runtime code; only `AddressablesAssetSource` and `AddressablesAssetHandle<T>` touch the
   Addressables API. A serialized reference in a config, scene or prefab is still fine for an asset that must
   always be in memory.
2. **One loader per scope.** `AssetLoader` is `Lifetime.Scoped`: the Root scope and each scene scope resolve
   their own instance, and VContainer disposes it with its scope. Everything a scene loaded is released when
   that scene unloads, even if a consumer forgot to (`InstallerResolutionTests`). A session-long asset is
   loaded by a Root service through the Root loader.
3. **Pair every load with a release** of the same type and key, unless the asset lives as long as the scope.
   Release only after your load returned: releasing something the loader does not hold, or a load still
   running, throws `InvalidOperationException`. Cancel the load's token to abandon it in flight.
4. **A load is keyed by address and type.** Loading the same key twice (also concurrently) costs one backend
   load; the same key as two types is two loads.
5. **Cancellation.** `AssetLoader.LoadAsync` takes a `CancellationToken` (the scope's token by default). A
   cancelled caller gives up its hold at once; the backend load still finishes and is released if nobody else
   holds it. `IAssetSource.LoadAsync` takes no token on purpose: one backend load is shared by every holder, so
   no single caller may cancel it.
6. **Failure surfaces.** A failed load throws to every waiting caller and is forgotten, so the next call retries.
   No caller gets `null` back. Addressables also logs the failure itself as an error (an unknown key logs
   `InvalidKeyException`), so a Play Mode test that provokes one expects that log. A load that fails after its
   last holder cancelled has no caller left; UniTask reports it as an unobserved exception.
7. **References over strings.** Configs reference addressable assets with `AssetReferenceT<T>` fields; string
   addresses are for assets found by convention. An empty reference throws `ArgumentException`.
8. Instantiating prefabs: load the prefab through `AssetLoader`, then `Instantiate` it (or hand it to a pool);
   keep the prefab loaded while instances exist.

## 3. Boot

`InitializeAssetsStep` (weight 1) runs first in `SplashInstaller`, so the catalog is ready before levels or
player data load. The catalog is local, so the step does not wait on the network
([splash-boot.md](splash-boot.md) §1, guarantee 3). A remote catalog makes it a step that can block, which
needs a timeout (splash-boot.md §3, item 4).

## 4. Project settings

| Setting | Value | Why |
| --- | --- | --- |
| Settings asset | `Assets/AddressableAssetsData/AddressableAssetSettings.asset` | created by the package; commit the whole folder with its `.meta` files |
| Groups | `UI`, `Audio`, `Levels`, the empty `Default Local Group` and the package's `Built In Data`; all local build and load paths | no remote content yet |
| `EditorBuildSettings` config object | `com.unity.addressableassets` points at the settings asset | how the editor finds the settings; keep it |
| Build Addressables on Player Build | Build With Player | a player build always carries current content |
| Include GUIDs in Catalog (each group) | on | `AssetReferenceT<T>` loads by GUID; turning it off breaks every reference |
| Play Mode Script (editor) | Use Asset Database | per-machine editor state, not committed; the package default, so no content build is needed to press Play |

Make an asset addressable in its Inspector (**Addressable** toggle) or **Window > Asset Management >
Addressables > Groups**. Give it a short address only when code loads it by string.

Before testing a device build with **Use Existing Build**, run **Build > New Build > Default Build Script** in
the Groups window.

## 5. Adding a consumer

1. Add an `AssetReferenceT<T>` field to the area's `<Area>Config` (architecture.md §5) and assign the asset.
   Every assembly that uses `AssetLoader` or `AssetReferenceT<T>` references `Unity.Addressables`.
2. Inject `AssetLoader` into the service or view that needs it.
3. `await loader.LoadAsync(config.Reference, token)`; release when done, or let the scope release it.
4. In Edit Mode tests, build an `AssetLoader` over `Tests/EditMode/Helpers/FakeAssetSource`, which serves assets
   from a dictionary, can hold a load open (`Hold`) and records loads and releases.

## 6. Tests

`AssetLoaderTests` and `InitializeAssetsStepTests` (Edit Mode, over `FakeAssetSource`) cover the loader and
the boot step; `AddressablesAssetSourceTests` (Play Mode) runs the real source against the project's catalog;
`InstallerResolutionTests` pins the registrations. What each pins is listed in [testing.md](testing.md) §2;
the manual checks are in [addressables-qa.md](../testing/addressables-qa.md).


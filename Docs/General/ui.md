# UI

> **Binding agent rules.** Read this page before adding or changing a canvas, a button, a popup or
> anything under `Scripts/Game/UI/`.

`SceneSetupTests` opens every build scene and enforces §1 and §2.

## 1. Canvases

1. UI is authored in scenes or prefabs, never built in code; views bind serialized references.
2. Every root canvas has a `CanvasScaler` with **Scale With Screen Size, 1080x1920, match 0.5**.
3. Every root canvas has a **`SafeArea`** child (stretched, `SafeAreaFitter`) holding all interactive and
   informative content. Full-screen backgrounds sit beside it, outside the safe area, so they still fill
   a notched screen.
4. `SafeAreaFitter` re-fits only when the canvas resizes (`OnRectTransformDimensionsChange`), never per
   frame. The anchor math is `SafeArea` (`SafeAreaTests`).
5. Canvases are Screen Space – Overlay. A scene with buttons has one `EventSystem` with
   `InputSystemUIInputModule`. A scene that takes touches on 3D objects adds a `PhysicsRaycaster` to its
   camera; Overlay UI still wins the EventSystem raycast (`BoardInputTests`).

## 2. Buttons

1. Every `Button` carries **`ButtonPressFeedback`** with `_config`, `_button` (itself) and `_target`
   (usually its own transform, uniformly scaled) set. Pressing scales the target by `UiConfig.PressFeedback.PressedScale`,
   releasing springs it back to its authored scale; a non-interactable button gives no feedback; a
   button disabled mid-press comes back at full size (`ButtonPressFeedbackTests`).
2. `ButtonPressFeedback` reads `UiConfig` from a serialized asset reference, the one exception to "configs
   come through the container": it exists on every button, has no service dependency, and registering
   each button with the scope would add wiring without value.
3. A view exposes taps as C# events (`HomeView.LevelButtonClicked`) wired to `Button.onClick` in its
   `[Inject] Construct`; the flow that owns routing subscribes in its constructor and unsubscribes in
   `Dispose`. Views never navigate or change data.
4. Placeholder buttons (R-12, R-14: visual feedback, no function) get `ButtonPressFeedback` and no listener.

## 3. Popups

1. Popups open and close only through the scene's **`IPopupService`** (registered by the Home and
   Gameplay installers). One popup at a time: opening a second one throws `InvalidOperationException`,
   because the open popup's blocker makes that a wiring bug (`PopupServiceTests`).
2. A popup is an authored view implementing `IPopup`, inactive until shown, whose root is a full-screen
   blocker. Every popup derives from **`PopupView`**: its root holds a `CanvasGroup`, a dimming `Blocker`
   and a `Panel`; showing activates the root and plays `FadeScaleInAnimation` (`UiConfig.PopupReveal`),
   hiding deactivates it at once. A framed popup uses `bg_popup` for the panel, `bg_popup_header` with the
   title, `Popup_Close_Button` at the top right and `bg_popup_info_holder` for inner panels.
   - **Popups are prefabs** under `Assets/_Project/Prefabs/UI/Popups/`, saved inactive with a full-screen
     root. The scenes hold none: `GameplayPopupConfig` and `HomePopupConfig` list the prefabs and each scope
     creates them under its `Popups` layer with `RegisterComponentInNewPrefab` and injects it. Change a
     popup by editing its prefab (`PopupPrefabTests` checks the config, the inactive root, press
     feedback on every button and auto-sized texts).
   - **Texts fit their box:** every label stretches over the area it belongs to (anchors, not a fixed
     size) and auto-sizes between a minimum and a maximum, so nothing spills out of its button or panel.
   - **Sprite padding:** `bg_popup` has transparent margins (50 units left and right, 19 on top, 37 below),
     so a panel's rect is its visible size plus (100, 56). The button and holder sprites have almost none.
3. A popup view only reports taps as C# events; a flow decides what they do (§2.3).

## 4. Configs

| Asset | Holds |
| --- | --- |
| `UiConfig` (Root) | button press feedback, popup reveal |
| `GameplayPopupConfig` (Gameplay) | the gameplay popup prefabs |
| `HomePopupConfig` (Home) | the Home popup prefabs (the Home settings variant) |

## 5. Gameplay HUD and popups

The HUD is authored in `Gameplay.unity` under `HudCanvas`; the popups are prefabs (§3). Both are laid out
after the reference screens, sizes measured on them and scaled to the canvas (DA-28):

| Part | Art | Behaviour |
| --- | --- | --- |
| `SafeArea/TopBar/LevelBadge` | `Btn_booster_normal`, "Level" and the number | `LevelStartedSignal` |
| `TopBar/TimePanel` | `bg_popup`, `ic_time`, `bg_gameplay_time_holder` | `TimerChangedSignal`, `mm:ss` |
| `TopBar/RestartButton`, `PauseButton` | `Btn_square_retry` + `ic_restart`, `Btn_settings_normal` + `ic_pause` | restart at once; pause opens the pause popup |
| `TopBar/CoinCounter` | `bg_home_coin_holder`, `Coin_Single`, `Btn_circle_green_small` + `ic_plus` | balance from `IWalletService` and `CoinsChangedSignal` ("1.28k" from 1000); "+" is a placeholder |
| `SafeArea/BoosterBar` | `Btn_booster_normal` with `Clock_Freeze`, `Rocket_Purple_Icon`, `Hammer`, `Vacuum`, count badge `Btn_circle_blue_small`, buy badge `Btn_circle_green_small` + `ic_plus` | `BoosterBarView`: stock from `BoosterInventory`; taps use or offer a booster ([boosters.md](boosters.md)) |
| `SafeArea/BoosterBanner`, `IceFrame`, `TopBar/TimePanel/FreezeMeter` | title and hint text; `icetexture` strips; `bg_gameplay_time_holder` + `SquareRoundedMask` fill + `Clock_Freeze` | booster targeting and freeze looks ([boosters.md](boosters.md) §4) |
| `PausePopup` prefab | "SETTINGS": vibration, sound and music switches, Legal Terms, Restore Purchases, Support, Language, Home | switches show and set `ISettingsService`; the four blue buttons are placeholders (DA-29) |
| `WinPopup` prefab | "LEVEL COMPLETE", a pile of `Coin_Single`, Continue with the reward | Continue plays the next level (DA-12) |
| `OutOfTimePopup` prefab | "Out of Time!", `Clock`, "+20", Continue with the price | buys more time; close gives up (DA-30) |
| `LevelFailedPopup` prefab | "Level Failed!" with `Heart`, Retry | Retry restarts; close goes Home (DA-31) |
| `BoosterPurchasePopup` prefab | the booster's title, icon, "x1", description, Buy with the price | buys one booster; close resumes (DA-36) |

Text uses `LiberationSans SDF` with the `LiberationSans SDF - Hud` material (outline and drop shadow) until
the game has its own font. A switch's "off" look is a dark track with a plain knob, because the art has
only the "on" knob (asset-feedback.md A-1).

## 6. Settings popup

One layout, two scenes. **`SettingsPopup.prefab`** is the base: panel, header, the vibration, sound and
music switches, and the Legal Terms, Restore Purchases, Support and Language placeholders, laid out after
the Home reference. It carries no view component; each scene uses a **prefab variant** of it:

| Variant | View | Adds |
| --- | --- | --- |
| `HomeSettingsPopup` (`HomePopupConfig`) | `SettingsPopupView` | nothing: close returns to Home |
| `GameplaySettingsPopup` (`GameplayPopupConfig`, the pause popup) | `PausePopupView : SettingsPopupView` | a taller panel and Home; close resumes |

A change to the shared layout goes in the base prefab and reaches both. `SettingsPopupView` shows the saved
switches (`Refresh` before opening, `SettingChangedSignal` after) and reports taps; the flow sets
`ISettingsService` (DA-10). `PopupPrefabTests` checks both are variants of the base.

## 7. Home screen

Authored in `Home.unity` under `HomeCanvas`, laid out after the reference (T-23):

| Part | Art | Behaviour |
| --- | --- | --- |
| `Background` | `bg_home_screen` | fills the screen, outside the safe area |
| `TopBar/ProfileButton` | `profile_box_blue` | placeholder |
| `TopBar/Lives` | `bg_home_resource_holder`, `Heart`, "Full" and 5, green "+" | placeholder (DA-9) |
| `TopBar/Coins` | `bg_home_resource_holder`, `Coin_Single`, green "+" | balance (`CoinAmountText`); "+" is a placeholder |
| `TopBar/SettingsButton` | `Settings_Icon` | opens the Home settings popup |
| `LevelPath` | gold rails, `Btn_level_green` tile for the current level, `Btn_settings_normal` + `lock` for the next two | numbers from the saved level |
| `LevelPath/LevelButton` | `Btn_level_green` (nine-sliced, art-pipeline.md §4), "Level N" | plays the current level |
| `TabBar` | `bg_menu`, `bg_home_line` separators, `ActiveTab` under Home, `lock`, `shop`, `home_icon`, `block_collection`, `lock` "Lvl 50" | placeholders: press feedback only; Home stays active (DA-11) |

The reference's chef portrait and its skull marker for hard levels have no art in the project, so the
profile frame stays empty and locked tiles all look alike. `HomeFlow` shows the level and the balance on
start and routes the level and settings buttons (T-24).

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
     root. The scenes hold none: `GameplayPopupConfig` and `HomePopupConfig` (in the `ConfigProvider`,
     [architecture.md](architecture.md) §5) list the prefabs and each scope creates them under its `Popups` layer with `RegisterComponentInNewPrefab` and injects it. Change a
     popup by editing its prefab (`PopupPrefabTests` checks the config, the inactive root, press
     feedback on every button and auto-sized texts).
   - **Texts fit their box:** every label stretches over the area it belongs to (anchors, not a fixed
     size) and auto-sizes between a minimum and a maximum, so nothing spills out of its button or panel.
   - **Sprite padding:** `bg_popup` has transparent margins (50 units left and right, 19 on top, 37 below),
     so a panel's rect is its visible size plus (100, 56). Its inner face ends 95 units above the rect's bottom (margin plus rim): content centred in a panel centres between the header and that line. The button and holder sprites have almost none.
3. A popup view only reports taps as C# events; a flow decides what they do (§2.3).

## 4. Text styles

Every text in every scene and UI prefab, the board's ice counts included, is drawn in one font and one of two
styles (`TextStyleTests`):

| Style | Material | Look | Used for |
| --- | --- | --- | --- |
| Outlined | `LilitaOne SDF - Outlined` | chunky face (dilate 0.12), thick navy outline (0.24), navy drop shadow just below | every text on blue or coloured art: titles, HUD, buttons, badges, popups, the ice counts |
| Plain | `LilitaOne SDF - Plain` | chunky face, no outline | dark text on light art: the Home lives and coin bars, the splash subtitle |

- **Font:** `Art/Fonts/LilitaOne/LilitaOne SDF` (static SDF atlas built from `LilitaOne-Regular.ttf`, Lilita
  One by Juan Montoreano, SIL Open Font License 1.1, `OFL.txt` beside it). It covers Basic Latin and Latin-1;
  letters it lacks (Turkish ğ ş İ, ₺) fall back to `LiberationSans SDF`.
- **Default:** `TMP Settings` makes it TextMeshPro's default font, so a new text starts in the game font; pick
  one of the two materials as its preset.
- **Colour** is the text's vertex colour (white by default, gold for a title, navy on a light bar); the
  outline and shadow come from the style. Lilita is already bold: leave Font Style at Normal.
- **A new style** is a new material made from the font asset, added to `TextStyleTests.Styles` and this table.

## 5. Configs

| Asset | Holds |
| --- | --- |
| `UiConfig` (Root) | button press feedback, popup reveal, scene fade out and in, the Home level pulse, the atlas address |
| `GameplayPopupConfig` (Gameplay) | the gameplay popup prefabs |
| `HomePopupConfig` (Home) | the Home popup prefabs (the Home settings variant) |

## 6. Gameplay HUD and popups

The HUD is authored under `HudCanvas` in `Gameplay.unity` from prefabs in `Prefabs/UI/Hud/`: `TopBar.prefab`,
`BoosterBar.prefab`, and the booster buttons in `Hud/Boosters/`. `BoosterButton.prefab` is their base (frame, empty icon slot,
count and buy badges); `FreezeBoosterButton`, `RocketBoosterButton`, `HammerBoosterButton` and
`VacuumBoosterButton` are its variants, each setting only its icon and `BoosterType`. `BoosterBar` places one
of each and overrides nothing but their slot. A change to every booster button goes in the base; a new
booster is a new variant. Change the HUD in its prefabs; the popups are prefabs too (§3). Both are laid out
after the reference screens, sizes measured on them and scaled to the canvas (DA-28):

| Part | Art | Behaviour |
| --- | --- | --- |
| `SafeArea/TopBar/TimePanel/LevelBadge` | no art of its own: "Level" and the number over the bar's circle | `LevelStartedSignal` |
| `TopBar/TimePanel` | `Btn_levelBg_normal` (circle and patterned bar in one sprite, nine-sliced), `ic_time`, `bg_gameplay_time_holder` | `TimerChangedSignal`, `mm:ss` |
| `TopBar/SideButtons/RestartButton`, `PauseButton` | `Btn_square_retry` + `ic_restart`, `Btn_settings_normal` + `ic_pause` | restart at once; pause opens the pause popup |
| `TopBar/CoinCounter` | `bg_home_coin_holder`, `Coin_Single`, `Btn_circle_green_small` + `ic_plus` | balance from `IWalletService` and `CoinsChangedSignal` ("1.28k" from 1000); "+" is a placeholder |
| `SafeArea/BannerArea` | none: an empty strip 180 units tall at the bottom | kept free for an ad banner (DA-48); nothing else may sit in it |
| `SafeArea/BoosterBar` | `Btn_booster_normal` with `Clock_Freeze`, `Rocket_Purple_Icon`, `Hammer`, `Vacuum`, count badge `Btn_circle_blue_small`, buy badge `Btn_circle_green_small` + `ic_plus` | `BoosterBarView`: stock from `BoosterInventory`; taps use or offer a booster ([boosters.md](boosters.md)) |
| `SafeArea/BoosterBanner`, `IceFrame`, `TopBar/TimePanel/TimeArea/FreezeMeter` | title and hint text; `icetexture` strips; `bg_gameplay_time_holder` track + `SquareRoundedMask` fill, both nine-sliced with a pixels-per-unit multiplier (2.6, 4) so their round ends fit the 44 px bar, the fill shrinking by its right anchor (`BarFillAnimation`), + `Clock_Freeze` | booster targeting and freeze looks ([boosters.md](boosters.md) §4) |
| `PausePopup` prefab | "SETTINGS": vibration, sound and music switches, Legal Terms, Restore Purchases, Support, Language, Home | switches show and set `ISettingsService`; the four blue buttons are placeholders (DA-29) |
| `WinPopup` prefab | "LEVEL COMPLETE", a pile of `Coin_Single` (`_coinPile`, the flight's start), Continue with the reward | Continue flies the coins to the HUD counter and goes Home (DA-12, T-51; [gameplay.md](gameplay.md) §7) |
| `LevelFailedPopup` prefab | "Level Failed!" with `Heart` and "-1"; Retry (`Btn_popup_green`) above Return to Menu (`Btn_popup_blue`), centred as a pair in the panel below the header | opens when the timer runs out; Retry restarts, or opens the lives popup with none left; Return to Menu and close go Home (DA-31) |
| `LeaveLevelPopup` prefab | "Leave Level?", `Heart` with "-1", "You will lose a life!", Leave | Leave takes the life and goes Home; close goes back to pause (DA-38) |
| `OutOfLivesPopup` prefab (Home and Gameplay) | "Out of Lives", `Heart` with the count, the next life's countdown, Refill with the price | fills every life for coins; in Gameplay close goes Home (DA-38) |
| `BoosterPurchasePopup` prefab | the booster's title, icon, "x1", description, Buy with the price | buys one booster; close resumes (DA-36) |

**Top bar layout** (`HudLayoutTests`):

1. **Anchors follow the areas.** Each group is anchored where it sits: `TimePanel` and its bar to the top-left
   corner, `SideButtons` (restart above pause, as tall as the bar) and `CoinCounter` to the top-right corner, the
   four boosters to the quarter points of `BoosterBar`. Everything inside a group stretches over its own area
   (anchors on its corners, no offsets): `LevelBadge` over the bar's circle, `TimeArea` over the bar right of
   the circle with `TimeIcon`, `TimeTitle`, `TimeSlot` and `FreezeMeter` inside it, a button's icon and badges
   over the button. Resizing a group moves and scales its contents with it; no part sits on a fixed point.
2. `TimePanel` draws `Btn_levelBg_normal` at 788x290 from the top bar's corner, sliced at the sprite's own
   scale (pixels-per-unit multiplier = sprite height / panel height), so neither the circle nor the pattern
   stretches. `LevelBadge` covers the circle; its title (`ic_time` and "Time", centred together) and its
   `TimeSlot` sit in one column centred in the bar right of the circle.
3. `RestartButton` and `PauseButton` stack in `SideButtons` beside the panel: the restart's top lines up with the bar's
   visible top and the pause's bottom with its visible bottom (the sprite has 31 px of margin above the
   bar and 30 below).
4. In `CoinCounter`, `Coin_Single` sits inside the holder's left end and the green "+" inside its right end,
   both slightly smaller than the holder's height; the balance is centred between them.
5. Every green "+" (the coin counters, the booster buy badge, the Home lives bar) is an instance of
   `Prefabs/UI/Common/PlusButton.prefab`: `Btn_circle_green_small` with `ic_plus` stretched over 1.25 times
   the button (the sprite carries about 42 px of transparent margin on each side, so the cross fills a little
   over half of the circle) and centred at 59.5% of the button's height, on the raised face: the circle's
   shadow and thick lower rim put its face 11.5 px above the sprite's centre, and the cross sits 2.5 px low
   in its own sprite. An instance that is a button adds `Button` and `ButtonPressFeedback` to the prefab's
   look; change the look in the prefab.

A switch's "off" look is a dark track with a plain knob, because the art has
only the "on" knob (asset-feedback.md A-1).

## 7. Settings popup

One layout, two scenes. **`SettingsPopup.prefab`** is the base: panel, header, the vibration, sound and
music switches, and the Legal Terms, Restore Purchases, Support and Language placeholders, laid out after
the Home reference. It carries no view component; each scene uses a **prefab variant** of it:

| Variant | View | Adds |
| --- | --- | --- |
| `HomeSettingsPopup` (`HomePopupConfig`) | `SettingsPopupView` | nothing: close returns to Home |
| `GameplaySettingsPopup` (`GameplayPopupConfig`, the pause popup) | `PausePopupView : SettingsPopupView` | a taller panel and Home; close resumes |

The four blue buttons are 430x168, about as wide as a switch row, 210 apart (`Btn_popup_blue`); Home in the
pause popup is 430x172 (`Btn_popup_green`). Both are drawn at their sprite's vertical scale
([art-pipeline.md](art-pipeline.md) §2). Labels are centred: a plain label stretches over the button with 45 units of
margin each side; on Support and Home the icon and the label are centred together as one group, 18 units
apart. Labels are at most 50 points (44 for the two-line Restore Purchases, 54 for Home), so the text never
fills more than 80% of the button's width (`PopupPrefabTests`).

A change to the shared layout goes in the base prefab and reaches both. `SettingsPopupView` shows the saved
switches (`Refresh` before opening, `SettingChangedSignal` after) and reports taps; the flow sets
`ISettingsService` (DA-10). `PopupPrefabTests` checks both are variants of the base.

## 8. Home screen

Authored under `HomeCanvas` in `Home.unity` from prefabs in `Prefabs/UI/Home/`, laid out after the reference
(T-23): `HomeBackground`, `HomeTopBar` (the scene object keeps the name `TopBar`), `LevelPath` and `TabBar`.
`HomeView` stays on the canvas and references parts inside them; `Popups` is an empty layer the scope fills.
Change a part in its prefab:

| Part | Art | Behaviour |
| --- | --- | --- |
| `Background` | `bg_home_screen` | fills the screen, outside the safe area |
| `TopBar/ProfileButton` | `profile_box_blue`, 220x220 | placeholder |
| `TopBar/Lives` | `bg_home_resource_holder`, `Heart`, "Full" and 5, green "+" | `LivesBarView`: lives, "Full" or the countdown; a tap anywhere on the bar opens the lives popup ([lives.md](lives.md)) |
| `TopBar/Coins` | `bg_home_resource_holder`, `Coin_Single`, green "+" | balance (`CoinAmountText`); "+" is a placeholder |
| `TopBar/SettingsButton` | `Settings_Icon` | opens the Home settings popup |
| `LevelPath` | gold rails, `Btn_level_green` tile for the current level (`CurrentTile`), `Btn_settings_normal` + `lock` for the next two | numbers from the saved level; the current tile breathes (DA-45) |
| `LevelPath` (`ScrollRect`) | `Viewport` (a clear image that catches drags) and `Content` with the rails and tiles | vertical and elastic: the path stretches when dragged and springs back; there is nothing to scroll to yet, the `Level N` button stays put (DA-47) |
| `LevelPath/LevelButton` | `Btn_level_green` (nine-sliced, art-pipeline.md §4), "Level N" | plays the current level |
| `TabBar` | `bg_menu`, `bg_home_line` separators, `ActiveTab` under Home, `lock`, `shop`, `home_icon`, `block_collection`, `lock` "Lvl 50" | placeholders: press feedback only; Home stays active (DA-11) |

The top bar's four parts are centred 150 units below the safe area's top and spread over it with 36 units of
margin each side in the reference's proportions: profile 220, lives 329, coins 261, settings 103. Neighbours
sit close, as in the reference, but never touch: at least 5 units apart, measured on what is drawn (sprite
padding excluded), counting the heart, the coin and the "+" badges that stick out of the bars
(`HomeLayoutTests`). While Home is shown, `HomeView` plays
`IdlePulseAnimation` on the current level's tile from `UiConfig.CurrentLevelPulse` (1.06 times its size and 10
units up, 0.85 s each way, `InOutSine`, looping), restarted from rest whenever the level is shown and killed
with the view.

The reference's chef portrait and its skull marker for hard levels have no art in the project, so the
profile frame stays empty and locked tiles all look alike. `HomeFlow` shows the level and the balance on
start and routes the level and settings buttons (T-24).

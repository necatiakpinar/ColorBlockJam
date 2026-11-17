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
   blocker. The shared popup view and its in/out animations land with the first popup (T-19, T-25).

## 4. Configs

| Asset | Holds |
| --- | --- |
| `UiConfig` (Root) | button press feedback |

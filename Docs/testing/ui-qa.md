# Shared UI Kit QA

Verifies [ui.md](../General/ui.md). `⚙` = automated (test named), `👁` = manual. Every case is checked at
1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## §1 Canvases

- [x] ⚙ Every canvas scales from 1080x1920, match 0.5 — `SceneSetupTests.Canvases_ShouldScaleFromReferenceResolution_WhenInBuildScene`
- [x] ⚙ Every canvas has a safe-area root — `SceneSetupTests.Canvases_ShouldKeepContentInsideSafeArea_WhenInBuildScene`
- [x] ⚙ Safe-area anchors exclude notch and home bar — `SafeAreaTests.Anchors_ShouldExcludeNotchAndHomeBar_WhenTallPhoneHasInsets`
- [ ] 👁 Device Simulator, a notched phone: content sits below the notch, backgrounds still fill the screen.

## §2 Buttons

- [x] ⚙ Every button has press feedback wired — `SceneSetupTests.Buttons_ShouldGivePressFeedback_WhenInBuildScene`
- [x] ⚙ A pressed button shrinks and springs back — `ButtonPressFeedbackTests.LevelButton_ShouldShrinkWhilePressedAndRestoreOnRelease_WhenTapped`
- [x] ⚙ A non-interactable button keeps its size — `ButtonPressFeedbackTests.LevelButton_ShouldKeepItsSize_WhenNotInteractable`
- [x] ⚙ A button hidden mid-press comes back full size — `ButtonPressFeedbackTests.LevelButton_ShouldRestoreItsSize_WhenDisabledWhilePressed`
- [ ] 👁 Dragging off a pressed button and releasing restores its size.

## §3 Popups

- [x] ⚙ One popup at a time — `PopupServiceTests.Open_ShouldThrowAndKeepFirstPopup_WhenAnotherIsOpen`
- [ ] 👁 With a popup open, taps behind it do nothing (checked when the first popup lands, T-19 / T-25).

# Settings popup QA

Stub created by the task breakdown; filled by tasks T-25. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

- [ ] ⚙ The settings button opens the popup and close closes it — `HomeScreenTests.SettingsButton_ShouldOpenAndCloseTheSettingsPopup_WhenTapped`
- [ ] ⚙ The switches show the saved settings and a tap flips one — `HomeFlowTests`, `GameplayFlowTests`
- [ ] ⚙ The Home and the pause popups are variants of one base prefab — `PopupPrefabTests`
- [ ] 👁 Each switch looks clearly different on and off; the state survives closing, reopening and an app restart.
- [ ] 👁 Legal Terms, Restore Purchases, Support and Language only shrink when pressed.

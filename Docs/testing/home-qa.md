# Home screen QA

Stub created by the task breakdown; filled by tasks T-23, T-24. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

- [ ] ⚙ Home shows the saved level on the path and the "Level N" button, and the balance — `HomeFlowTests.Start_ShouldShowTheLevelPathAndTheBalance_WhenSceneStarts`, `HomeScreenTests`
- [ ] ⚙ The level button opens Gameplay — `HomeFlowTests.LevelButton_ShouldNavigateToGameplay_WhenTapped`
- [ ] ⚙ Every Home button gives press feedback — `SceneSetupTests`
- [ ] 👁 Compare Home with the reference screen at 1080x1920 and 20:9: top bar, level path, tab bar.
- [ ] 👁 Tabs, the "+" buttons and the profile only shrink when pressed; Home stays the active tab.

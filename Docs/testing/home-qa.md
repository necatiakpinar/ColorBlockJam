# Home screen QA

Stub created by the task breakdown; filled by tasks T-23, T-24. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

- [x] ⚙ Home shows the saved level on the path and the "Level N" button, and the balance — `HomeFlowTests.Start_ShouldShowTheLevelPathAndTheBalance_WhenSceneStarts`, `HomeScreenTests`
- [x] ⚙ The balance follows every spend on Home, such as a lives refill — `HomeFlowTests.Refill_ShouldShowTheBalanceLeft_WhenLivesAreBought`
- [x] ⚙ The level button opens Gameplay — `HomeFlowTests.LevelButton_ShouldNavigateToGameplay_WhenTapped`
- [x] ⚙ Every Home button gives press feedback — `SceneSetupTests`
- [x] ⚙ The top bar sits high, the profile is at least 200 wide and no part of the top bar touches its neighbour — `HomeLayoutTests`
- [x] ⚙ The current level's tile breathes while Home is shown — `HomeLayoutTests.ShowLevel_ShouldSetTheCurrentTileBreathing_WhenHomeShowsTheLevel`
- [ ] 👁 The breathing reads as a gentle invitation, not a jitter, and stops cleanly when the level starts.
- [ ] 👁 Compare Home with the reference screen at 1080x1920 and 20:9: top bar, level path, tab bar.
- [ ] 👁 Tabs, the "+" buttons and the profile only shrink when pressed; Home stays the active tab.

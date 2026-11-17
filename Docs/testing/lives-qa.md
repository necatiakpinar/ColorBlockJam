# Lives QA

Checks for [lives.md](../General/lives.md). `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

- [x] ⚙ A new player is full; a lost life starts a 30-minute countdown; lives come back one by one, never past 5 — `LivesServiceTests`
- [x] ⚙ The countdown waits for the trusted time and survives a restart of the app — `LivesServiceTests.Countdown_ShouldWaitForTheTrustedTime_WhenALifeIsLostOffline`, `Lives_ShouldSurviveARestart_WhenSavedAndLoaded`
- [x] ⚙ Refill fills every life for 100 coins, or does nothing when full or short — `LivesServiceTests.Refill_*`
- [x] ⚙ Only a started, abandoned attempt costs a life, once; a win costs none — `LevelAttemptLivesTests`
- [x] ⚙ Running out of time takes the started attempt's life once — `LevelAttemptLivesTests.Fail_ShouldCostOneLifeOnce_WhenAStartedAttemptRunsOutOfTime`, `LevelResultFlowTests.Fail_ShouldTakeTheAttemptsLife_WhenTheLevelFails`
- [x] ⚙ Without lives, Retry and restart offer a refill; Refill restarts, close goes Home — `LevelAttemptLivesTests`
- [x] ⚙ Pause → Home asks first when it would cost a life; Leave takes it, close goes back to pause — `GameplayFlowTests.PausePopupHome_ShouldAskFirst_WhenLeavingWouldCostALife`, `LeavePopup_*`
- [x] ⚙ Home plays only with a life; otherwise the lives popup opens — `HomeFlowTests.Play_ShouldOpenTheLivesPopupInsteadOfTheLevel_WhenNoLivesAreLeft`
- [ ] 👁 The Home bar shows the count and "Full", or a countdown that ticks every second; "+" opens the popup.
- [ ] 👁 Changing the device clock moves no countdown; closing the app for an hour gives two lives back.
- [ ] 👁 The lives popup fits the screen on both scenes; Refill is greyed out without the coins or when full.

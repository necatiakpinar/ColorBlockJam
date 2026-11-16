# Gameplay, timer, coins, popups and the five levels QA

Stub created by the case breakdown; filled by tasks T-09–T-22, T-32. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Input

- [ ] ⚙ Press, drag and release report cells in order — `BoardInputTests.PressDragRelease_ShouldReportCellsInOrder_WhenOneFingerMovesAcrossTheBoard`
- [ ] ⚙ A second finger is ignored — `BoardInputTests.SecondFinger_ShouldBeIgnored_WhenAFingerIsAlreadyDown`
- [ ] ⚙ A touch on a board cell reaches the board — `BoardInputTests.EventSystemRaycast_ShouldHitTheBoard_WhenTouchingABoardCell`
- [ ] ⚙ A HUD button over the board wins the touch — `BoardInputTests.EventSystemRaycast_ShouldHitTheHudButtonFirst_WhenTheButtonCoversTheBoard`
- [ ] ⚙ A release off the board plane ends the touch at the last cell — `BoardInputTests.Release_ShouldEndTheTouchAtTheLastCell_WhenTheReleaseRayMissesTheBoardPlane`
- [ ] ⚙ Disabling the board mid-touch frees input — `BoardInputTests.Press_ShouldBeAccepted_WhenTheViewWasDisabledMidTouch`

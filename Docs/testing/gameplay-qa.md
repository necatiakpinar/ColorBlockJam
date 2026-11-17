# Gameplay, timer, coins, popups and the five levels QA

Stub created by the task breakdown; filled by tasks T-09–T-22, T-32. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Level session

- [ ] ⚙ Start, pause, resume, win, fail and restart follow the state table; anything else is refused — `LevelSessionTests`
- [ ] ⚙ Restart gives a board equal to a fresh start — `LevelSessionTests.Restart_ShouldGiveAFreshBoardEqualToAFirstStart_WhenPlayingPausedOrFailed`
- [ ] ⚙ Pausing mid-drag drops the block onto its cell; no input while paused or lost — `BlockDragControllerTests.Pause_ShouldDropTheHeldBlockOntoItsCell_WhenTheLevelPausesMidDrag`, `Input_ShouldBeIgnored_UntilTheLevelResumes`, `Input_ShouldBeIgnored_WhenTheLevelIsLost`
- [ ] ⚙ The last exit wins the level without dropping the block that left — `BlockDragControllerTests.Win_ShouldForgetTheExitedBlockWithoutDroppingIt_WhenTheLastExitCompletesTheLevel`

- [ ] ⚙ The timer waits at full time until the first block is grabbed, then counts down second by second and fails the level once at zero — `LevelTimerTests`, `GameplayTimerTests.Hud_ShouldCountTheTimerDown_OnceABlockIsGrabbed`
- [ ] ⚙ Pausing or backgrounding the app freezes the timer; restart gives the full time — `LevelTimerTests`, `GameplayTimerTests.Timer_ShouldStandStill_WhileTheAppIsInTheBackground`
- [ ] 👁 On a device, send the app to the background for a while mid-level: the timer shows the same time when it comes back.
- [ ] ⚙ A win pays its difficulty's coins once; failing or leaving pays nothing — `LevelRewardTests`, `CoinPersistenceTests`
- [ ] ⚙ Coins stay the same after going Home and coming back — `CoinPersistenceTests`
- [ ] 👁 Kill the app after a win and open it again: the balance is the same.
- [ ] ⚙ Pause opens the settings popup, the switches remember their state, close resumes — `GameplayFlowTests`, `GameplayPopupTests`
- [ ] ⚙ A win opens the win popup with the reward; Continue plays the next level — `LevelResultFlowTests`, `LevelProgressionFlowTests`
- [ ] ⚙ Out of time sells 20 s for 50 coins; giving up shows Level Failed; Retry restarts, close goes Home — `LevelResultFlowTests`, `LevelContinueTests`, `GameplayPopupTests`
- [ ] 👁 Compare the HUD and the four popups with the reference screens at 1080x1920 and 20:9. Open them
  with the debug window ([debug-tools.md](../General/debug-tools.md)): **Win now**, **Run out of time**,
  the pause button.
- [ ] ⚙ The debug window's shortcuts go through the real services — `GameplayDebugCommandsTests`
- [ ] 👁 Every HUD and popup button shrinks while pressed; boosters and the coin "+" do nothing else.
- [ ] ⚙ Winning starts the next level; after the last level the first layout comes back — `LevelProgressionFlowTests`
- [ ] ⚙ All five shipped levels are valid and solvable — `ShippedLevelsTests`
- [ ] 👁 Play levels 1 to 5 by hand: each one is a little harder than the one before, and each finishes well inside its timer.

### Rules

- [ ] ⚙ Blocks stop at walls and at other blocks — `BoardStateTests.TryStep_ShouldStopAtWall_WhenNoDoorIsThere`, `TryStep_ShouldStopAtAnotherBlock_WhenItIsInTheWay`
- [ ] ⚙ A block leaves through a matching door that covers it — `BoardStateTests.TryStep_ShouldExit_WhenAMatchingDoorCoversTheBlock`
- [ ] ⚙ Wrong colour, narrow or offset doors keep it in — `BoardStateTests.TryStep_ShouldBeBlocked_WhenTheDoorColourDiffers` and siblings
- [ ] ⚙ A block cannot leave while another block stands in its way out — `BoardStateTests.TryStep_ShouldBeBlocked_WhenAnotherBlockStandsInTheWayOut`
- [ ] ⚙ The last exit clears the board — `BoardStateTests.IsCleared_ShouldBecomeTrue_WhenTheLastBlockExits`
- [ ] ⚙ A restart starts from the level, not the last attempt — `BoardStateTests.NewBoard_ShouldStartFromTheLevel_WhenAnEarlierAttemptMovedBlocks`

### Board and controls

- [ ] ⚙ The board draws floor, walls, doors, corners and every block — `GameplayBoardTests.Board_ShouldDrawEveryPartAndBlock_WhenGameplayStarts`
- [ ] ⚙ The wall ring is continuous bars from corner to corner, with no seams — `GameplayBoardTests.Board_ShouldCloseTheWallRingWithoutSeams_WhenWallsAreStretchedBetweenDoors`
- [ ] ⚙ Each door shows one arrow, centred, pointing out of the board — `GameplayBoardTests.Doors_ShouldShowOneCentredArrowPointingOutOfTheBoard_WhenTheBoardIsBuilt`
- [ ] 👁 The arrows sit on the door (not floating, not sunk) and go down and up with it when a block leaves.
- [ ] ⚙ A held block follows the finger between cells — `BlockDragControllerTests.Drag_ShouldFollowTheFingerBetweenCells_WhenTheWayIsFree`
- [ ] ⚙ A fast drag crosses free cells but stops on the edge of an obstacle — `BlockDragControllerTests.Drag_ShouldCrossSeveralFreeCellsInOneEvent_WhenTheFingerMovesFast`, `Drag_ShouldStopOnTheCellEdge_WhenAFastFingerPassesAnObstacle`
- [ ] ⚙ A released block settles on the nearest cell — `BlockDragControllerTests.Release_ShouldSnapToTheNearestCell_WhenTheBlockIsBetweenCells`
- [ ] ⚙ A block slightly off its line slips into a one-cell gap and stays with the finger — `BlockDragControllerTests.Drag_ShouldEaseIntoAOneCellGap_WhenPushedThroughItSlightlyOffTheCellLine`, `Drag_ShouldStayWithTheFinger_WhenItSlippedIntoAGapAndCameOutAgain`
- [ ] ⚙ A slow push into a one-cell gap from slightly off the line does not jitter — `BlockDragControllerTests.Drag_ShouldNotJitter_WhenPushedSlowlyIntoAOneCellGapSlightlyOffTheCellLine`
- [ ] ⚙ A block never locks out of a free gap or corridor: it turns into a side corridor after slipping toward another gap, and enters a gap while the finger wobbles at its mouth — `BlockDragControllerTests.Drag_ShouldTurnIntoASideCorridor_WhenItSlippedTowardAnotherGapFirst`, `Drag_ShouldEnterTheGap_WhenTheFingerWobblesSidewaysAtItsMouth`
- [ ] 👁 Held anywhere, a block lined up roughly with a free one-cell gap or corridor goes in and comes back out without sticking.
- [ ] ⚙ A held block glides after the finger instead of jumping on a sudden move — `GameplayBoardTests.BlockView_ShouldGlideAfterTheFinger_WhenItMovesSuddenly`
- [ ] 👁 Dragging feels smooth: the block trails the finger by about a tenth of a second and never teleports.
- [ ] ⚙ A press just next to a block picks it; a press further away picks nothing — `BlockDragControllerTests.Grab_ShouldPickTheNearestBlock_WhenThePressMissesItByLittle`, `Grab_ShouldPickNothing_WhenThePressIsBeyondTheTouchTolerance`
- [ ] ⚙ An exit taps the device lightly — `GameplayHapticsTests`
- [ ] 👁 On an Android and an iOS device, the tap is short and crisp (not a long buzz).
- [ ] 👁 Pushing a block into a door of another colour stops it like a wall: no shake, no tap.
- [ ] 👁 A block leaving through a bottom door (facing the camera) sits flush at the door while it is cut away; no gap shows between block and door.
- [ ] ⚙ A held block with no room leans a little and snaps back — `BlockDragControllerTests.Drag_ShouldLeanTowardTheFingerButNotCommit_WhenTheBlockHasNoRoom`
- [ ] ⚙ The held block is outlined — `GameplayBoardTests.Outline_ShouldShowWhileABlockIsHeld_WhenGrabbedAndHideOnRelease`
- [ ] 👁 The outline stays visible where another object covers the block.
- [ ] ⚙ The outline hugs the block with rounded corners, notches included — `OutlineMeshBuilderTests`
- [ ] ⚙ A swipe takes a block out; the last exit completes the level once — `GameplayBoardTests.Swipe_ShouldTakeTheBlockOffTheBoardAndClearItOnce_WhenEveryBlockLeaves`
- [ ] ⚙ The board fits between HUD and boosters at 9:16 and 20:9 — `BoardFramingTests`
- [ ] ⚙ A reload mid-exit rebuilds clean blocks — `GameplayBoardTests.Reload_ShouldRebuildCleanBlocks_WhenABlockIsStillSlidingOut`
- [ ] ⚙ Dragging allocates nothing — `BlockDragControllerTests.Drag_ShouldNotAllocate_WhenAFingerMovesABlockBackAndForth`, `GameplayBoardTests.BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping`
- [ ] 👁 Blocks of every shape (I, L, T, S, square, 1x1) have seamless sides and rounded outer corners.
- [ ] 👁 Dragging feels attached to the finger, also when flicked fast across the board; a released block settles on the grid with a small overshoot.
- [ ] ⚙ A block leaves as soon as it is pushed a little into its own door — `BlockDragControllerTests.Drag_ShouldExit_WhenTheBlockIsPushedJustIntoItsDoor`
- [ ] ⚙ An exiting block settles at its door before it starts to disappear — `GameplayBoardTests.Exit_ShouldSettleAtTheDoorBeforeSlidingIn_WhenTheBlockLeaves`
- [ ] ⚙ The door sinks open for a leaving block and closes after it — `GameplayBoardTests.Exit_ShouldOpenTheDoorAndCloseItAfter_WhenTheBlockLeaves`
- [ ] ⚙ A block narrower than its door sinks the whole door — `GameplayBoardTests.Exit_ShouldSinkTheWholeDoor_WhenASmallerBlockLeavesThroughIt`
- [ ] 👁 Debris bursts out of the door in a cone pointing away from the board (left door to the left, bottom door down).
- [ ] ⚙ Blocks leaving one door together share one spray — `GameplayBoardTests.Exit_ShouldShareOneSpray_WhenTwoBlocksLeaveTheSameDoorTogether`
- [ ] ⚙ An exiting block keeps its size, is cut at the board's edge and sprays debris — `GameplayBoardTests.Exit_ShouldCutTheBlockAtTheDoorAndSprayDebris_WhenItLeaves`
- [ ] 👁 An exiting block lines up with its door, slides into it at a steady speed and disappears into the door while cubes of its colour spray out; nothing pops.

### Input

- [ ] ⚙ Press, drag and release report cells in order — `BoardInputTests.PressDragRelease_ShouldReportCellsInOrder_WhenOneFingerMovesAcrossTheBoard`
- [ ] ⚙ A second finger is ignored — `BoardInputTests.SecondFinger_ShouldBeIgnored_WhenAFingerIsAlreadyDown`
- [ ] ⚙ A touch on a board cell reaches the board — `BoardInputTests.EventSystemRaycast_ShouldHitTheBoard_WhenTouchingABoardCell`
- [ ] ⚙ A HUD button over the board wins the touch — `BoardInputTests.EventSystemRaycast_ShouldHitTheHudButtonFirst_WhenTheButtonCoversTheBoard`
- [ ] ⚙ A release off the board plane ends the touch at the last cell — `BoardInputTests.Release_ShouldEndTheTouchAtTheLastCell_WhenTheReleaseRayMissesTheBoardPlane`
- [ ] ⚙ Disabling the board mid-touch frees input — `BoardInputTests.Press_ShouldBeAccepted_WhenTheViewWasDisabledMidTouch`

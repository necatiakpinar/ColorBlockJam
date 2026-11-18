# Gameplay, timer, coins, popups and the shipped levels QA

Stub created by the task breakdown; filled by tasks T-09–T-22, T-32. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Level session

- [x] ⚙ Start, pause, resume, win, fail and restart follow the state table; anything else is refused — `LevelSessionTests`
- [x] ⚙ Restart gives a board equal to a fresh start — `LevelSessionTests.Restart_ShouldGiveAFreshBoardEqualToAFirstStart_WhenPlayingPausedOrFailed`
- [x] ⚙ Pausing mid-drag drops the block onto its cell; no input while paused or lost — `BlockDragControllerTests.Pause_ShouldDropTheHeldBlockOntoItsCell_WhenTheLevelPausesMidDrag`, `Input_ShouldBeIgnored_UntilTheLevelResumes`, `Input_ShouldBeIgnored_WhenTheLevelIsLost`
- [x] ⚙ The last exit wins the level without dropping the block that left — `BlockDragControllerTests.Win_ShouldForgetTheExitedBlockWithoutDroppingIt_WhenTheLastExitCompletesTheLevel`

- [x] ⚙ The timer waits at full time until the first block is grabbed, then counts down second by second and fails the level once at zero — `LevelTimerTests`, `GameplayTimerTests.Hud_ShouldCountTheTimerDown_OnceABlockIsGrabbed`
- [x] ⚙ Pausing or backgrounding the app freezes the timer; restart gives the full time — `LevelTimerTests`, `GameplayTimerTests.Timer_ShouldStandStill_WhileTheAppIsInTheBackground`
- [ ] 👁 On a device, send the app to the background for a while mid-level: the timer shows the same time when it comes back.
- [x] ⚙ A win pays its difficulty's coins once, with the next level, in one save; failing or leaving pays nothing — `LevelWinServiceTests`, `CoinPersistenceTests`
- [x] ⚙ Coins stay the same after going Home and coming back — `CoinPersistenceTests`
- [ ] 👁 Kill the app after a win and open it again: the balance is the same.
- [x] ⚙ Pause opens the settings popup, the switches remember their state, close resumes — `GameplayFlowTests`, `GameplayPopupTests`
- [x] ⚙ A win opens the win popup with the reward; Continue flies the coins to the counter and goes Home — `LevelResultFlowTests`, `LevelProgressionFlowTests`
- [ ] 👁 The coins burst out of the popup's pile and arc to the coin counter one after another; each landing swells the counter, sparkles and counts it up; then Home shows the next level.
- [x] ⚙ Running out of time shows Level Failed at once; Retry restarts, or offers a refill with no lives left; Return to Menu and close go Home — `LevelResultFlowTests`, `GameplayPopupTests`
- [ ] 👁 Compare the HUD and the three popups with the reference screens at 1080x1920 and 20:9. Open them
  with the debug window ([debug-tools.md](../General/debug-tools.md)): **Win now**, **Run out of time**,
  the pause button.
- [x] ⚙ The debug window's shortcuts go through the real services — `GameplayDebugCommandsTests`
- [ ] 👁 Every HUD and popup button shrinks while pressed; boosters and the coin "+" do nothing else.
- [x] ⚙ Ten restarts in a row leave the same board, timer, coins and objects as a fresh start — `RestartResetsStateTests.Restart_ShouldLeaveTheSameStateAsAFreshStart_WhenRepeatedTenTimes`
- [x] ⚙ Going Home after a loss and back gives the same level, fresh — `RestartResetsStateTests.Home_ShouldGiveTheSameLevelAndAFreshAttempt_WhenLeftAfterALoss`
- [x] ⚙ No gameplay listener survives going Home — `RestartResetsStateTests.SceneUnload_ShouldLeaveNoGameplayListener_WhenGoingHome`
- [x] ⚙ Winning moves on to the next level, played from Home; after the last level the first layout comes back — `LevelProgressionFlowTests`, `LevelProviderTests`
- [x] ⚙ All twenty shipped levels are valid and solvable; 1–8 ramp up, then groups of six with a breather and a rising peak; level 6 has two frozen blocks, levels 7 and 8 arrow blocks — `ShippedLevelsTests`
- [ ] 👁 Play levels 14 and 20 by hand: they need blocks moved around in the right order before most can leave, and the timer leaves room to think.
- [x] 👁 Play levels 1 to 5 by hand (developer, 2026-11-17): each one is a little harder than the one before, and each finishes well inside its timer.
  The solver's solutions, blocks numbered as in the editor (Space steps them on its canvas):
  - L1: 1 Cyan out right; 2 Purple out down.
  - L2: 2 Orange down; 3 Blue left; 1 Cyan up.
  - L3: 2 Orange right; 4 Red down; 5 Yellow right; move 1 Yellow aside; 3 Purple up; 1 Yellow right.
  - L4: 3 Blue up; 6 Pink right; move 1 Pink down two; 2 Orange up; 4 Yellow left; 1 Pink right; 5 Red right; 7 Orange up.
  - L5: 7 Red up; 8 Green down; move 1 Red up two; 5 Orange right; move 4 Blue aside; 2 Green down; 6 Yellow down; 3 Purple left; 1 Red up; 4 Blue left.
  - L6 (ice: 4 Blue waits for 2 exits, 1 Cyan for 4): move 2 Red left two; 3 Pink right; 6 Purple up; 9 Pink right; 4 Blue (thawed) up; 5 Green left; move 1 Cyan (thawed) up two; 8 Red down; 1 Cyan right; 7 Cyan right; 2 Red down.
- [ ] 👁 Play level 6 by hand: both frozen blocks show their count, count down with every exit, thaw with a burst of ice and then leave.
- [ ] 👁 Play levels 7 and 8 by hand: every arrow block shows its arrow, moves only along it and leaves through a door on that axis.

### Rules

- [x] ⚙ A frozen block cannot move until enough others leave, then thaws — `IceBlockerTests`, `IceBlockerViewTests`
- [x] ⚙ An arrow block moves and leaves only along its axis — `ArrowBlockerTests`
- [ ] 👁 Arrow blocks show a white double-headed arrow sized to the block; dragging across the axis does not move or lean the block.
- [ ] 👁 A frozen block looks icy over its colour with a readable count; the count drops with every exit; the thaw swells it back to its colour.

- [x] ⚙ Blocks stop at walls and at other blocks — `BoardStateTests.TryStep_ShouldStopAtWall_WhenNoDoorIsThere`, `TryStep_ShouldStopAtAnotherBlock_WhenItIsInTheWay`
- [x] ⚙ A block leaves through a matching door that covers it — `BoardStateTests.TryStep_ShouldExit_WhenAMatchingDoorCoversTheBlock`
- [x] ⚙ Wrong colour, narrow or offset doors keep it in — `BoardStateTests.TryStep_ShouldBeBlocked_WhenTheDoorColourDiffers` and siblings
- [x] ⚙ A block cannot leave while another block stands in its way out — `BoardStateTests.TryStep_ShouldBeBlocked_WhenAnotherBlockStandsInTheWayOut`
- [x] ⚙ The last exit clears the board — `BoardStateTests.IsCleared_ShouldBecomeTrue_WhenTheLastBlockExits`
- [x] ⚙ A restart starts from the level, not the last attempt — `BoardStateTests.NewBoard_ShouldStartFromTheLevel_WhenAnEarlierAttemptMovedBlocks`

### Board and controls

- [x] ⚙ The board draws floor, walls, doors, corners and every block — `GameplayBoardTests.Board_ShouldDrawEveryPartAndBlock_WhenGameplayStarts`
- [x] ⚙ The wall ring is continuous bars from corner to corner, with no seams — `GameplayBoardTests.Board_ShouldCloseTheWallRingWithoutSeams_WhenWallsAreStretchedBetweenDoors`
- [x] ⚙ Each door shows one arrow, centred, pointing out of the board — `GameplayBoardTests.Doors_ShouldShowOneCentredArrowPointingOutOfTheBoard_WhenTheBoardIsBuilt`
- [ ] 👁 The arrows sit on the door (not floating, not sunk) and go down and up with it when a block leaves.
- [x] ⚙ A held block follows the finger between cells — `BlockDragControllerTests.Drag_ShouldFollowTheFingerBetweenCells_WhenTheWayIsFree`
- [x] ⚙ A fast drag crosses free cells but stops on the edge of an obstacle — `BlockDragControllerTests.Drag_ShouldCrossSeveralFreeCellsInOneEvent_WhenTheFingerMovesFast`, `Drag_ShouldStopOnTheCellEdge_WhenAFastFingerPassesAnObstacle`
- [x] ⚙ A released block settles on the nearest cell — `BlockDragControllerTests.Release_ShouldSnapToTheNearestCell_WhenTheBlockIsBetweenCells`
- [x] ⚙ A block slightly off its line slips into a one-cell gap and stays with the finger — `BlockDragControllerTests.Drag_ShouldEaseIntoAOneCellGap_WhenPushedThroughItSlightlyOffTheCellLine`, `Drag_ShouldStayWithTheFinger_WhenItSlippedIntoAGapAndCameOutAgain`
- [x] ⚙ A slow push into a one-cell gap from slightly off the line does not jitter — `BlockDragControllerTests.Drag_ShouldNotJitter_WhenPushedSlowlyIntoAOneCellGapSlightlyOffTheCellLine`
- [x] ⚙ A block never locks out of a free gap or corridor: it turns into a side corridor after slipping toward another gap, and enters a gap while the finger wobbles at its mouth — `BlockDragControllerTests.Drag_ShouldTurnIntoASideCorridor_WhenItSlippedTowardAnotherGapFirst`, `Drag_ShouldEnterTheGap_WhenTheFingerWobblesSidewaysAtItsMouth`
- [ ] 👁 Held anywhere, a block lined up roughly with a free one-cell gap or corridor goes in and comes back out without sticking.
- [x] ⚙ A held block glides after the finger instead of jumping on a sudden move — `GameplayBoardTests.BlockView_ShouldGlideAfterTheFinger_WhenItMovesSuddenly`
- [ ] 👁 Dragging feels smooth: the block trails the finger by about a tenth of a second and never teleports.
- [x] ⚙ A press just next to a block picks it; a press further away picks nothing — `BlockDragControllerTests.Grab_ShouldPickTheNearestBlock_WhenThePressMissesItByLittle`, `Grab_ShouldPickNothing_WhenThePressIsBeyondTheTouchTolerance`
- [x] ⚙ A grab taps medium; an exit ticks light until the block has slid out; a win and a fail tap heavy; the
  vibration toggle silences them — `GameplayHapticsTests`
- [ ] 👁 On a device, picking a block clicks once, and a block leaving its door ticks in a quick run that stops
  as its last piece disappears; two blocks leaving together tick as one run.
- [ ] 👁 On a device, a win and a fail each give one heavy tap.
- [ ] 👁 On an Android and an iOS device, the tap is short and crisp (not a long buzz).
- [ ] 👁 Pushing a block into a door of another colour stops it like a wall: no shake, no tap.
- [ ] 👁 A block leaving through a bottom door (facing the camera) sits flush at the door while it is cut away; no gap shows between block and door.
- [x] ⚙ A held block with no room leans a little and snaps back — `BlockDragControllerTests.Drag_ShouldLeanTowardTheFingerButNotCommit_WhenTheBlockHasNoRoom`
- [x] ⚙ The held block is outlined — `GameplayBoardTests.Outline_ShouldShowWhileABlockIsHeld_WhenGrabbedAndHideOnRelease`
- [ ] 👁 The outline stays visible where another object covers the block.
- [x] ⚙ The outline hugs the block with rounded corners, notches included — `OutlineMeshBuilderTests`
- [x] ⚙ A swipe takes a block out; the last exit completes the level once — `GameplayBoardTests.Swipe_ShouldTakeTheBlockOffTheBoardAndClearItOnce_WhenEveryBlockLeaves`
- [x] ⚙ The board fits between HUD and boosters at 9:16 and 20:9 — `BoardFramingTests`
- [x] ⚙ A reload mid-exit rebuilds clean blocks — `GameplayBoardTests.Reload_ShouldRebuildCleanBlocks_WhenABlockIsStillSlidingOut`
- [x] ⚙ Dragging allocates nothing — `BlockDragControllerTests.Drag_ShouldNotAllocate_WhenAFingerMovesABlockBackAndForth`, `GameplayBoardTests.BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping`
- [ ] 👁 Blocks of every shape (I, L, T, S, square, 1x1) have seamless sides and rounded outer corners.
- [ ] 👁 Dragging feels attached to the finger, also when flicked fast across the board; a released block settles on the grid with a small overshoot.
- [x] ⚙ A block leaves as soon as it is pushed a little into its own door — `BlockDragControllerTests.Drag_ShouldExit_WhenTheBlockIsPushedJustIntoItsDoor`
- [x] ⚙ An exiting block settles at its door before it starts to disappear — `GameplayBoardTests.Exit_ShouldSettleAtTheDoorBeforeSlidingIn_WhenTheBlockLeaves`
- [x] ⚙ The door sinks open for a leaving block and closes after it — `GameplayBoardTests.Exit_ShouldOpenTheDoorAndCloseItAfter_WhenTheBlockLeaves`
- [x] ⚙ A block narrower than its door sinks the whole door — `GameplayBoardTests.Exit_ShouldSinkTheWholeDoor_WhenASmallerBlockLeavesThroughIt`
- [ ] 👁 Debris bursts out of the door in a cone pointing away from the board (left door to the left, bottom door down).
- [x] ⚙ Blocks leaving one door together share one spray — `GameplayBoardTests.Exit_ShouldShareOneSpray_WhenTwoBlocksLeaveTheSameDoorTogether`
- [x] ⚙ An exiting block keeps its size, is cut at the board's edge and sprays debris — `GameplayBoardTests.Exit_ShouldCutTheBlockAtTheDoorAndSprayDebris_WhenItLeaves`
- [ ] 👁 An exiting block lines up with its door, slides into it at a steady speed and disappears into the door while cubes of its colour spray out; nothing pops.

### Input

- [x] ⚙ Press, drag and release report cells in order — `BoardInputTests.PressDragRelease_ShouldReportCellsInOrder_WhenOneFingerMovesAcrossTheBoard`
- [x] ⚙ A second finger is ignored — `BoardInputTests.SecondFinger_ShouldBeIgnored_WhenAFingerIsAlreadyDown`
- [x] ⚙ A touch on a board cell reaches the board — `BoardInputTests.EventSystemRaycast_ShouldHitTheBoard_WhenTouchingABoardCell`
- [x] ⚙ A HUD button over the board wins the touch — `BoardInputTests.EventSystemRaycast_ShouldHitTheHudButtonFirst_WhenTheButtonCoversTheBoard`
- [x] ⚙ A release off the board plane ends the touch at the last cell — `BoardInputTests.Release_ShouldEndTheTouchAtTheLastCell_WhenTheReleaseRayMissesTheBoardPlane`
- [x] ⚙ Disabling the board mid-touch frees input — `BoardInputTests.Press_ShouldBeAccepted_WhenTheViewWasDisabledMidTouch`
- [x] ⚙ A touch lost to the background drops the held block and the next press works — `BlockDragControllerTests.Cancel_ShouldDropTheHeldBlockOntoItsCellAndTakeTheNextPress_WhenTheTouchIsLost`, `BoardInputTests.Background_ShouldCancelTheHeldPressAndAcceptTheNextOne_WhenTheAppPausesMidDrag`
- [ ] 👁 On a device, drag a block, press Home or take a call, come back: the block sits on a cell and the board takes the next drag.

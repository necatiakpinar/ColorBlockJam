# Gameplay, timer, coins, popups and the five levels QA

Stub created by the task breakdown; filled by tasks T-09–T-22, T-32. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Rules

- [ ] ⚙ Blocks stop at walls and at other blocks — `BoardStateTests.TryStep_ShouldStopAtWall_WhenNoDoorIsThere`, `TryStep_ShouldStopAtAnotherBlock_WhenItIsInTheWay`
- [ ] ⚙ A block leaves through a matching door that covers it — `BoardStateTests.TryStep_ShouldExit_WhenAMatchingDoorCoversTheBlock`
- [ ] ⚙ Wrong colour, narrow or offset doors keep it in — `BoardStateTests.TryStep_ShouldBeBlocked_WhenTheDoorColourDiffers` and siblings
- [ ] ⚙ A block cannot leave while another block stands in its way out — `BoardStateTests.TryStep_ShouldBeBlocked_WhenAnotherBlockStandsInTheWayOut`
- [ ] ⚙ The last exit clears the board — `BoardStateTests.IsCleared_ShouldBecomeTrue_WhenTheLastBlockExits`
- [ ] ⚙ A restart starts from the level, not the last attempt — `BoardStateTests.NewBoard_ShouldStartFromTheLevel_WhenAnEarlierAttemptMovedBlocks`

### Board and controls

- [ ] ⚙ The board draws floor, walls, doors, corners and every block — `GameplayBoardTests.Board_ShouldDrawEveryPartAndBlock_WhenGameplayStarts`
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
- [ ] ⚙ Pushing a block into a door it cannot use knocks it once per push and lets it spring back; a plain wall does nothing — `BlockDragControllerTests.Drag_ShouldRejectOncePerPush_WhenPushedIntoADoorOfAnotherColour`, `Drag_ShouldNotReject_WhenPushedIntoAPlainWall`, `GameplayBoardTests.Reject_ShouldKnockTheBlockAndLetItSpringBack_WhenPushedIntoAWrongDoor`
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

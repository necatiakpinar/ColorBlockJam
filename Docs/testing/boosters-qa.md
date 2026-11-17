# Boosters QA

Checks for [boosters.md](../General/boosters.md). `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Stock and shop

- [ ] ⚙ A new player owns one of each; using and buying are saved at once — `BoosterInventoryTests`
- [ ] ⚙ Buying takes the price, or changes nothing when the coins are short — `BoosterInventoryTests.Shop_ShouldTakeThePriceAndAddOne_WhenThePlayerCanPay`, `Shop_ShouldChangeNothing_WhenTheCoinsAreShort`
- [ ] ⚙ With none left the button shows "+", a tap pauses and offers one; Buy plays on — `BoosterFlowTests.EmptyBooster_ShouldOfferOneForCoinsAndPlayOn_WhenBought`
- [ ] 👁 The purchase popup fits the screen; Buy is greyed out without the coins; close resumes the timer.

### Using

- [ ] ⚙ Freeze holds the timer for 10 s of play, pauses with the level, does not stack, ends on restart — `LevelTimerFreezeTests`
- [ ] ⚙ Rocket, hammer and vacuum wait for a tap on a block, spend one, change the board and play on — `BoosterServiceTests`
- [ ] ⚙ Taking the last blocks wins the level — `BoosterServiceTests.Booster_ShouldWinTheLevel_WhenItRemovesTheLastBlocks`
- [ ] ⚙ A blasted block that falls apart splits into blocks that move and leave on their own — `BoardRemovalTests.RemoveCell_ShouldMakeEachPieceABlock_WhenTheBlockFallsApart`, `SplitPieces_ShouldMoveAndLeaveOnTheirOwn_WhenTheBlockFellApart`
- [ ] 👁 Blast the middle of a long block: both ends are drawn as separate blocks and can be dragged one by one.
- [ ] ⚙ Through the real HUD: hammer smashes, rocket rebuilds the block, freeze shows the ice — `BoosterFlowTests`
- [ ] 👁 Tapping the waiting booster again puts it back; tapping another switches; pause while choosing puts it back first.

### Looks

- [ ] 👁 While choosing: the title and hint show, the top bar and the other boosters fade, the scene around the board darkens, every block is outlined.
- [ ] 👁 Freeze: the ice frame and the meter under the timer show; the meter empties in 10 s and stops while paused.
- [ ] 👁 Hammer swells and breaks the block with a burst of cubes; vacuum lifts the colour's blocks away one after another; rocket leaves the rest of the block drawn correctly.

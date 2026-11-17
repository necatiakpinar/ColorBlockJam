# Audio QA

Checks for [audio.md](../General/audio.md). `⚙` = automated (test named), `👁` = manual.
Every case is checked on a device with the volume up unless it says otherwise.

## Cases

- [x] ⚙ Each cue plays its clip at its volume; nothing plays with the sound toggle off — `AudioServiceTests`
- [x] ⚙ Grab, exit, booster, win and fail each play their cue — `GameplaySoundsTests.Sounds_ShouldPlayTheirCue_WhenTheGameplayEventHappens`
- [x] ⚙ The tick plays from 10 s left down to 1 s, never before or at 0 — `GameplaySoundsTests.Tick_ShouldPlayOnlyInTheLastTenSeconds_WhenTheTimerChanges`
- [x] ⚙ The timer flashes red in the last 10 s and is normal again after a restart — `GameplayHudViewTests.Timer_ShouldFlashRed_WhenTenSecondsOrLessAreLeft`, `Timer_ShouldLoseTheWarning_WhenTheLevelRestarts`
- [x] ⚙ The shipped config has a clip for every cue — `AudioServiceTests.ShippedConfig_ShouldHoldAClipForEveryCue_WhenTheGameShips`
- [ ] 👁 Every button on Home, in the HUD and in every popup clicks once per tap.
- [ ] 👁 Grabbing a block, a block leaving through its door, a booster, a win and a fail each sound right and
  are not too loud next to each other.
- [ ] 👁 In the last 10 s the timer ticks every second and flashes red; a pause or the freeze stops both; a
  restart brings the normal timer back.
- [ ] 👁 Turning sound off in the settings or the pause popup silences everything at once; turning it on
  brings it back.

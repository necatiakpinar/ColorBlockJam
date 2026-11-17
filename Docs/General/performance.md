# Performance

> **Binding agent rules.** Read this page before changing per-frame work, the drag, the board views or
> anything that may allocate during play. The rules themselves are in `CLAUDE.md` ("Update loop", "GC
> allocations"); this page holds how they are checked and the numbers measured.

## 1. What is guaranteed by tests

| Guarantee | Test |
| --- | --- |
| No `Update`, `LateUpdate`, `FixedUpdate` or `ITickable` in the game's code; an exception needs a reason in the test's allow-list | `ArchitectureTests.ProjectTypes_ShouldHaveNoPerFrameCode_WhenNotAllowListed` |
| Dragging a block allocates **0 B**: 200 drags in the real Gameplay scene, each a one-cell move through input, board, signals and every view, after a warm-up | `DragAllocationTests` (Play Mode) |

The drag measurement reads `GC.GetAllocatedBytesForCurrentThread` around the handlers only, so the editor's
own allocations between frames do not count. Idle play runs none of the game's code per frame (first row);
what does run per frame is DOTween's tween update and UniTask's player loop.

## 2. Profiler markers

| Marker | Where |
| --- | --- |
| `ColorBlockJam.Drag` | `BlockDragController`, every pointer event while input is accepted |
| `ColorBlockJam.Move` | `ActiveBoard.TryStep`, every one-cell step and exit |

Find them in the Profiler's CPU module (Hierarchy, search `ColorBlockJam`).

## 3. Measured

**Editor, Play Mode.**
- **Setup.** Game view 1170x2532, Gameplay at level 26, which plays level 6's layout. `ProfilerRecorder`
  over 92 frames, 2026-11-17.

| Counter | Value |
| --- | --- |
| Main thread | 16.67 ms average (the 60 FPS cap of `ApplicationConfig`), 19.15 ms worst |
| Batches / draw calls | 173 |
| SetPass calls | 8 (the SRP Batcher groups the block materials) |
| Triangles | 34.6 k |
| GC allocated per frame | 76 KB, **editor included**. Not a game number: in Play Mode the counter also holds the editor's own work. |

**Level solver.** The 20 shipped levels solve in about 0.2 s together, and a generated level takes about
13 ms ([level-editor.md](level-editor.md) §3). Editor-only.

## 4. Not measured yet

- **On a device.** A full level profiled on an Android device (T-38): frame time, GC per frame without the
  editor's share, battery and thermal behaviour. Needs a development build (T-41) and a device.
- **Draw calls.** 173 draw calls is fine with 8 SetPass calls on mid-range hardware. If a device shows the
  CPU render thread as the bottleneck, merge the studs into each block's mesh first.

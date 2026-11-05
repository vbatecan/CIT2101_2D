# Door entrance animation

Implemented in the shared `Assets/Scenes/CaseManager.unity` scene for all three cases. Other scenes are unchanged.

| Case | Primary entrance | Secondary entrance |
| --- | --- | --- |
| 1 | Vince | Jane |
| 2 | Charl Vonn | Paul |
| 3 | Shanaia | Shan |

Enter Play Mode with the desired case selected. The sequence automatically selects that case's walk cycles when the existing investigation table is shown after a case load. To replay during Play Mode, use **Door Entrance Sequence > DoorEntranceSequence component menu > Replay Entrance (Play Mode)**. This does not reload the case. Existing keyboard shortcuts 1, 2, and 3 can load the cases in the shared scene.

The left door opens over 0.65 seconds; the primary character enters first and the secondary follows 1.2 seconds later. Both walk behind the existing table, blend into their existing seated portraits, and the door closes. Total duration is about 7.6 seconds. Opening dialogue starts only after the sequence finishes. Existing character displays continue to own their portraits and expressions. Timer rules, scoring, case data, and progression are unchanged. The sequence pauses its presentation while another UI panel is displayed. Case reloads restart it; disabling the component restores the presentation.

GameBootstrap prepares the interrogation target on OnCaseLoaded, hides the previous dialogue without completing its node, and waits for the entrance's pending/playing state to clear before displaying the opening node. This follows animation completion rather than a fixed delay. Loading or retrying a case cancels any earlier pending opening. Scenes without a configured entrance retain immediate dialogue startup. The original two-argument SetInterrogationTarget API retains its immediate behavior for existing interactive callers.

The scene contains authored Primary/Secondary doorway/arrival markers, walking renderers, and a door leaf. Move these markers to tune paths. Timing, frame rate, and step bob are Inspector settings on Door Entrance Sequence. Case Artwork stores each case's frames and displayed heights. The scene's existing camera/background transform edits were preserved.

Assets:
- `Assets/Assets/CHARACTERS/DoorEntrance/VinceWalk.png`: eight transparent frames in a 4 by 2 sheet.
- `Assets/Assets/CHARACTERS/DoorEntrance/JaneWalk.png`: eight transparent frames in a 4 by 2 sheet.
- `Assets/Assets/CHARACTERS/DoorEntrance/VonnWalk.png`: Case 2 primary, eight transparent frames.
- `Assets/Assets/CHARACTERS/DoorEntrance/PaulWalk.png`: Case 2 secondary, eight transparent frames.
- `Assets/Assets/CHARACTERS/DoorEntrance/ShaniaWalk.png`: Case 3 primary, eight transparent frames.
- `Assets/Assets/CHARACTERS/DoorEntrance/ShanWalk.png`: Case 3 secondary, eight transparent frames.
- Matching Unity sprite slices and two small sprite assets for the door leaf and doorway interior.
- `Assets/Scripts/Gameplay/DoorEntranceSequence.cs`: presentation playback and cleanup.

Generated with the built-in ImageGen tool using the six characters' existing side-view artwork as identity/style references. Original artwork and its import metadata were preserved. No external API key was used.

## Final generation prompts

The four additional Case 2/3 prompts are recorded in [DoorEntrance-Case2-Case3-Prompts.md](DoorEntrance-Case2-Case3-Prompts.md).

### Vince

Use case: stylized-concept. Asset type: transparent game sprite sheet for Unity 2D. Reference image is the identity and art style reference for Vince, not a background to preserve. Create an eight-frame WALK CYCLE of EXACTLY this young adult man, right-facing SIDE PROFILE, full body, curly black hair, black glasses, cream button shirt, brown suspenders, brown trousers and brown shoes. Precisely 4 columns x 2 rows of equal-sized cells, reading left to right top to bottom, one complete full-body figure per cell. All eight figures same size, same face, same camera, same baseline within each cell, centered horizontally. Frames: contact left foot forward, down, passing, up, contact right foot forward, down, passing, up. Clear alternating leg strides and opposing arm swings; realistic restrained walking, preserve identity and outfit. Clean anime linework and soft warm cel shading matching reference. Wide sheet, no labels, no grid, no extra objects, no shadows, NO background: actual alpha transparency. Adequate padding in each cell. Every head and shoe entirely inside its own cell.

### Jane

Use case: stylized-concept. Asset type: transparent game sprite sheet for Unity 2D. Reference image is identity and art style for Jane. Create eight-frame WALK CYCLE of EXACTLY this young adult woman, right-facing SIDE PROFILE, full body, long dark brown hair, black glasses, sleeveless black halter top, charcoal jeans, black sneakers with white soles. Precisely 4 columns x 2 rows equal-sized cells, reading left to right top to bottom, one complete full-body figure per cell. All eight figures same size, same face, same camera, same baseline within each cell, centered horizontally. Frames: contact left foot forward, down, passing, up, contact right foot forward, down, passing, up. Distinct alternating leg strides, opposing arm swings and subtle hair movement. Natural restrained walk, preserve identity and outfit. Clean anime linework, soft warm cel shading matching reference. Wide sheet, no labels, no grid, no extra objects, no ground shadows, actual alpha transparency. Adequate padding, every head and shoe entirely inside its own cell.

## Verification

Unity 6000.3.27f1 compiled the new component and live reflection confirmed it loaded. Play Mode checks covered the door opening, walking with table occlusion, automatic replay through OnCaseLoaded, and completion restoring both seated renderers at full opacity, the closed background, and hidden walking/door layers. Console queries found no errors or warnings. No project test suites were created or run.

The Case 2/3 extension was verified by loading Case 2, Case 3, then Case 1 through GameBootstrap.LoadLevel. The automatic entrance selected Vonn/Paul, Shanaia/Shan, and Vince/Jane respectively. Both additional cases finished with the correct authored seated sprites, full opacity, the closed door, and hidden walking renderers. Switching back to Case 1 also restored its correct walk cycles and allowed CharacterDisplay to continue updating dialogue expressions. The updated Assembly-CSharp DLL was newer than the changed script; final Console queries found no errors or warnings.

Screenshots are in `Captures/door-opening.png`, `Captures/door-walking.png`, and `Captures/door-live-replay.png`.

Additional previews: `Captures/door-case2-walking.png` and `Captures/door-case3-walking.png`.

Dialogue timing fix: live checks in all three cases confirmed no active dialogue node, typing, or open dialogue panel while characters walk, followed by NODE_01 only after the entrance ends with both seated sprites fully visible. Retry and switching from Case 3 to Case 2 during an entrance also deferred dialogue correctly and selected only the new case's opening. Screenshot: `Captures/door-delayed-dialogue-walking.png`. Compilation was confirmed by the loaded three-argument API and updated DLL. No Console errors were reported; Unity reported TMP word-wrapping deprecation warnings and missing-script warnings during reload. This timing change did not modify or save scenes or assets.


# Dialogue, button icons and character expressions

## UI changes

`DialogueUI` converts legacy character banners to one reusable Canvas panel at runtime. The panel uses editable `Custom Dialogue Appearance` fields for surface/ink colors and normalized anchors. The default paper opacity is 86%, and its anchors span from 3.5% to 18.5% vertically (leaving a clean bottom bezel so it does not touch the lower-most screen edge, limiting panel height to 15% of the screen) and from 14% to 86% horizontally. Speaker and body text remain opaque. Existing scene and prefab references stay in place; no scene or prefab assets were edited.

The speaker name is text rather than part of the banner image. The custom panel has no Continue button: the existing click-anywhere, Space and Enter input handles dialogue advancement. Clicking while typing reveals the line; challenge and advancement rules still use the existing interrogation state.

Both dialogue presenters now parse TMP text once per line and reveal it through `maxVisibleCharacters`. Character pacing uses unscaled elapsed time instead of allocating a new string and wait instruction for every character. No frame-rate or loading-time benchmark was performed.

Compact buttons retain their authored icon on a separate non-raycasting Image. Notebook tabs are explicitly treated as compact buttons. Folder click targets are excluded from global surface styling, and Canvas-backed folder buttons render the existing folder sprite as an Image. Close controls are classified before folder cards, preserving contrast inside the folder modal.

Button surfaces now use warm parchment for navigation and compact controls, light brass for primary actions, and pale terracotta for destructive actions. Hover and pressed states stay in this palette. Button labels use opaque dark ink for contrast, and the shared action helper uses the same light surface colors.

Automatic icon preservation excludes Unity's `UISprite` and `Background` surfaces as well as the generated rounded surface. These are button backgrounds, not pictograms. This prevents white squares from covering Mute, Reset Defaults and Back to Menu labels while preserving authored notebook icons. Previously captured background icons are cleared and their generated Image is disabled when styling is reapplied.

## Generated artwork

The following assets live in `Assets/Assets/CHARACTERS/Expressions/` and are mapped in `Assets/Resources/CharacterExpressions.asset`. This batch adds 20 expression sprites across nine transparent PNGs. Existing neutral portraits and Vince's earlier Defensive, Nervous and Shocked sprites remain available.

| File | Character | Expressions, left to right for sheets | Reference art |
| --- | --- | --- | --- |
| `charl_expressions.png` | Charl Vonn Pascual | Defensive, Nervous, Shocked | `Vonn Pascual.png` |
| `paul_expressions.png` | Paul Gabriel Camacho | Smug, Angry, Shocked | `Paul Camacho.png` |
| `paul_defensive.png` | Paul | Defensive | `Paul Camacho.png` |
| `shanaia_expressions.png` | Shanaia Ortega | Nervous, Angry, Shocked | `Shania Ortega.png` |
| `shanaia_defensive.png` | Shanaia | Defensive | `Shania Ortega.png` |
| `jane_expressions.png` | Jane Arie Reyes | Curious, Thinking, Surprised, Sad | `Jane Reyes.png` |
| `shan_expressions.png` | Shan Jaraba | Thinking, Curious, Sad | `Shan Jaraba.png` |
| `shan_nervous.png` | Shan | Nervous | `Shan Jaraba.png` |
| `vince_angry.png` | Vince Batecan | Angry | `Vince Batecan.png` |

Reference art is under `Assets/Assets/CHARACTERS/`. Jane uses the existing story/profile ID `CHAR_CASE1_FEMALE`. PNG metadata provides named slices and pixels-per-unit values based on each original portrait's height. Generated art may vary slightly in silhouette and proportions; the original portraits were not overwritten.

## Story references and reusable prompt set

Narrative direction came from:

- `docs/story/levels/CHARACTERS_AND_CAST.md`
- `docs/story/levels/LEVEL_01_THE_MISSING_NECKLACE.md`
- `docs/story/levels/LEVEL_02_THE_SHATTERED_MIRROR.md`
- `docs/story/levels/LEVEL_03_THE_LAST_CALL.md`

Expression names were matched to the enums already used by dialogue and contradiction reactions. Additional sheet expressions remain available for future authored dialogue.

The following is a normalized reusable prompt set, not a verbatim generation log. Each output used the corresponding original portrait as its visual reference and requested transparent output through the built-in image-generation tool.

Common prompt:

> Create expression variants for this Case Closed character, using the supplied portrait as the identity and style reference. Preserve the character's face, hairstyle, eyewear, clothes, colors, clean anime line work, seated torso framing and viewpoint. Change facial expression and subtle posture only. Use an actual transparent background, with no scenery, labels, text or watermarks. For a sheet, place each expression in an equally spaced horizontal cell at the same scale, with aligned head heights and clear separation. Keep the complete head and upper torso inside each cell.

Append the relevant variation:

| Output | Expression direction |
| --- | --- |
| Charl sheet | Guarded denial with tense brows; anxious testimony as his shift evidence undermines him; shocked composure breaking when the contradiction is exposed. Keep his glasses and blue shirt. |
| Paul sheet | Overconfident dismissive grin; angry outrage when his insurance scheme is exposed; shocked loss of confidence. Keep his black turtleneck and tousled hair. |
| Paul Defensive | Tight mouth, suspicious eyes and defensive tension as he diverts questions about the gallery and policy. |
| Shanaia sheet | Nervous concealment of her secret meetings; angry resistance to accusations; shocked reaction to the recording and termination evidence. Keep her original outfit and hairstyle. |
| Shanaia Defensive | Guarded, defensive denial before the evidence catches up with her. |
| Jane sheet | Curious observation; thoughtful recollection of what she witnessed; surprised realization; sad concern. Preserve her original identity and outfit. |
| Shan sheet | Thinking while recalling the cafe encounter; curious observation; sad concern about the victim. Preserve his original hair, glasses and outfit. |
| Shan Nervous | Uneasy testimony while recounting the meeting and surveillance evidence. |
| Vince Angry | Irritated, angry resistance under questioning, preserving his original clothing, hair and facial identity. |

## Cleanup and verification

Deleted only the unreferenced test background `Assets/Assets/TEST_ASSETS/Interrogation Room.jpg` and its metadata (2,382,834 image bytes). Its GUID and runtime-path reference checks found no usages. Other test assets still have prefab or recovery-scene references and were retained. New artwork adds storage, so this is not a claim that the overall project size decreased.

Verification used Unity MCP in the existing CIT2101_2D editor, source/caller review and Git diff checks. Unity compiled the changed scripts, completed its reload and reported readiness. Bounded Console queries found no game-script errors or NullReferenceException entries; historical Unity AI Assistant tracing entries remain in the Console and were not cleared.

Read-only asset inspection confirmed the sprite slices and expression-library references. Speaking nodes in all three authored cases resolve their non-neutral expression mappings; Neutral and Calm retain their original pose fallback. Case 2 gameplay showed the notebook icons, folder opening/closing, the new dialogue panel, and Paul's generated angry expression. The generated Continue button was subsequently removed at the user's request; existing captures predate that removal. No unit tests or scene rebuilders were used. Full playthroughs of Cases 1 and 3 and visual review of every expression are outside these checks.

Review captures are in `Captures/`: `notebook-after.png`, `custom-dialogue.png`, `folder-after.png`, and `expression-dialogue.png`. The dialogue capture confirms 86% surface opacity and bottom anchors `(0.14, 0)` to `(0.86, 0.20)`; the folder capture confirms its visible close symbol. The newer `light-buttons-dialogue.png` shows the light button palette and removal of Continue. A fresh gameplay check confirmed zero buttons inside the custom dialogue panel and no generated Next reference. The changed scripts compiled, and the editor was returned to idle with CaseManager still loaded.

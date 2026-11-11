# Requirements from the CIM and CCP Finals recordings

## 1. Product purpose and intended users
Case Closed is an existing Unity detective game for players investigating cases. This change adds an illustrated startup screen and adjusts ambient gameplay walkers; it does not reconstruct the drawing app or Messenger interface visible in the recordings.

## 2. User journey
Cold startup shows the empty investigation room, a closed suitcase, rotating fan, subtle light variation, and `PRESS ANY KEY TO START`. A fresh keyboard press or primary pointer press opens the existing case selection. Later returns to Main Menu expose the existing New Case, Settings, and Quit controls. Selecting a case retains the story intro, three-second reading hold, entrance animation, investigation, and dialogue.

The direct case-selection destination and startup-only lifetime are explicit user decisions, not demonstrated navigation in the recordings.

## 3. Screen inventory
- Opening: existing warm yellow room artwork, teal side walls, observation window, wooden table; centered closed suitcase on the table; fan above window; ivory prompt below. No people, HUD, hand pointer, or menu buttons overlay this screen. Aspect-fit presentation preserves the composition with letterboxing.
- Existing Main Menu: preserve current controls, settings, and exit confirmation on later visits.
- Existing case selection and investigation: preserve hierarchy and rules. Gameplay window silhouettes use 40% peak opacity and a ten-second crossing (implementation defaults approved by the user).

## 4. Actions and system responses
- Release startup input, then press a key or primary pointer: continue once to case selection. Held input and repeat activation cannot trigger multiple loads.
- If case selection cannot be loaded, existing MainMenuUI falls back to its authored case-selection panel.
- Back from case selection: existing menu, without replaying the opening. Settings and Quit retain their established behavior.
- Opening idle: fan frames animate at ten frames/second; localized fan/light brightness varies smoothly over a restrained 94–100% range. No full-screen flashes or new sound effect.

## 5. Data and business rules
Use existing Unity scenes, Sprite assets, fan artwork, briefcase first frame, MainMenuUI navigation, case data, progression, and PlayerPrefs. Only a transient startup flag is added; it resets on a fresh application/Editor Play session. No backend, new persistence, scoring changes, or unlock rules are required by this evidence.

## 6. Open questions and provenance
- NON-BLOCKING: `XFhtdTsftOC.ogg` yielded no recognized speech; its intended use is unknown. It is not assigned to a speculative action.
- NON-BLOCKING: exact flicker timing, suitcase size, walker opacity/speed, and continuation destination are not numerically specified by the media. Values above are documented implementation choices, with navigation and walker defaults approved by the user.
- Machine transcription is a draft, not a verified verbatim transcript. A local whisper.cpp v1.9.2 multilingual small-q5_1 model processed local 16 kHz mono WAVs. No recordings were uploaded. Recognition errors include fan being rendered as “pan”/“fun”; repeated instructions, visible annotations, and user clarifications resolve the implemented scope.
- Sources are actually stored directly under `Assets/References`, not the prompt's `references/originals`. Review derivatives and transcription tools are ignored temporary artifacts under `Temp/ReferenceReview`.

## 7. Evidence and verification map
| Requirement | Source filename + timestamp | Confidence | Implementation / check |
| --- | --- | --- | --- |
| Room as opening | CIM and CCP Finals _ Messenger(1).mp4, 00:00–00:13 | High: visual + draft speech | MainMenu OpeningScreen; compare full frames |
| Press any key | Same, 00:08–00:13; CIM and CCP Finals _ Messenger(2).mp4, 00:02–00:06 | High: repeated draft speech | OpeningScreenController input gate |
| Flat suitcase on table | Recording (1), 00:13–00:22; recording (2), 00:06–00:12 | High: annotations + draft speech | Existing suitcase first sprite |
| Fan rotation | Recording (1), 00:22–00:25; recording (2), 00:12–00:18 | High: repeated draft speech | Existing fan frame cycle |
| Light flicker | Recording (1), 00:25–00:28 | User confirmed | Restrained localized brightness variation |
| No people on opening | Recording (2), 00:18–00:23 | High: draft speech + user confirmed scope | Static room composite |
| Lighter/slower gameplay walkers | CIM and CCP Finals _ Messenger.mp4, 00:00–00:15 | Draft speech + user confirmation | BackgroundPeopleMotion scene settings |
| Direct dossiers, later menu access | User clarification | Confirmed product decision | Existing navigation + transient startup state |
| Extra sound behavior | XFhtdTsftOC.ogg, 00:00–00:01.67 | Unresolved | No speculative assignment |

Full filenames for abbreviated rows: recording (1) is `CIM and CCP Finals _ Messenger(1).mp4`; recording (2) is `CIM and CCP Finals _ Messenger(2).mp4`.

### Verification procedure
Use Unity Play Mode, as selected by the user; no NUnit infrastructure. Check startup, release/press gate, duplicate input, case selection, Back, Settings, exit confirmation cancellation, story intro, empty-room reveal, entrance, dialogue, and lighter/slower walkers. Inspect 16:9 and 4:3, Console errors, and scoped diffs. Results are recorded below after verification.

### Recorded results
- Opening rendered in Play Mode with room, static closed suitcase, animated fan, and prompt; screenshot: ignored `Temp/ReferenceReview/opening-final.png`.
- Continue through the real controller reached LevelSelect; Back exposed existing New Case, Settings, and Quit controls without replaying the opening. Settings and exit confirmation opened and were closed/cancelled successfully.
- Case 1 launch passed the story-intro gate and entered gameplay. Runtime scene inspection confirmed walker opacity 0.4 and crossing duration 10 seconds. No Console errors at the end of this check.
- Desktop input automation is unavailable in this session. Navigation was exercised via real controller/button events in Play Mode, not physical key injection; held-input/physical keyboard checks remain a manual follow-up.
- Free-aspect Game View was visually inspected with aspect-fit room composition. A separate 4:3 capture and a standalone player build were not performed. No unit tests, commits, or deployment were performed.

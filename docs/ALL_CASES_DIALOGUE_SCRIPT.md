# Case Closed: Complete Dialogue Scripts

All three prototype cases use the same 3 to 5 minute structure:

1. One evidence item is available when the case opens.
2. Completing the first dialogue lead reveals evidence 2.
3. Completing the second dialogue lead reveals evidence 3.
4. The final statement can be challenged with evidence 2.
5. A successful contradiction opens the confession and conclusion quiz.

Each case also includes two short detective-led beats (`NODE_01B_INTERVIEW` and `NODE_03B_CONFIRMATION`) so the player has time to read the statement, inspect the relevant evidence, and understand the deduction before the final challenge.

The dialogue bubble stays in its default panel position for suspect conversations. When evidence starts a dialogue, `DialogueUI.AlignToWorldTarget` moves the configured bubble above that evidence item.

## Case 01: The Missing Necklace

**Suspect:** Vince Angelo Batecan  
**Motive:** Gambling debt  
**Visible evidence:** Family Photograph  
**Evidence 2:** Broken Teacup, unlocked by `NODE_02_ROOM_LEAD`  
**Evidence 3:** Kitchen Pantry Log, unlocked by `NODE_03_TEACUP_LEAD`  
**Challenge:** Present Kitchen Pantry Log to `NODE_04_FINAL_ALIBI`  
**Confession:** `NODE_05_CONFESSION`

### Dialogue

`NODE_01` Vince: "I never went near the study. I stayed in the kitchen from 8:30 PM until everyone started shouting!"

`NODE_02_ROOM_LEAD` Vince: "The study was locked, Detective. There is nothing there that connects me to the necklace."

`NODE_03_TEACUP_LEAD` Vince: "That broken cup? I heard it fall, but I was nowhere near the safe. The pantry log will prove I was in the kitchen."

`NODE_04_FINAL_ALIBI` Vince: "I stayed in that kitchen the entire time. You cannot prove otherwise."

`NODE_05_CONFESSION` Vince: "W-what?! The kitchen pantry log? Fine! I needed money to clear my debts, so I took the necklace!"

**Player flow:** Click through `NODE_01` and `NODE_02_ROOM_LEAD`; inspect the Broken Teacup to show the evidence-linked bubble and lead; click through `NODE_03_TEACUP_LEAD`; inspect or present the Kitchen Pantry Log; solve the conclusion quiz.

## Case 02: The Shattered Mirror

**Witness:** Charl Vonn Pascual  
**Organizer:** Paul Gabriel Camacho  
**Motive:** Insurance fraud  
**Visible evidence:** Window Frame Crime Scene Photo  
**Evidence 2:** Security Guard Shift Log, unlocked by `NODE_02_WINDOW_LEAD`  
**Evidence 3:** Art Insurance Policy, unlocked by `NODE_03_SHIFT_LEAD`  
**Challenge:** Present Security Guard Shift Log to `NODE_04_FINAL_STATEMENT`  
**Confession:** `NODE_05_CONFESSION`

### Dialogue

`NODE_01` Charl: "I was standing right outside the office door when I heard the window shatter from the alley at 11:00 PM."

`NODE_02_WINDOW_LEAD` Charl: "The alley window was broken from outside. Check the frame if you doubt me."

`NODE_03_SHIFT_LEAD` Charl: "The shift log is routine. It will show I was near the office, exactly as I said."

`NODE_04_FINAL_STATEMENT` Charl: "I was outside that office at 11:00 PM. The keycard record cannot say otherwise."

`NODE_05_CONFESSION` Charl: "Fine! The shift log doesn't lie. I was at the East Gate. Mr. Paul Camacho paid me 2,000 credits to stage the break-in!"

**Player flow:** Inspect the window photo for the inside-break clue; complete the first two dialogue leads; inspect the shift log to align the bubble beside it; present the shift log during the final statement; use the insurance policy as supporting evidence in the conclusion.

## Case 03: The Prototype Drive

**Suspect:** Shanaia Ortega  
**Witness:** Shan Jaraba  
**Motive:** Retaliation after an intended termination  
**Visible evidence:** Victim's Smartphone Call Log  
**Evidence 2:** Coffee Shop CCTV Frame, unlocked by `NODE_02_PHONE_LEAD`  
**Evidence 3:** Termination Notice Draft, unlocked by `NODE_03_CCTV_LEAD`  
**Challenge:** Present Coffee Shop CCTV Frame to `NODE_04_FINAL_STATEMENT`  
**Confession:** `NODE_05_CONFESSION`

### Dialogue

`NODE_01` Shanaia: "Once our 5:30 PM meeting wrapped up, I went straight home. I didn't contact Kurt or return to the cafe for the rest of the night."

`NODE_02_PHONE_LEAD` Shanaia: "Kurt's phone contains nothing useful. You should focus on the meeting, not his private calls."

`NODE_03_CCTV_LEAD` Shanaia: "The back exit camera is unreliable. It could not possibly show me there after I left."

`NODE_04_FINAL_STATEMENT` Shanaia: "Once I left at 5:30 PM, I never returned to that cafe. That is the timeline."

`NODE_05_CONFESSION` Shanaia: "What?! You found the CCTV footage? Kurt was going to fire me and take my code! I snuck back in at 7:10 PM to take what belongs to me!"

**Player flow:** Inspect the smartphone log; complete the first two dialogue leads; inspect the CCTV frame to align the bubble beside it; present the CCTV frame during the final statement; use the termination draft and phone log in the conclusion.

## Scene Setup Checklist

- Keep evidence 1 `startsDiscovered = true`.
- Set evidence 2 and 3 `startsDiscovered = false`.
- Set `requiredDialogueNodeId` to the unlock node listed above.
- Set the evidence-linked `dialogueNodeToTriggerOnInspect` to the second lead node.
- Assign `DialogueUI.bubbleRect` to the bubble panel RectTransform.
- Tune `DialogueUI.bubbleScreenOffset` per evidence position so the bubble does not cover the item.

# Case 01 Dialogue Script: The Missing Necklace

## Target Playtime

3 to 5 minutes for a first playthrough. The player can read quickly, inspect the two unlocked evidence items, and challenge the final alibi within the five-minute case target.

## Evidence Visibility Rules

| Evidence | Initial state | Unlock condition | Story purpose |
| --- | --- | --- | --- |
| Family Photograph | Visible | Case start | Places Vince near the study door at 8:45 PM. |
| Broken Teacup | Hidden | Complete `NODE_02_ROOM_LEAD` | Points the investigation toward the safe and triggers the pantry lead when inspected. |
| Kitchen Pantry Log | Hidden | Complete `NODE_03_TEACUP_LEAD` | Proves the pantry was locked and disproves Vince's alibi. |

## Playable Sequence

The two detective-led prompts add investigation time without blocking the player with artificial delays: `NODE_01B_INTERVIEW` follows the opening alibi, and `NODE_03B_CONFIRMATION` follows the pantry lead before the final challenge.

### 1. Opening alibi: `NODE_01`

**Vince:** "I never went near the study. I stayed in the kitchen from 8:30 PM until everyone started shouting!"

Player action: Click Next.

Next node: `NODE_02_ROOM_LEAD`.

### 2. First lead: `NODE_02_ROOM_LEAD`

**Vince:** "The study was locked, Detective. There is nothing there that connects me to the necklace."

Player action: Click Next.

Reward: `EVD_BROKEN_TEACUP` appears on the investigation table. The bubble remains on the dialogue panel's default position until the player inspects the evidence.

### 3. Inspect the teacup: `NODE_03_TEACUP_LEAD`

Player action: Inspect the Broken Teacup. The bubble aligns above the clicked evidence item.

**Vince:** "That broken cup? I heard it fall, but I was nowhere near the safe. The pantry log will prove I was in the kitchen."

Player action: Click Next.

Reward: `EVD_KITCHEN_LOG` appears on the investigation table.

### 4. Final alibi: `NODE_04_FINAL_ALIBI`

**Vince:** "I stayed in that kitchen the entire time. You cannot prove otherwise."

Player action: Activate Challenge and present the Kitchen Pantry Log, or click the log while the dialogue bubble is open.

Result: The contradiction rule `RULE_VINCE_ALIBI_LIE` succeeds.

### 5. Confession: `NODE_05_CONFESSION`

**Vince:** "W-what?! The kitchen pantry log? Fine! I needed money to clear my debts, so I took the necklace!"

Result: The player has the suspect, motive, and evidence needed for the conclusion quiz.

## Unity Setup

1. Assign `DialogueUI.bubbleRect` to the panel RectTransform that should move beside evidence. Leave it empty to move the DialogueUI RectTransform itself.
2. Adjust `DialogueUI.bubbleScreenOffset` to keep the bubble clear of the evidence sprite.
3. Ensure the Broken Teacup table item has either its existing `dialogueNodeToTriggerOnInspect` field set to `NODE_03_TEACUP_LEAD`, or leave that field empty so it uses the value in its `EvidenceSO` data.
4. Keep the Case 01 initializer enabled for the prototype scene. The initializer creates the evidence and dialogue data at runtime.

The evidence unlocks are applied only when the player completes the relevant dialogue node, so opening or skipping directly to a node does not silently reveal later evidence.

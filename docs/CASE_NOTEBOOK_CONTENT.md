# Case Notebook Content

This is the writing content for the in-game Case File Notebook. Each case is organized as its own folder. Evidence marked **LOCKED** becomes visible only after the listed dialogue node is completed.

---

# Folder 01: The Missing Necklace

## Case Summary

**Case ID:** LEVEL_01  
**Location:** High-Society Manor Study  
**Time:** Stormy evening  
**Victim:** Kirby Raymundo, aristocrat and necklace owner  

**Incident:** A valuable family necklace disappeared from the manor safe during an evening gathering.

**Objective:** Interrogate Vince Angelo Batecan, inspect the table evidence, disprove his kitchen alibi, and uncover the truth.

**Working theory:** Vince used the storm and crowded gathering as cover to enter the study. His gambling debts provide a possible motive.

## Suspects

### Vince Angelo Batecan

**Age:** 25  
**Role:** Victim's nephew  
**Personality:** Defensive  

**Alibi:** Claims he stayed in the kitchen from 8:30 PM until everyone started shouting.

**Motive:** Urgent gambling debts owed to local loan sharks.

**Conflict:** Frequently argued with Kirby over his financial allowance.

### Janine Marie Sotto

**Age:** 24  
**Role:** Manor guest and key witness  
**Personality:** Observant  

**Alibi:** Sat in the dining room talking with guests until 9:00 PM.

**Relevance:** Noticed Vince arguing with Kirby before dinner and saw him running toward the garden.

## Evidence

### 1. Family Photograph

**Status:** DISCOVERED at case start  
**Time shown:** 8:45 PM  
**Description:** A photograph showing the study doorway.  
**Observation:** A silhouette matching Vince is visible near the study door.  
**Clue:** `CLUE_VINCE_AT_DOOR` - Vince was near the study at 8:45 PM.

### 2. Broken Teacup

**Status:** LOCKED  
**Unlocks after:** `NODE_02_ROOM_LEAD`  
**Description:** A teacup shattered inside the locked study beside the safe.  
**Observation:** The cup was broken during the break-in, close to the safe.  
**Clue:** The damage places activity near the safe rather than in the kitchen.  
**Inspection dialogue:** `NODE_03_TEACUP_LEAD`

### 3. Kitchen Pantry Log

**Status:** LOCKED  
**Unlocks after:** `NODE_03_TEACUP_LEAD`  
**Description:** The pantry log records that staff locked the kitchen from 8:30 PM to 9:15 PM.  
**Observation:** Vince could not have remained inside the kitchen during his claimed alibi.  
**Clue:** `EVD_KITCHEN_LOG_BASE_CLUE` - Vince's kitchen alibi is impossible.

## Clues and Deductions

- `CLUE_VINCE_AT_DOOR`: Vince was outside the study at 8:45 PM.
- `EVD_KITCHEN_LOG_BASE_CLUE`: The kitchen was locked during Vince's claimed alibi.
- `CLUE_VINCE_OUTSIDE_STUDY`: The photograph and pantry log place Vince outside the study while he was supposed to be in the kitchen.
- `CLUE_VINCE_STAGED_BREAKIN`: Vince confessed to staging the break-in for debt money.

## Conclusion

**Contradiction:** Present the Kitchen Pantry Log to `NODE_04_FINAL_ALIBI`.  
**Confession:** `NODE_05_CONFESSION`.  
**Answer:** Vince Angelo Batecan stole the necklace to pay his gambling debts.

---

# Folder 02: The Shattered Mirror

## Case Summary

**Case ID:** LEVEL_02  
**Location:** Upscale Art Gallery Back Office  
**Time:** 11:00 PM  
**Victim:** Paul Gabriel Camacho, gallery owner  

**Incident:** Paul claims an intruder entered through an exterior alley window and stole a priceless painting.

**Objective:** Interrogate Charl Vonn Pascual, inspect the glass evidence, expose the false testimony, and uncover the insurance fraud.

**Working theory:** The burglary was staged from inside the gallery. Paul may have arranged the theft to claim a larger insurance payout.

## Suspects

### Charl Vonn Pascual

**Age:** 30  
**Role:** Night security guard  
**Personality:** Calm  

**Alibi:** Claims he stood outside the office and heard the window shatter from the alley at 11:00 PM.

**Motive:** Was bribed by Paul to support the false burglary story.

### Paul Gabriel Camacho

**Age:** 42  
**Role:** Gallery owner and victim  
**Personality:** Secretive  

**Alibi:** Claims he was at home when the alarm triggered.

**Motive:** A large insurance payout could save his failing gallery.

## Evidence

### 1. Window Frame Crime Scene Photo

**Status:** DISCOVERED at case start  
**Description:** A photograph of the shattered back-office window from the alley.  
**Observation:** Glass shards are scattered outside on the alley pavement, proving the window was broken from inside.  
**Clue:** `CLUE_BROKEN_FROM_INSIDE` - The burglary was staged from inside the office.

### 2. Security Guard Shift Log

**Status:** LOCKED  
**Unlocks after:** `NODE_02_WINDOW_LEAD`  
**Description:** An electronic keycard log showing guard movements.  
**Observation:** Charl scanned at the East Gate at 11:00 PM, away from the office.  
**Clue:** Charl was not outside the office when the window broke.  
**Inspection dialogue:** `NODE_03_SHIFT_LEAD`

### 3. Art Insurance Policy

**Status:** LOCKED  
**Unlocks after:** `NODE_03_SHIFT_LEAD`  
**Description:** The gallery's insurance agreement for the stolen painting.  
**Observation:** Coverage was doubled to $500,000 only 48 hours before the incident.  
**Clue:** `EVD_INSURANCE_POLICY_BASE_CLUE` - Paul had a powerful financial reason to stage the burglary.

## Clues and Deductions

- `CLUE_BROKEN_FROM_INSIDE`: The window was broken from inside the office.
- `EVD_INSURANCE_POLICY_BASE_CLUE`: Paul recently increased the insurance value.
- `CLUE_INSURANCE_FRAUD_PROOF`: The inside break and sudden policy increase prove a staged insurance fraud.
- `CLUE_PAUL_STAGED_BURGLARY`: Charl admits Paul paid him to lie and stage the break-in.

## Conclusion

**Contradiction:** Present the Security Guard Shift Log to `NODE_04_FINAL_STATEMENT`.  
**Confession:** `NODE_05_CONFESSION`.  
**Answer:** Paul Gabriel Camacho organized the fake burglary for the insurance payout, using Charl as an accomplice.

---

# Folder 03: The Last Call

## Case Summary

**Case ID:** LEVEL_03  
**Location:** Downtown Coffee Shop Office  
**Time:** After hours  
**Victim:** Kurt Miguel Ancheta, startup founder  

**Incident:** Kurt's secret prototype drive disappeared from his bag after a late meeting.

**Objective:** Interrogate Shanaia Ortega, examine the phone log and CCTV, expose her false departure claim, and recover the prototype.

**Working theory:** Shanaia returned to the cafe after claiming she went home. She intended to take proprietary code before Kurt could terminate her.

## Suspects

### Shanaia Ortega

**Age:** 27  
**Role:** Lead software developer and business partner  
**Personality:** Calm  

**Alibi:** Claims she went straight home at 5:30 PM and never contacted Kurt or returned to the cafe.

**Motive:** Steal proprietary code before being fired.

### Shan Jaraba

**Age:** 29  
**Role:** Cafe manager and key informant  
**Personality:** Secretive  

**Alibi:** Working at the register until the 7:30 PM closing.

**Relevance:** Can confirm who accessed the cafe during the evening.

## Evidence

### 1. Victim's Smartphone Call Log

**Status:** DISCOVERED at case start  
**Description:** Call history extracted from Kurt's phone.  
**Observation:** It shows an unanswered 10-minute encrypted call from Shanaia at 7:15 PM.  
**Clue:** Shanaia contacted Kurt after claiming she had gone home.

### 2. Coffee Shop CCTV Frame

**Status:** LOCKED  
**Unlocks after:** `NODE_02_PHONE_LEAD`  
**Description:** A security camera frame from the cafe back exit.  
**Observation:** Shanaia's distinct jacket is visible entering the back door at 7:10 PM.  
**Clue:** `CLUE_SHANAIA_RETURNED` - Shanaia returned to the cafe 1 hour and 40 minutes after her claimed departure.  
**Inspection dialogue:** `NODE_03_CCTV_LEAD`

### 3. Termination Notice Draft

**Status:** LOCKED  
**Unlocks after:** `NODE_03_CCTV_LEAD`  
**Description:** A draft termination letter found inside Kurt's briefcase.  
**Observation:** Kurt planned to fire Shanaia for secretly selling company data to rival firms.  
**Clue:** `EVD_RESIGNATION_LETTER_BASE_CLUE` - Shanaia had a direct professional motive.

## Clues and Deductions

- `CLUE_SHANAIA_RETURNED`: CCTV places Shanaia at the cafe at 7:10 PM.
- `EVD_RESIGNATION_LETTER_BASE_CLUE`: Kurt planned to terminate Shanaia.
- `CLUE_PROTOTYPE_THEFT_TIMELINE`: CCTV and the encrypted call place Shanaia at the cafe near the theft time.
- `CLUE_SHANAIA_CONFESSED`: Shanaia admits returning to steal the prototype drive.

## Conclusion

**Contradiction:** Present the Coffee Shop CCTV Frame to `NODE_04_FINAL_STATEMENT`.  
**Confession:** `NODE_05_CONFESSION`.  
**Answer:** Shanaia Ortega returned to steal the prototype before Kurt could fire her.

---

# Notebook Display Rules

- The Summary tab displays the case summary and objective.
- The Suspects tab displays all known suspect and witness profiles.
- The Evidence tab displays only evidence IDs contained in `CaseManager.discoveredEvidenceIds`.
- The Clues tab displays only clues contained in `CaseManager.unlockedCluesText`.
- Locked evidence should not appear in the Evidence tab until its required dialogue node is completed.
- Evidence inspection may trigger the linked dialogue bubble beside the clicked table item.

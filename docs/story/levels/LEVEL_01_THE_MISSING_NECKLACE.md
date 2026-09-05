# Level 1: The Missing Necklace (Story & Screenplay Bible)

**Case ID:** `LEVEL_01`  
**Internal Reference:** `Case001`  
**Incident:** Grand Larceny & Staged Manor Break-In  
**Setting:** High-Society Manor Study & Kitchen  
**Atmosphere:** A violent thunderstorm lashes the French casement windows; thunder rumbles as a brass grandfather clock ticks solemnly.  
**Target Playtime:** 3 to 5 Minutes  
**Primary Suspect:** Vince Angelo Batecan  
**Secondary Witness:** Jane Arie Reyes  
**Victim:** Kirby Raymundo (Aristocrat, Manor Patriarch)  
**Lead Investigator:** Assigned Detective  

---

## 1. Narrative Synopsis

During an upscale dinner gathering at the Raymundo country estate, the Raymundo family heirloom—a Victorian sapphire-and-diamond necklace valued at over 150,000 credits—is stolen from the reinforced wall safe inside the private study. 

The house was sealed due to torrential rain. No external doors were breached. Patriarch Kirby Raymundo immediately locked down the estate and summoned the police, convinced the thief is someone under his own roof.

Suspicion immediately centers on Vince Angelo Batecan, Kirby's estranged 25-year-old nephew. Vince was recently cut off from the family trust fund due to catastrophic debts incurred at underground gambling parlors. He claims he was nowhere near the study, insisting he spent the entire window between 8:30 PM and 9:15 PM raiding snacks and nursing tea in the manor kitchen.

However, physical evidence scattered across the estate tells a very different story.

---

## 2. Dramatis Personae

### Vince Angelo Batecan (Primary Suspect)
* **Age:** 25
* **Occupation:** Unemployed heir / High-stakes gambler
* **Relationship to Victim:** Nephew
* **Visual Asset:** `Vince.png`
* **Personality:** Defensive, irritable, cornered. Feigns indignation when questioned about his financial troubles.
* **Motive:** Local loan sharks issued a strict midnight deadline to settle a 60,000 credit debt or face physical violence. The necklace was his only escape ticket.
* **Alibi:** *"I was in the kitchen from 8:30 PM until everyone started screaming about the safe. Check the kettle if you don't believe me."*
* **Psychological Tell:** Taps his index finger rapidly on the table and shifts gaze toward the door whenever the word "safe" or "study" is uttered.

### Jane Arie Reyes (Secondary Witness)
* **Age:** 24
* **Occupation:** High-society guest & freelance journalist
* **Relationship to Victim:** Family acquaintance
* **Visual Asset:** `Jane.png`
* **Personality:** Observant, composed, articulate. Keeps a close eye on social dynamics.
* **Role in Mystery:** Observed Vince and Uncle Kirby engaged in a vicious screaming match in the foyer right before dinner (around 8:15 PM). Noticed Vince quietly slipping away from the reception area around 8:40 PM.

### Kirby Raymundo (Victim)
* **Age:** 58
* **Occupation:** Aristocrat & real estate magnate
* **Visual Asset:** `Kirby.png`
* **Personality:** Proud, imperious, unforgiving. Believes discipline is the only remedy for Vince's recklessness.

---

## 3. Evidence & Forensic Dossier

| Evidence ID | Name | Category | Initial State | Unlock Condition | Forensic Significance |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `EVD_FAMILY_PHOTO` | **Family Photograph** | Photograph | **Discovered** | Case Start | Framed polaroid snapped at 8:45 PM in the gallery hallway. Shows guests socializing, but a silhouette matching Vince's build and suit jacket is clearly reflected in the hallway mirror standing outside the study door. |
| `EVD_BROKEN_TEACUP` | **Broken Teacup** | Physical Clue | **Locked** | `NODE_02_ROOM_LEAD` | Fine porcelain teacup shattered directly in front of the open safe. Porcelain fragments are trampled, proving someone knocked it over in frantic haste during the theft. |
| `EVD_KITCHEN_LOG` | **Kitchen Pantry Log** | Document | **Locked** | `NODE_03_TEACUP_LEAD` | Official estate logbook signed by Head Chef Raymond. Confirms the kitchen and pantry were padlocked from 8:30 PM to 9:15 PM for inventory counting. Vince could not possibly have been inside. |

---

## 4. Complete Screenplay & Interrogation Dialogue

### Scene Setup
* **Camera:** Fixed investigation POV looking across the mahogany table.
* **Characters:** Vince Angelo Batecan sits on the left (`Character_Suspect_Left`). Jane Arie Reyes sits on the right (`Character_Suspect_Right`).
* **Table Items:** Family Photograph sits face-up near the center blotter.
* **Audio:** Low ambient rain, intermittent distant thunder, rhythmic clock ticking.

---

### [BEAT 1: The Opening Alibi]

**NODE_01**  
*Speaker:* Vince Angelo Batecan  
*Expression:* **Defensive**  
*Audio:* Tension chord  
> **VINCE:**  
> "I never went near the study! I stayed in the kitchen from 8:30 PM until everyone started shouting! Why are you staring at me when anyone at this dinner could have picked that lock?!"

**NODE_01B_INTERVIEW (Detective Beat)**  
*Speaker:* Detective  
*Expression:* Neutral  
> **DETECTIVE:**  
> "Nobody mentioned picking locks, Vince. But you're very quick to establish an alibi. Let's talk about the study itself."

---

### [BEAT 2: The Locked Room Claim]

**NODE_02_ROOM_LEAD**  
*Speaker:* Vince Angelo Batecan  
*Expression:* **Defensive**  
> **VINCE:**  
> "The study was locked, Detective. Uncle Kirby keeps the only brass key on his vest chain. There is nothing in that room that connects me to the necklace."

*(System Action: `NODE_02_ROOM_LEAD` completed. Unlocks `EVD_BROKEN_TEACUP` onto the table.)*

---

### [BEAT 3: Inspection of Physical Clue]

*(Player Action: Inspects the Broken Teacup on the desk. Camera zooms to Top-Down 360° inspector view. The dialogue bubble smoothly anchors above the teacup.)*

**NODE_03_TEACUP_LEAD**  
*Speaker:* Vince Angelo Batecan  
*Expression:* **Nervous**  
*Animation:* Swallows hard, avoids eye contact.  
> **VINCE:**  
> "That broken cup? I... I heard it fall from down the hall, but I was nowhere near the safe! The pantry log will prove I was raiding snacks in the kitchen!"

*(System Action: `NODE_03_TEACUP_LEAD` completed. Unlocks `EVD_KITCHEN_LOG` onto the table.)*

**NODE_03B_CONFIRMATION (Detective Beat)**  
*Speaker:* Detective  
*Expression:* Neutral  
> **DETECTIVE:**  
> "A shattered teacup right next to the open safe. You claim you heard it fall from down the hall, yet your story insists you were secluded in the kitchen. Let's test that timeline against the staff records."

---

### [BEAT 4: The Fatal Lie & Contradiction Point]

**NODE_04_FINAL_ALIBI** *(Challengeable Statement)*  
*Speaker:* Vince Angelo Batecan  
*Expression:* **Defensive**  
> **VINCE:**  
> "I stayed in that kitchen the entire time! From eight-thirty to quarter past nine! You have no witnesses and you cannot prove otherwise!"

*(Gameplay: Player clicks [Challenge] and presents `EVD_KITCHEN_LOG` (Kitchen Pantry Log).)*

---

### [BEAT 5: The Contradiction Sting & Confession]

*Audio:* **Dramatic Contradiction Sting** (`SFX_Objection_Chord`)  
*Visual:* Camera shake, screen flash. Vince recoils in terror.  
*Rule Triggered:* `RULE_VINCE_ALIBI_LIE`  

> **DETECTIVE:**  
> "You claim you were in the kitchen from 8:30 to 9:15 PM, Vince. But this pantry log tells a very different story. Head Chef Raymond locked the kitchen doors from the outside at 8:30 on the dot for the bi-weekly silver audit. The doors didn't open until 9:15!"

> **VINCE:**  
> "Wait... the kitchen log shows it was locked by staff?! I... I..."

**NODE_05_CONFESSION**  
*Speaker:* Vince Angelo Batecan  
*Expression:* **Shocked / Nervous Breakdown**  
*Audio:* Melancholic piano theme  
> **VINCE:**  
> "W-what?! The kitchen pantry log was signed?! Fine! Damn it, FINE! I needed money to clear my debts before midnight, or those loan sharks were going to break my legs! Uncle Kirby refused to give me a single cent! When the thunder struck at 8:45, I used a duplicate key I stole last week to enter the study. I knocked over the tea tray in the dark, grabbed the necklace, and slipped it into my coat pocket! Please... don't let Uncle Kirby send me to prison!"

---

## 5. Deduction Board Synergy

* **Clue A:** `CLUE_VINCE_AT_DOOR` (*"Silhouette matching Vince spotted near study doorway at 8:45 PM."*)
* **Clue B:** `EVD_KITCHEN_LOG_BASE_CLUE` (*"Kitchen pantry was locked by staff from 8:30 PM to 9:15 PM; Vince could not have been inside."*)
* **Deduction Result:** `CLUE_VINCE_OUTSIDE_STUDY`
* **Deduction Synthesis Text:** *"The kitchen log proves Vince was never inside the kitchen, while the hallway photo places him directly outside the study door at 8:45 PM!"*

---

## 6. Case Conclusion Quiz (Accusation Phase)

| Question # | Question Prompt | Options | Correct Answer | Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Q1 (Culprit)** | Who stole Kirby Raymundo's necklace? | A) Vince Angelo Batecan<br/>B) Jane Arie Reyes<br/>C) House Staff | **A) Vince Angelo Batecan** | Vince confessed after his alibi collapsed under material scrutiny. |
| **Q2 (Motive)** | What was Vince Angelo Batecan's motive? | A) Jealousy<br/>B) Gambling debts<br/>C) Revenge | **B) Gambling debts** | Vince owed urgent debts to loan sharks with a midnight deadline. |
| **Q3 (Alibi)** | Where did Vince claim he remained during the incident? | A) The Kitchen<br/>B) The Garden<br/>C) The Study | **A) The Kitchen** | He claimed he was raiding snacks in the kitchen between 8:30 and 9:15 PM. |
| **Q4 (Evidence)** | Which evidence most strongly disproved Vince's kitchen alibi? | A) Kitchen Pantry Log<br/>B) Broken Teacup<br/>C) Family Photograph | **A) Kitchen Pantry Log** | The log showed the kitchen doors were locked from the outside by staff during the entire window. |
| **Q5 (Witness)** | Who saw Vince and Kirby arguing before dinner? | A) Jane Arie Reyes<br/>B) Charl Vonn Pascual<br/>C) Shan Jaraba | **A) Jane Arie Reyes** | Jane witnessed their heated exchange regarding money in the foyer at 8:15 PM. |

---

## 7. Epilogue & Narrative Resolution

* **Victory Condition (`CasesWIN.png`):** Vince hands over the heirloom necklace from the inside lining of his tailored coat. Kirby Raymundo looks on with bitter disappointment, instructing the police to take his nephew into custody. Detective receives commendation for breaking the alibi within the 5-minute window.
* **Failure Condition (`case1FAILED.png`):** Time expires or repeated false accusations allow Vince to feign outrage, storm out into the stormy night with the necklace, and catch a late midnight train to the capital, leaving the case permanently unresolved.

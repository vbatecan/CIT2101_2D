# Level 2: The Shattered Mirror (Story & Screenplay Bible)

**Case ID:** `LEVEL_02`  
**Internal Reference:** `Case002`  
**Incident:** Staged Burglary & Art Insurance Fraud  
**Setting:** Upscale Contemporary Art Gallery (Camacho Fine Arts) — Private Office & Rear Alley  
**Atmosphere:** Late-night city quiet; cool neon ambient light filtered through venetian blinds; the hum of an air conditioner in an otherwise silent gallery.  
**Target Playtime:** 3 to 5 Minutes  
**Primary Suspect / Witness:** Charl Vonn Pascual (Night Security Guard)  
**Secondary Suspect:** Paul Gabriel Camacho (Gallery Owner & Fraud Architect)  
**Lead Investigator:** Detective Kyle Gabriel Pastrana  

---

## 1. Narrative Synopsis

At 11:05 PM, emergency services receive a panicked call from Paul Gabriel Camacho, proprietor of the prestigious Camacho Fine Arts Gallery in the metropolitan cultural district. 

Paul reports that a brazen cat burglar shattered the reinforced back-office window from the secluded rear alley, breached the gallery's private showroom, and made off with *"The Crimson Reverie"*—a celebrated modernist painting appraised at half a million credits.

When Detective Kyle Gabriel Pastrana arrives on the scene, night security guard Charl Vonn Pascual greets him with unflappable calm. Charl provides a vivid, detailed eyewitness statement: he insists he was standing right outside the heavy office door doing his hourly rounds at exactly 11:00 PM when he heard the window shatter violently from the alley outside, but arrived moments too late to stop the intruder.

However, forensic crime scene photography and digital access telemetry expose an amateurish, meticulously staged inside job designed to defraud the insurance syndicate.

---

## 2. Dramatis Personae

### Charl Vonn Pascual (Primary Suspect / Witness)
* **Age:** 30
* **Occupation:** Head Night Security Officer
* **Relationship to Victim:** Salaried Employee of Paul Gabriel Camacho
* **Visual Asset:** `Vonn.png`
* **Personality:** Stoic, procedural, unshakeable. Relies on corporate security terminology to project professionalism.
* **Motive:** Received a bribe of 2,000 upfront credits (with a promise of 10,000 more once the claim cleared) to falsify his patrol report and supply the hammer used to break the glass.
* **Alibi:** *"I was standing right outside the private office door on my eleven o'clock patrol. I heard the glass explode from the alley side and drew my flashlight immediately."*
* **Psychological Tell:** Adjusts his guard belt buckle and checks his wristwatch whenever timestamps are scrutinized.

### Paul Gabriel Camacho (Organizer & Fraud Mastermind)
* **Age:** 42
* **Occupation:** Art Gallery Owner & Connoisseur
* **Relationship to Victim:** Victim / Mastermind
* **Visual Asset:** `Paul.png`
* **Personality:** Secretive, dramatic, theatrical. Puts on an exaggerated performance of grief over his "lost masterpiece."
* **Motive:** The gallery is drowning under 300,000 credits of bad commercial debt. Paul could not sell *"The Crimson Reverie"* at market price, so he schemed to liquidate it via insurance payout.
* **Alibi:** Claims he was fast asleep in his penthouse apartment across town when the automated burglar alarm triggered.

### Detective Kyle Gabriel Pastrana (Lead Field Investigator)
* **Age:** 34
* **Occupation:** Senior Detective, Major Crimes Unit
* **Personality:** Methodical, sharp-eyed, unimpressed by theatrical theatrics. Specializes in physical crime scene reconstruction and digital telemetry.

---

## 3. Evidence & Forensic Dossier

| Evidence ID | Name | Category | Initial State | Unlock Condition | Forensic Significance |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `EVD_WINDOW_PHOTO` | **Window Frame Crime Scene Photo** | Photograph | **Discovered** | Case Start | High-definition crime scene photo taken from the alley cobblestone. Shows glass shards concentrated *outside* on the alley floor rather than inside the room, accompanied by backward impact conchoidal fractures. Proves the window was struck from the *inside*. |
| `EVD_SHIFT_LOG` | **Security Guard Shift Log** | Document | **Locked** | `NODE_02_WINDOW_LEAD` | Exported telemetry from the building's digital access control system. Shows badge `#GUARD-04` (Charl Vonn Pascual) was scanned at the East Perimeter Gate at 11:00:22 PM—over 250 meters away from the office door. |
| `EVD_INSURANCE_POLICY` | **Art Insurance Policy** | Document | **Locked** | `NODE_03_SHIFT_LEAD` | Underwriting contract for *"The Crimson Reverie"*. An emergency rider was executed exactly 48 hours prior to the incident, doubling the indemnity payout from $250,000 to $500,000. |

---

## 4. Complete Screenplay & Interrogation Dialogue

### Scene Setup
* **Camera:** Fixed investigation POV looking across the sleek glass desk.
* **Characters:** Charl Vonn Pascual sits on the left (`Character_Suspect_Left`). Paul Gabriel Camacho sits on the right (`Character_Suspect_Right`).
* **Table Items:** Window Frame Crime Scene Photo lies near the evidence tray.
* **Audio:** Quiet hum of fluorescent lights, distant siren wailing in the city streets.

---

### [BEAT 1: The Guard's Confident Alibi]

**NODE_01**  
*Speaker:* Charl Vonn Pascual  
*Expression:* **Calm**  
*Audio:* Controlled synth ambiance  
> **CHARL:**  
> "I was standing right outside the office door when I heard the window shatter from the alley at 11:00 PM. I drew my flashlight and rushed in, but whoever it was had already grabbed the canvas and vanished into the darkness."

**NODE_01B_INTERVIEW (Detective Pastrana Beat)**  
*Speaker:* Detective Kyle Pastrana  
*Expression:* Neutral  
> **DETECTIVE PASTRANA:**  
> "You heard the break at exactly 11:00 PM. Describe where you were standing, Charl, and tell me what you saw on each side of the window."

---

### [BEAT 2: The Physical Window Challenge]

**NODE_02_WINDOW_LEAD**  
*Speaker:* Charl Vonn Pascual  
*Expression:* **Calm**  
> **CHARL:**  
> "The alley window was broken from the outside. The burglar threw a heavy brick or mallet through the pane. Check the frame if you doubt my word."

*(System Action: `NODE_02_WINDOW_LEAD` completed. Unlocks `EVD_SHIFT_LOG` onto the table.)*

---

### [BEAT 3: Inspection of Access Telemetry]

*(Player Action: Inspects the Security Guard Shift Log on the table. The dialogue bubble smoothly re-anchors to the digital badge printout.)*

**NODE_03_SHIFT_LEAD**  
*Speaker:* Charl Vonn Pascual  
*Expression:* **Nervous**  
*Animation:* Eyes dart momentarily toward Paul Camacho.  
> **CHARL:**  
> "The shift log is routine. It will show I was patrolling near the central corridor and office, exactly as I testified."

*(System Action: `NODE_03_SHIFT_LEAD` completed. Unlocks `EVD_INSURANCE_POLICY` onto the table.)*

**NODE_03B_CONFIRMATION (Detective Pastrana Beat)**  
*Speaker:* Detective Kyle Pastrana  
*Expression:* Neutral  
> **DETECTIVE PASTRANA:**  
> "The glass distribution raises an obvious question. If the window was smashed from inside the office, why does your sworn statement place you right outside the corridor door? And more importantly... where were your boots actually planted at 11:00 PM?"

---

### [BEAT 4: The Fatal Boast & Contradiction Point]

**NODE_04_FINAL_STATEMENT** *(Challengeable Statement)*  
*Speaker:* Charl Vonn Pascual  
*Expression:* **Calm (Defiant)**  
> **CHARL:**  
> "I was outside that office at 11:00 PM. The keycard record cannot say otherwise."

*(Gameplay: Player clicks [Challenge] and presents `EVD_SHIFT_LOG` (Security Guard Shift Log).)*

---

### [BEAT 5: The Contradiction Sting & Confession]

*Audio:* **Dramatic Contradiction Sting** (`SFX_Objection_Chord`)  
*Visual:* Camera shake, sharp spotlight zoom onto Charl.  
*Rule Triggered:* `RULE_CHARL_LOCATION_LIE`  

> **DETECTIVE PASTRANA:**  
> "You say the keycard record cannot say otherwise? Look at line forty-two, Officer Pascual. At eleven o'clock and twenty-two seconds, your digital badge was swiped at the East Perimeter Gate—two hundred and fifty meters away! It would take a sprint champion two minutes to reach the office from there!"

> **CHARL:**  
> "The keycard shift log? Ah... I... I forgot the electronic turnstiles record exact second timestamps..."

**NODE_05_CONFESSION**  
*Speaker:* Charl Vonn Pascual  
*Expression:* **Nervous Breakdown**  
*Audio:* Somber realization theme  
> **CHARL:**  
> "Fine! Damn it, the shift log doesn't lie! I was at the East Gate! Mr. Paul Camacho paid me two thousand credits in cash to stage the break-in! He took the painting himself at ten-thirty, drove it to his storage lockup, and ordered me to smash the window from the inside with a crowbar when the coast was clear! He swore the insurance company would pay half a million without asking questions! I only took the money because my mother's medical bills are overdue!"

*(Paul Camacho jumps out of his chair in pure panic: "You fool! You swore an oath of silence!")*

---

## 5. Deduction Board Synergy

* **Clue A:** `CLUE_BROKEN_FROM_INSIDE` (*"Glass shards scattered outside on alley pavement prove window was broken from the INSIDE."*)
* **Clue B:** `EVD_INSURANCE_POLICY_BASE_CLUE` (*"Paul doubled the insurance payout value of the painting to $500,000 just 48 hours prior to the theft."*)
* **Deduction Result:** `CLUE_INSURANCE_FRAUD_PROOF`
* **Deduction Synthesis Text:** *"The window broken from the inside combined with Paul doubling the insurance policy 48 hours earlier confirms a premeditated, staged burglary for financial fraud!"*

---

## 6. Case Conclusion Quiz (Accusation Phase)

| Question # | Question Prompt | Options | Correct Answer | Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Q1 (Architect)** | Who organized the staged burglary at the art gallery? | A) Paul Gabriel Camacho<br/>B) Charl Vonn Pascual<br/>C) An Unknown Thief | **A) Paul Gabriel Camacho** | Paul devised the scheme to rescue his failing business with insurance money. |
| **Q2 (Accomplice)** | Who was Paul's paid accomplice? | A) Kirby Raymundo<br/>B) Charl Vonn Pascual<br/>C) Vince Angelo Batecan | **B) Charl Vonn Pascual** | Charl accepted 2,000 credits to smash the window and provide false testimony. |
| **Q3 (Motive)** | What was Paul's motive for staging the burglary? | A) To obtain a $500,000 insurance payout<br/>B) Revenge against Charl<br/>C) To steal another painting | **A) To obtain a $500,000 insurance payout** | His gallery was insolvent; the insurance indemnity was his sole bailout. |
| **Q4 (Forensics)** | What did the window crime scene reveal? | A) The window was broken from the inside.<br/>B) The window was broken from the outside.<br/>C) The window was already damaged before the incident. | **A) The window was broken from the inside.** | Glass fragments lay on the exterior alley cobblestone, not the interior office rug. |
| **Q5 (Policy)** | What suspicious change was made to the painting's insurance policy? | A) The insurance was cancelled.<br/>B) Coverage was reduced to $100,000.<br/>C) Coverage increased from $250,000 to $500,000. | **C) Coverage increased from $250,000 to $500,000.** | An emergency rider executed 48 hours before the break-in doubled the payout. |

---

## 7. Epilogue & Narrative Resolution

* **Victory Condition (`CasesWIN.png`):** Detective Pastrana secures signed confessions from both Charl and Paul. Police recover *"The Crimson Reverie"* intact from Paul's private climate-controlled garage. The insurance syndicate drops charges against the gallery staff while arresting Camacho for felony grand fraud.
* **Failure Condition (`Case2FAILED.png`):** The player fails to connect the telemetry timestamp before the timer elapses. Charl sticks to his scripted story, Paul's legal counsel files an immediate insurance payout demand, and the pair slip through regulatory scrutiny.

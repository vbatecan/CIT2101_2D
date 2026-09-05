# Case Closed — Master Character & Cast Bible

**Project:** `CIT2101_2D` (*Case Closed*)  
**Document:** Character Profiles, Psychological Dossiers & Asset Mappings  

---

## 1. Overview & Dual-Suspect Interrogation Architecture

The game's visual presentation centers on a two-suspect composition across the interrogation table:
* **Left Character (`Character_Suspect_Left`):** The primary suspect undergoing intense cross-examination.
* **Right Character (`Character_Suspect_Right`):** An accomplice, material witness, or rival party who reacts to shifts in testimony.

```
┌────────────────────────────────────────────────────────────────────────┐
│                        INTERROGATION CHAMBER POV                       │
│                                                                        │
│       ┌──────────────────────┐          ┌──────────────────────┐       │
│       │                      │          │                      │       │
│       │   PRIMARY SUSPECT    │          │  SECONDARY SUSPECT / │       │
│       │       (LEFT)         │          │     WITNESS (RIGHT)  │       │
│       │                      │          │                      │       │
│       └──────────┬───────────┘          └──────────┬───────────┘       │
│                  │                                 │                   │
│   ═══════════════╧═════════════════════════════════╧════════════════  │
│                     INVESTIGATION & EVIDENCE TABLE                     │
│         [ Photo / Log ]         [ Clue Item ]         [ Arm Pointer ]  │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Playable & Registered Investigators

### Detective Kyle Gabriel Pastrana
* **Internal ID:** `CHAR_KYLE_PASTRANA`
* **Title:** Senior Field Detective, Major Crimes Division
* **Age:** 34
* **Personality Trait:** `PersonalityTrait.Observant`
* **Specialty:** Physical scene reconstruction, trajectory analysis, behavioral profiling.
* **Background:** A seasoned homicide and grand larceny investigator who built his career by refusing to accept tidy circumstantial stories. He possesses an uncanny ability to spot physical anomalies that amateurs overlook (such as the directional displacement of shattered glass).
* **Investigative Philosophy:** *"People practice their lies in front of mirrors. Physics never practices, and physics never lies."*

### Detective Miguel Borja
* **Internal ID:** `CHAR_MIGUEL_BORJA`
* **Title:** Chief Inspector, Cyber & Intellectual Property Task Force
* **Age:** 36
* **Personality Trait:** `PersonalityTrait.Methodical`
* **Specialty:** Digital forensics, network telemetry, encrypted communication metadata.
* **Background:** Former network security engineer turned police inspector. Specializes in white-collar theft, trade secret leaks, and corporate espionage. Known for dismantling high-tech alibis using server timestamps and camera metadata.
* **Investigative Philosophy:** *"Every keystroke leaves an echo. Every packet has a sender. You cannot walk through a digital world without casting a shadow."*

---

## 3. Cast of Case 01: The Missing Necklace

### Vince Angelo Batecan (Primary Suspect)
* **Internal ID:** `CHAR_VINCE_BATECAN`
* **Role:** Nephew of Kirby Raymundo
* **Age:** 25
* **Personality Trait:** `PersonalityTrait.Defensive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Vince.png`
* **Scene Slot:** `CharacterSlot.PrimarySuspect` (Left)
* **Expressions Supported:** `Neutral`, `Defensive`, `Nervous`, `Shocked`
* **Psychological Profile:** Vince is arrogant on the surface but fundamentally panic-stricken underneath. Disowned by his wealthy family for chronic gambling, he owes enormous sums to ruthless loan sharks. When interrogated, he bristles at perceived insults, raises his voice, and tries to divert suspicion onto household staff.
* **Key Contradiction:** Claims he was in the kitchen continuously from 8:30 PM to 9:15 PM, directly disproved by the head chef's locked pantry log.

### Jane Arie Reyes (Secondary Witness)
* **Internal ID:** `CHAR_CASE1_FEMALE`
* **Role:** Manor Dinner Guest & Key Witness
* **Age:** 24
* **Personality Trait:** `PersonalityTrait.Observant`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Jane.png`
* **Scene Slot:** `CharacterSlot.SecondarySuspect` (Right)
* **Expressions Supported:** `Neutral`, `Observant`, `Surprised`
* **Psychological Profile:** Quiet, analytical, and highly observant. As an outsider to the Raymundo family drama, she watched the interpersonal friction with detached curiosity. She provides unbiased testimony confirming Vince's violent argument with Kirby in the foyer.

### Kirby Raymundo (Victim)
* **Internal ID:** `CHAR_KIRBY_RAYMUNDO`
* **Role:** Patriarch & Wealthy Aristocrat
* **Age:** 58
* **Personality Trait:** `PersonalityTrait.Secretive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Kirby.png`
* **Psychological Profile:** Demanding, proud, and authoritarian. Refused to bail out his nephew Vince, precipitating the desperate break-in.

---

## 4. Cast of Case 02: The Shattered Mirror

### Charl Vonn Pascual (Primary Suspect / Witness)
* **Internal ID:** `CHAR_CHARL_PASCUAL`
* **Role:** Head Night Security Guard, Camacho Fine Arts
* **Age:** 30
* **Personality Trait:** `PersonalityTrait.Calm`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Vonn.png`
* **Scene Slot:** `CharacterSlot.PrimarySuspect` (Left)
* **Expressions Supported:** `Calm`, `Defensive`, `Nervous`, `Shocked`
* **Psychological Profile:** Charl prides himself on maintaining complete military bearing under pressure. He speaks in crisp, measured sentences. Beneath his calm exterior, he is terrified of being convicted for fraud, having only accepted Paul Camacho's bribe to cover his ailing mother's medical treatments. When his electronic shift badge exposes his real location, his rigid composure instantly shatters.
* **Key Contradiction:** Insists he was outside the office door at 11:00 PM; badge scan proves he was at the East Perimeter Gate 250 meters away.

### Paul Gabriel Camacho (Accomplice / Fraud Architect)
* **Internal ID:** `CHAR_PAUL_CAMACHO`
* **Role:** Art Gallery Owner & Connoisseur
* **Age:** 42
* **Personality Trait:** `PersonalityTrait.Secretive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Paul.png`
* **Scene Slot:** `CharacterSlot.SecondarySuspect` (Right)
* **Expressions Supported:** `Neutral`, `Dramatic`, `Angry`, `Defeated`
* **Psychological Profile:** Flamboyant, narcissistic, and deeply in debt. Paul viewed the staged burglary as a victimless financial transaction with his insurance syndicate. He aggressively coaches Charl with subtle glances during the interrogation, only to turn venomous when Charl cracks.

---

## 5. Cast of Case 03: The Last Call

### Shanaia Ortega (Primary Suspect)
* **Internal ID:** `CHAR_SHANAIA_ORTEGA`
* **Role:** Lead Software Architect & Co-Founder, AetherCore Systems
* **Age:** 27
* **Personality Trait:** `PersonalityTrait.Calm` / `PersonalityTrait.Secretive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Shania.png`
* **Scene Slot:** `CharacterSlot.PrimarySuspect` (Left)
* **Expressions Supported:** `Calm`, `Nervous`, `Angry`, `Shocked`
* **Psychological Profile:** Formidably intelligent, fiercely territorial over her intellectual property, and emotionally cold. She feels that Kurt Ancheta was merely the "suit" taking credit for her engineering genius. Discovering his draft termination letter triggered a calculated, retaliatory burglary. Her calm demeanor masks volcanic resentment.
* **Key Contradiction:** Swears she went straight home at 5:30 PM and never returned; CCTV footage captures her distinctive jacket entering the rear alley door at 7:10 PM.

### Shan Jaraba (Secondary Witness / Key Informant)
* **Internal ID:** `CHAR_SHAN_JARABA`
* **Role:** Cafe Manager, Bean & Binary
* **Age:** 29
* **Personality Trait:** `PersonalityTrait.Secretive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Shan.png`
* **Scene Slot:** `CharacterSlot.SecondarySuspect` (Right)
* **Expressions Supported:** `Neutral`, `Observant`, `Smug`
* **Psychological Profile:** Streetwise, cynical, and fiercely protective of his cafe. He dislikes tech-bro arrogance and kept a close eye on Kurt and Shanaia's meetings. His private back-alley surveillance system provides the crucial smoking gun.

### Kurt Miguel Ancheta (Victim)
* **Internal ID:** `CHAR_KURT_ANCHETA`
* **Role:** Founder & CEO, AetherCore Systems
* **Age:** 31
* **Personality Trait:** `PersonalityTrait.Defensive`
* **Visual Asset:** `Assets/Assets/CHARACTERS/Kurt.png`
* **Psychological Profile:** High-strung, exhausted, and desperate. Had grown increasingly paranoid of Shanaia's clandestine communications with competitors, pushing him toward drafting her termination before the final prototype was secured.

---

## 6. Technical Implementation & ScriptableObject Mapping

### Runtime Character Component (`CharacterDisplay.cs`)
```csharp
// Configured on Character_Suspect_Left & Character_Suspect_Right GameObjects
[SerializeField] private CharacterSlot _slot; // PrimarySuspect or SecondarySuspect
[SerializeField] private SpriteRenderer _spriteRenderer;
[SerializeField] private bool _enableIdleBreathing = true;
[SerializeField] private float _breathingSpeed = 2.0f;
[SerializeField] private float _breathingAmount = 0.03f;
```

### Character Profile Data Model (`CharacterProfileSO.cs`)
```csharp
[CreateAssetMenu(fileName = "NewCharacterProfile", menuName = "Case Closed/Character Profile")]
public class CharacterProfileSO : ScriptableObject
{
    public string characterId;
    public string fullName;
    public int age;
    public string occupation;
    public string relationshipToVictim;
    public PersonalityTrait personalityTrait;
    
    [TextArea(2, 5)] public string background;
    [TextArea(2, 5)] public string alibi;
    [TextArea(2, 5)] public string possibleMotives;
    [TextArea(2, 5)] public string knownConflicts;
    
    public Sprite defaultSittingPose;
    public List<CharacterExpressionEntry> expressions;
}
```

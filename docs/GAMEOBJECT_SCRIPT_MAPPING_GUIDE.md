# GameObject to Script Drag & Drop Mapping Guide

**Project:** `CIT2101_2D` / *Case Closed*  
**Reference Sketch:** Interrogation Room Desk, 2 Interrogated Suspects, Table Evidence Items, Open Case Book, and Bottom Dialogue UI.

---

## 1. Visual Overview & Scene Layout

Based on your design sketch, the game screen is divided into 4 primary layers:

```
+-------------------------------------------------------------------------+
| [Layer 1: Background] Wall, Window / Two-Way Mirror with "!" Alert     |
|                                                                         |
|        [Character_Suspect_Left]            [Character_Suspect_Right]    |
|        (Primary Suspect)                   (Secondary Suspect/Witness)  |
|                                                                         |
+-------------------------------------------------------------------------+
| [Layer 3: Investigation Table / Desk]                                   |
|   [Item_Photo]       [Item_CaseBook]       [Item_Weapon]   [Item_Cup]   |
|   (Envelope/Photo)   (Open Notebook)       (Knife Clue)    (Drink Clue) |
|                            [Detective_Hand_Cursor]                      |
+-------------------------------------------------------------------------+
| [Layer 4: Bottom UI] Dialogue Box, Speaker Nameplate, Challenge Button |
+-------------------------------------------------------------------------+
```

---

## 2. Complete Hierarchy & Component Mapping Table

| Hierarchy GameObject | Purpose in Sketch | Required Unity Components | Script to Drag & Drop | Inspector Setup |
| :--- | :--- | :--- | :--- | :--- |
| **`Main Camera`** | Viewport & 2D camera lock | `Camera`, `AudioListener` | [`FixedInvestigationCamera.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/FixedInvestigationCamera.cs) | Set `Orthographic Size = 5`, `Fixed Position = (0, 0, -10)`. |
| **`Environment_Background`** | Interrogation wall & two-way mirror | `SpriteRenderer` | *(None — Static Visual)* | Assign background sprite (e.g. `Background.jpeg`). Sorting Layer: `Background`, Order: `0`. |
| **`Character_Suspect_Left`** | Left person getting interrogated | `SpriteRenderer` | [`CharacterDisplay.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs) | Set `Character Slot = PrimarySuspect`. Assign `Character Sprite Renderer`. Enable `Idle Breathing`. |
| **`Character_Suspect_Right`** | Right person getting interrogated | `SpriteRenderer` | [`CharacterDisplay.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs) | Set `Character Slot = SecondarySuspect`. Assign `Character Sprite Renderer`. Enable `Idle Breathing`. |
| **`Table_Desk`** | Wooden interrogation desk surface | `SpriteRenderer` | *(None — Static Visual)* | Sorting Layer: `Foreground`, Order: `10`. |
| **`Item_CaseBook`** | Open book on table (opens dossier) | `SpriteRenderer`, `BoxCollider2D` | [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) | **Check `Open Notebook On Click = true`**. Assign open book sprite. |
| **`Item_Photo_Evidence`** | Envelope/Photo on left side of table | `SpriteRenderer`, `BoxCollider2D` | [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) | Assign `Evidence Data = EVD_FAMILY_PHOTO` (or `EVD_WINDOW_PHOTO`). |
| **`Item_Weapon_Clue`** | Knife / weapon in center of table | `SpriteRenderer`, `BoxCollider2D` | [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) | Assign `Evidence Data = EVD_BROKEN_TEACUP` (or custom weapon SO). |
| **`Item_Cup_Clue`** | Cup / beverage on right side of desk | `SpriteRenderer`, `BoxCollider2D` | [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) | Assign `Evidence Data` ScriptableObject. |
| **`_Managers`** *(Root)* | Central game engine controllers | `Transform` | *(Container for core managers)* | Group all manager GameObjects underneath. |
| ├── **`CaseManager`** | Case progression & state tracker | *(None)* | [`CaseManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/CaseManager.cs) | Holds runtime active case, discovered evidence, and clues. |
| ├── **`InterrogationManager`** | Dialogue & contradiction controller | *(None)* | [`InterrogationManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/InterrogationManager.cs) | Handles dialogue navigation and evidence challenges. |
| ├── **`EvidenceManager`** | Evidence & modal coordinator | *(None)* | [`EvidenceManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/EvidenceManager.cs) | Coordinates table selection and inspect modal zoom. |
| ├── **`DeductionBoardController`** | Deduction board clue linking | *(None)* | [`DeductionBoardController.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/DeductionBoardController.cs) | Manages clue pairing and deduction unlocks. |
| ├── **`CaseConclusionManager`** | Final scoring & quiz grader | *(None)* | [`CaseConclusionManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/CaseConclusionManager.cs) | Evaluates quiz answers, star ratings, and letter grades. |
| ├── **`AudioManager`** | Audio BGM & SFX playback | `AudioSource` $\times 3$ | [`AudioManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Managers/AudioManager.cs) | Assign BGM tracks and SFX clips in Inspector. |
| └── **`GameBootstrap`** | Level loading & scene initialization | *(None)* | [`GameBootstrap.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Prototype/GameBootstrap.cs) | Enables prototype level switching with number keys `1`, `2`, `3`. |
| **`Canvas_MainUI`** | Screen-space Canvas root | `Canvas`, `CanvasScaler`, `GraphicRaycaster` | [`UIManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/UIManager.cs) | Link all panel GameObjects in the Inspector slots. |
| ├── **`Panel_Dialogue`** | Bottom dialogue box for suspect speech | `Image`, `CanvasGroup` | [`DialogueUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/DialogueUI.cs) | Hook `SpeakerNameText`, `DialogueBodyText`, `NextButton`, `ChallengeButton`. |
| ├── **`Panel_CaseFileNotebook`**| Fullscreen dossier & suspect notebook | `Image` | [`CaseFileNotebookUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/CaseFileNotebookUI.cs) | Hook `SummaryTabButton`, `SuspectsTabButton`, `EvidenceTabButton`, `CluesTabButton`. |
| ├── **`Panel_InspectModal`** | Zoomed evidence inspection popup | `Image` | [`EvidenceInspectModal.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/EvidenceInspectModal.cs) | Hook `EvidenceTitleText`, `EvidenceZoomImage`, `RotateButtons`, `CloseButton`. |
| └── **`Panel_ConclusionQuiz`** | Final conclusion Q&A screen | `Image` | [`ConclusionUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/ConclusionUI.cs) | Hook `SubmitButton`, `ContinueButton`, `ResultsContainer`. |

---

## 3. Step-by-Step Scene Setup in Unity

### Step 1: Setting up the Camera & Background
1. Select the **`Main Camera`** in the Hierarchy.
2. Drag and drop [`FixedInvestigationCamera.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/FixedInvestigationCamera.cs) onto it.
3. Create an empty GameObject named **`Environment_Background`**, add a `SpriteRenderer`, and assign your room/mirror background image.

---

### Step 2: Adding the Two Interrogated Characters
1. Create a 2D Sprite GameObject named **`Character_Suspect_Left`**:
   - Position: `X: -2.5, Y: 1.0, Z: 0`
   - Drag and drop [`CharacterDisplay.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs) onto it.
   - In Inspector: Set **`Character Slot = PrimarySuspect`**.
   - Assign its own `SpriteRenderer` to the `Character Sprite Renderer` field.
2. Create a 2D Sprite GameObject named **`Character_Suspect_Right`**:
   - Position: `X: 2.5, Y: 1.0, Z: 0`
   - Drag and drop [`CharacterDisplay.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs) onto it.
   - In Inspector: Set **`Character Slot = SecondarySuspect`**.
   - Assign its own `SpriteRenderer` to the `Character Sprite Renderer` field.

> **Result:** When a case loads, the primary suspect (e.g. Vince Batecan) automatically appears on the left, while the accomplice/witness (e.g. Paul Camacho or Charl Pascual) appears on the right!

---

### Step 3: Setting up the Investigation Desk & Table Items
1. Create a GameObject named **`Table_Desk`** with a `SpriteRenderer` for the desk surface.
2. **Open Case Book Item (`Item_CaseBook`)**:
   - Add `SpriteRenderer` and `BoxCollider2D`.
   - Drag and drop [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) onto it.
   - In Inspector: **Check `Open Notebook On Click = true`**.
   - *Behavior:* When clicked by the player, it instantly opens the detective's case dossier notebook!
3. **Physical Evidence Items (`Item_Photo_Evidence`, `Item_Weapon_Clue`, `Item_Cup_Clue`)**:
   - Add `SpriteRenderer` and `BoxCollider2D` to each.
   - Drag and drop [`TableEvidenceItem.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs) onto each item.
   - Assign the corresponding `EvidenceSO` asset (e.g. `EVD_FAMILY_PHOTO`, `EVD_BROKEN_TEACUP`, etc.) to the `Evidence Data` field.
   - *(Optional)* Set `Dialogue Node To Trigger On Inspect` to have the suspect explain the item when clicked!

---

### Step 4: Setting up UI & Canvas
1. Create a **Canvas** named `Canvas_MainUI` (Render Mode: `Screen Space - Overlay`).
2. Drag and drop [`UIManager.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/UIManager.cs) onto `Canvas_MainUI`.
3. Under Canvas, create:
   - **`Panel_Dialogue`** at bottom $\rightarrow$ drag [`DialogueUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/DialogueUI.cs).
   - **`Panel_CaseFileNotebook`** $\rightarrow$ drag [`CaseFileNotebookUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/CaseFileNotebookUI.cs).
   - **`Panel_InspectModal`** $\rightarrow$ drag [`EvidenceInspectModal.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/EvidenceInspectModal.cs).
   - **`Panel_ConclusionQuiz`** $\rightarrow$ drag [`ConclusionUI.cs`](file:///C:/Users/Andrei/Projects/CIT2101_2D/Assets/Scripts/UI/ConclusionUI.cs).
4. Assign these panels into the respective fields in `UIManager`.

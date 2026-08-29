# Walkthrough: Investigator Selection & Level 1/2/3 System

We have implemented the 2-character Investigator selection system and explicitly structured the 3 cases as **Level 1**, **Level 2**, and **Level 3**.

---

## Key Features Implemented

### 1. Two Selectable Investigator Characters
The player can choose who will lead the investigation across all cases:
- **Detective Kyle Gabriel Pastrana**: Lead Field Detective (Specializes in scene reconstruction, physical evidence analysis, and observing contradictions).
- **Detective Miguel Borja**: Lead Digital Forensics Detective (Specializes in encrypted logs, cyber forensics, and data trail deductions).

### 2. Cases Formatted as Levels 1, 2, and 3
- **Level 1**: *The Missing Necklace* (`LEVEL_01`)
  - Target: Vince Angelo Batecan
  - Crime: Locked study safe break-in & stolen heirloom necklace.
- **Level 2**: *The Shattered Mirror* (`LEVEL_02`)
  - Target: Charl Vonn Pascual & Paul Gabriel Camacho
  - Crime: Staged exterior window break-in & gallery insurance fraud.
- **Level 3**: *The Last Call* (`LEVEL_03`)
  - Target: Shanaia Ortega & Shan Jaraba
  - Crime: Missing prototype source code drive & encrypted phone calls.

### 3. Investigator & Level Selection Controls
- **Level Switching Keys**: Press `1`, `2`, or `3` to instantly jump to Level 1, Level 2, or Level 3.
- **Investigator Toggle Keys**:
  - Press `C` to instantly cycle the active lead detective (Kyle Pastrana ↔ Miguel Borja).
  - Press `I` to open the full [InvestigatorSelectionUI](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/UI/InvestigatorSelectionUI.cs) panel.
- **Results Screen Progression**:
  - Displays the active Lead Investigator on the solved scorecard.
  - Features a **"Proceed to Level X >"** button to advance smoothly from Level 1 → Level 2 → Level 3.

---

## Modified Files Summary

| File | Changes |
| :--- | :--- |
| [CaseSO.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Data/CaseSO.cs) | Added `levelNumber` and `leadInvestigator` properties. |
| [CaseManager.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Managers/CaseManager.cs) | Added active `selectedInvestigator`, `availableInvestigators`, and `OnInvestigatorChanged` event. |
| [GameBootstrap.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Prototype/GameBootstrap.cs) | Instantiates Kyle Pastrana and Miguel Borja, binds keyboard shortcuts (`1`, `2`, `3`, `I`, `C`). |
| [Case01Initializer.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Prototype/Case01Initializer.cs) | Configured as Level 1: The Missing Necklace (`LEVEL_01`). |
| [Case02Initializer.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Prototype/Case02Initializer.cs) | Configured as Level 2: The Shattered Mirror (`LEVEL_02`), registered Kyle as investigator. |
| [Case03Initializer.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Prototype/Case03Initializer.cs) | Configured as Level 3: The Last Call (`LEVEL_03`), registered Miguel as investigator. |
| [InvestigatorSelectionUI.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/UI/InvestigatorSelectionUI.cs) | New UI panel for switching investigators and selecting levels directly. |
| [NotebookFormattingService.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Services/NotebookFormattingService.cs) | Displays Level number and Lead Investigator profile in Case Summary tab. |
| [ConclusionUI.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/UI/ConclusionUI.cs) | Added Lead Investigator name to scorecard and Next Level advancement button. |
| [UIManager.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/UI/UIManager.cs) | Added `InvestigatorSelect` panel state management and toggle methods. |
| [CharacterDisplay.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs) | Added `CharacterSlot.Investigator` support. |
| [CharacterSlot.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Enums/CharacterSlot.cs) | Added `Investigator` enum value. |
| [UIPanelType.cs](file:///c:/Users/Janine/Documents/GitHub/CIT2101_2D/Assets/Scripts/Enums/UIPanelType.cs) | Added `InvestigatorSelect` panel type. |

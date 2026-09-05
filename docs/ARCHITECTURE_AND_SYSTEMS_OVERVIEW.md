# Architecture & Systems Overview

**Project:** `CIT2101_2D` / *Case Closed*  
**Document:** `docs/ARCHITECTURE_AND_SYSTEMS_OVERVIEW.md`

---

## 1. Architectural Philosophy

The codebase is designed around two complementary principles:
1. **Separation of Concerns (SoC) & YAGNI:**
   - **Data Models / ScriptableObjects:** Hold data definitions, evidence properties, and dialogue trees.
   - **Services (Pure C#):** Encapsulate business logic, calculations, rule validations, and string formatting without MonoBehaviour overhead.
   - **Controllers / Managers (MonoBehaviours):** Handle Unity lifecycle events, audio triggers, state tracking, and event broadcasting.
   - **UI / Gameplay Views (MonoBehaviours):** Focus purely on rendering, animations, user clicks, and inspector bindings.
2. **Unity Drag-and-Drop Workflow:**
   - All components attached to GameObjects in the scene are standard `MonoBehaviour` classes that can be dragged directly onto GameObjects in the Unity Inspector.
   - Controllers instantiate and utilize their pure C# services internally, eliminating the need for complex dependency injection containers or cluttered Inspector fields.

---

## 2. Complete File Directory & Layer Reference

```
Assets/Scripts/
├── Enums/
│   ├── CharacterExpression.cs
│   ├── CharacterSlot.cs
│   ├── EvidenceCategory.cs
│   ├── NotebookTab.cs
│   ├── PersonalityTrait.cs
│   └── UIPanelType.cs
├── Data/
│   ├── CaseEvaluationResult.cs
│   ├── CaseSO.cs
│   ├── CharacterProfileSO.cs
│   ├── ClueConnectionSO.cs
│   ├── ContradictionRuleSO.cs
│   ├── DialogueTreeSO.cs
│   └── EvidenceSO.cs
├── Services/
│   ├── CaseEvaluationService.cs
│   ├── DeductionService.cs
│   ├── EvidenceService.cs
│   ├── InterrogationService.cs
│   └── NotebookFormattingService.cs
├── Managers/
│   ├── AudioManager.cs
│   ├── CaseConclusionManager.cs
│   ├── CaseManager.cs
│   ├── DeductionBoardController.cs
│   ├── EvidenceManager.cs
│   └── InterrogationManager.cs
├── Gameplay/
│   ├── CharacterDisplay.cs
│   ├── FixedInvestigationCamera.cs
│   ├── InteractiveHotspot.cs
│   └── TableEvidenceItem.cs
├── UI/
│   ├── CaseFileNotebookUI.cs
│   ├── ConclusionUI.cs
│   ├── DialogueUI.cs
│   ├── EvidenceInspectModal.cs
│   ├── MainMenuUI.cs
│   └── UIManager.cs
├── Prototype/
│   ├── Case01Initializer.cs
│   ├── Case02Initializer.cs
│   ├── Case03Initializer.cs
│   └── GameBootstrap.cs
└── Editor/
    └── MainMenuSceneBuilder.cs
```

---

## 3. Layer Descriptions & Responsibilities

### 1. Enums (`Assets/Scripts/Enums/`)
Every enum is separated into its own dedicated `.cs` file:
* [`CharacterExpression.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/CharacterExpression.cs): Facial expressions (Neutral, Defensive, Shocked, Calm, etc.).
* [`CharacterSlot.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/CharacterSlot.cs): Suspect assignment slot (PrimarySuspect, SecondarySuspect, AutoDetect).
* [`EvidenceCategory.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/EvidenceCategory.cs): Evidence classification types.
* [`NotebookTab.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/NotebookTab.cs): Navigation tabs for the detective notebook.
* [`PersonalityTrait.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/PersonalityTrait.cs): Character personality profiles.
* [`UIPanelType.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Enums/UIPanelType.cs): Screen navigation panels and modals (MainMenu, InvestigationTable, etc.).

---

### 2. Services (`Assets/Scripts/Services/`)
Pure C# classes executing core domain calculations:
* [`CaseEvaluationService.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Services/CaseEvaluationService.cs): Scoring formulas, quiz grading, star math (1-5), and rank grades (`S`, `A`, `B`, `C`, `D`).
* [`DeductionService.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Services/DeductionService.cs): Clue pairing and connection validation against deduction rules.
* [`InterrogationService.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Services/InterrogationService.cs): Contradiction matching, failure expressions, and reaction dialogue generation.
* [`EvidenceService.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Services/EvidenceService.cs): Hotspot discovery validation, clue extraction, and table presence toggles.
* [`NotebookFormattingService.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Services/NotebookFormattingService.cs): Dossier, evidence, and clue text string layout compilation.

---

### 3. Managers & Controllers (`Assets/Scripts/Managers/`)
MonoBehaviours that can be dragged directly onto GameObjects:
* [`CaseManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/CaseManager.cs): Case runtime progression and discovery state tracker.
* [`InterrogationManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/InterrogationManager.cs): Dialogue tree navigation and contradiction challenge controller.
* [`EvidenceManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/EvidenceManager.cs): Evidence selection and inspect modal coordinator.
* [`DeductionBoardController.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/DeductionBoardController.cs): Deduction board interaction and clue linking controller.
* [`CaseConclusionManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/CaseConclusionManager.cs): Conclusion scoring controller.
* [`AudioManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Managers/AudioManager.cs): Music and SFX audio playback and volume settings.

---

### 4. Gameplay & UI Layers (`Assets/Scripts/Gameplay/` & `Assets/Scripts/UI/`)
* [`CharacterDisplay.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Gameplay/CharacterDisplay.cs): Manages portraits and emotional expressions.
* [`TableEvidenceItem.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Gameplay/TableEvidenceItem.cs): Physical table items, notebook openers, and dialogue triggers.
* [`FixedInvestigationCamera.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/Gameplay/FixedInvestigationCamera.cs): Camera lock and editor viewport gizmos.
* [`MainMenuUI.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/MainMenuUI.cs): Main menu screen, case select, detective handbook, settings, and credits coordinator.
* [`DialogueUI.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/DialogueUI.cs): Typewriter dialogue rendering and evidence presenter picker.
* [`CaseFileNotebookUI.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/CaseFileNotebookUI.cs): Detective case notebook with tab navigation.
* [`EvidenceInspectModal.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/EvidenceInspectModal.cs): Zoomed 2D inspection popup with interactive hotspots.
* [`ConclusionUI.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/ConclusionUI.cs): Conclusion quiz and final scorecard display.
* [`UIManager.cs`](file:///home/vbatecan/Projects/game_dev/my2D/CIT2101_2D/Assets/Scripts/UI/UIManager.cs): Canvas panel visibility and navigation coordinator.

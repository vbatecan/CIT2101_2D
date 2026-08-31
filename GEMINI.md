# GEMINI.md - Unity Game Development & Unity MCP Master Guide

This guide establishes the mandatory coding standards, architectural patterns, and tool execution protocols for developing **CIT2101_2D** (*Case Closed*) and interfacing with the **Unity MCP Server** (`MCPForUnity`).

---

## 1. Project Overview & Technology Stack

- **Project Name:** `CIT2101_2D` (*Case Closed* - Detective Investigation & Interrogation Game)
- **Engine Version:** Unity 6 (`6000.3.20f1`)
- **Render Pipeline:** Universal Render Pipeline (`com.unity.render-pipelines.universal` v17.3.0)
- **Input System:** Unity Input System (`com.unity.inputsystem` v1.19.0)
- **Cinemachine:** Cinemachine 3 (`com.unity.cinemachine` v3.1.7)
- **UI Frameworks:** UGUI 2.0 (`com.unity.ugui`) & UI Toolkit
- **2D Suite:** 2D Animation, 2D Aseprite, 2D PSD Importer, 2D Sprite, 2D SpriteShape, 2D Tilemap
- **Unity MCP Server:** `com.coplaydev.unity-mcp` (MCPForUnity)

---

## 2. Architectural Philosophy: Separation of Concerns (SoC) & YAGNI

```
┌─────────────────────────────────────────────────────────────┐
│                    DATA LAYER (ScriptableObjects)           │
│  Assets/Scripts/Data/ (CaseSO, EvidenceSO, DialogueTreeSO)   │
└──────────────────────────────┬──────────────────────────────┘
                               │ references
┌──────────────────────────────▼──────────────────────────────┐
│                    DOMAIN LOGIC (Pure C# Services)           │
│  Assets/Scripts/Services/ (CaseEvaluation, Deduction, etc.)  │
│  • Zero MonoBehaviour dependencies  • 100% Unit Testable    │
└──────────────────────────────┬──────────────────────────────┘
                               │ instantiated by
┌──────────────────────────────▼──────────────────────────────┐
│               MANAGERS & CONTROLLERS (MonoBehaviours)       │
│  Assets/Scripts/Managers/ (CaseManager, InterrogationMgr)   │
│  • Unity Lifecycle  • Event Broadcasting  • State Tracking  │
└──────────────────────────────┬──────────────────────────────┘
                               │ updates
┌──────────────────────────────▼──────────────────────────────┐
│                    VIEW & GAMEPLAY (MonoBehaviours)         │
│  Assets/Scripts/Gameplay/ & Assets/Scripts/UI/              │
│  • CharacterDisplay, TableEvidenceItem, DialogueUI, etc.    │
└─────────────────────────────────────────────────────────────┘
```

### Directory Structure & Responsibilities

1. **`Assets/Scripts/Enums/`**: Standalone enum definitions (`CharacterExpression.cs`, `CharacterSlot.cs`, `EvidenceCategory.cs`, `NotebookTab.cs`, `PersonalityTrait.cs`, `UIPanelType.cs`).
2. **`Assets/Scripts/Data/`**: `ScriptableObject` definitions and data structures (`CaseSO.cs`, `EvidenceSO.cs`, `DialogueTreeSO.cs`, `CharacterProfileSO.cs`, `ClueConnectionSO.cs`, `ContradictionRuleSO.cs`, `CaseEvaluationResult.cs`).
3. **`Assets/Scripts/Services/`**: Pure C# domain services containing business calculations and string formatting (`CaseEvaluationService.cs`, `DeductionService.cs`, `EvidenceService.cs`, `InterrogationService.cs`, `NotebookFormattingService.cs`). **Do not inherit from `MonoBehaviour` in this folder.**
4. **`Assets/Scripts/Managers/`**: Scene controllers and state coordinators (`CaseManager.cs`, `InterrogationManager.cs`, `EvidenceManager.cs`, `DeductionBoardController.cs`, `CaseConclusionManager.cs`, `AudioManager.cs`).
5. **`Assets/Scripts/Gameplay/`**: In-world interactive actors and cameras (`CharacterDisplay.cs`, `TableEvidenceItem.cs`, `FixedInvestigationCamera.cs`, `ArmPointerController.cs`).
6. **`Assets/Scripts/UI/`**: Canvas views, modals, and coordinators (`UIManager.cs`, `DialogueUI.cs`, `CaseFileNotebookUI.cs`, `EvidenceInspectModal.cs`, `ConclusionUI.cs`, `MainMenuUI.cs`, `InvestigatorSelectionUI.cs`).
7. **`Assets/Scripts/Prototype/`**: Bootstrap routines and level switchers (`GameBootstrap.cs`, `Case01Initializer.cs`, `Case02Initializer.cs`, `Case03Initializer.cs`).
8. **`Assets/Scripts/Editor/`**: Custom editor utilities, menu tools, and edit-mode test suites.

---

## 3. Unity & C# Game Development Best Practices

### A. Memory Management & Zero Garbage Collection (GC)
- **Zero Allocations in Hot Loops:** Never allocate memory (`new`, string concatenation, closures, LINQ) inside `Update()`, `FixedUpdate()`, `LateUpdate()`, or render loops.
- **Cache Component References:** Cache `GetComponent<T>()` calls in `Awake()`. Never call `GetComponent<T>()` or `FindObjectOfType<T>()` in `Update()`.
- **String Handling:** Avoid string concatenation in frame updates. Use `StringBuilder` or pre-formatted cached strings for UI counters and timers.
- **Physics Non-Alloc Queries:** Always use pre-allocated buffers with non-allocating queries:
  ```csharp
  // Good: Zero-allocation query
  private readonly Collider2D[] _hitBuffer = new Collider2D[16];
  int count = Physics2D.OverlapCircleNonAlloc(origin, radius, _hitBuffer, layerMask);
  ```
- **Property ID Caching:** Cache shader and animator property IDs:
  ```csharp
  private static readonly int DissolveProperty = Shader.PropertyToID("_DissolveAmount");
  private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");
  ```

### B. Unity Lifecycle & Execution Flow
- **`Awake()`**: Self-contained initialization, instantiating pure C# services, caching local components.
- **`OnEnable()` / `OnDisable()`**: Subscribe to C# events and UnityActions in `OnEnable()`, and **always unsubscribe in `OnDisable()`** to prevent memory leaks and null reference exceptions.
- **`Start()`**: Cross-object references, initial state fetches from Managers.
- **`FixedUpdate()`**: Physics calculations only. Use `Time.fixedDeltaTime` and `Rigidbody2D` methods (`MovePosition`, `linearVelocity`).
- **`Update()`**: Input handling, timer countdowns, UI polling, non-physics movement using `Time.deltaTime`.
- **`LateUpdate()`**: Camera positioning and post-animation transforms.
- **Unity Object Null Checks:** Avoid C# null propagation `?.` or `??` on `UnityEngine.Object` subclasses when destroyed state matters (Unity overrides `==` and `!=` for pseudo-null checking). Use `if (target != null)` or `if (target)`.

### C. Serialization & Inspector Ergonomics
- Encapsulate fields with `[SerializeField] private Type _fieldName;` and expose public readonly properties:
  ```csharp
  [Header("Evidence Configuration")]
  [Tooltip("ScriptableObject data representing this physical evidence item.")]
  [SerializeField] private EvidenceSO _evidenceData;
  public EvidenceSO EvidenceData => _evidenceData;
  ```
- Use `[RequireComponent(typeof(T))]` when a component strictly relies on another (e.g. `SpriteRenderer`, `Collider2D`).
- Implement `#if UNITY_EDITOR OnValidate() #endif` to detect unassigned references or invalid ranges at edit-time.

### D. 2D / 3D Rendering & UI Optimization
- **Sorting Layers:** Maintain consistent 2D sorting order:
  - `Background` (Order 0)
  - `Characters` (Order 5)
  - `Foreground / Table` (Order 10)
  - `Pointer / Cursor` (Order 20)
  - `UI` (Canvas Screen Space - Overlay)
- **Canvas Segmentation:** Separate frequently updating UI (dialogue text, timers, cursors) into sub-canvases from static UI (background panels, borders) to avoid redrawing the entire canvas tree on every text change.
- **Raycast Target Optimization:** Uncheck `Raycast Target` on all `Image` and `TextMeshProUGUI` components that do not require pointer clicks.
- **Audio Import:** Set background music to `Streaming` / `Vorbis` (saves memory), and UI/SFX sound clips to `Decompress On Load` / `ADPCM` or `PCM` (zero latency).

---

## 4. Unity MCP Server Reference & Tool Playbook

The Unity MCP server connects AI pair programmers directly to the running Unity Editor session.

### A. Targeting Unity Instances & Sessions
- Check active instances using the `mcpforunity://instances` resource.
- When multiple editor sessions are open, set the active instance with `set_active_instance(instance="Name@hash")`.
- Alternatively, supply `unity_instance="Name@hash"` on any individual tool call.

### B. Resources vs Tools Rule
- **RESOURCES** are used for **READING** editor state:
  - `mcpforunity://editor/state`: Domain reload status, play mode status, compilation status (`data.compilation.is_compiling`, `data.advice.ready_for_tools`).
  - `mcpforunity://project/info`, `mcpforunity://project/tags`, `mcpforunity://project/layers`.
  - `mcpforunity://scene/active`, `mcpforunity://scene/gameobject/{id}`, `mcpforunity://scene/gameobject/{id}/components`.
  - `mcpforunity://custom-tools`: Dynamic project-specific tools.
- **TOOLS** are used for **MUTATIONS & ACTIONS**:
  - Manipulating GameObjects, adding components, creating assets, editing code, controlling Play Mode.

### C. Payload Safety & Pagination Guidelines
- **Scene Hierarchies:** When calling `manage_scene(action="get_hierarchy")`, always start with `page_size: 50` and traverse using `cursor`.
- **GameObject Components:** When calling `manage_gameobject(action="get_components")`, use `include_properties: false` first, and keep `page_size` small (10-25).
- **Asset Searches:** When calling `manage_asset(action="search")`, set `page_size: 25-50` and keep `generate_preview: false` to avoid huge base64 payloads.

---

## 5. Complete Unity MCP Tool Matrix (48 Tools)

### 1. Scripting & Code Generation
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `create_script` | Creates new C# script with boilerplate | `name`, `path`, `script_type`, `contents`, `namespace` |
| `script_apply_edits` | Structured AST method/class edits (Safe) | `name`, `path`, `edits` (`op`: `replace_method`, `insert_method`, `delete_method`, `anchor_insert`) |
| `apply_text_edits` | Line/range-based file edits | `uri`, `edits` (`range`, `newText`) |
| `validate_script` | Checks diagnostics and syntax errors | `uri`, `level` (`basic`, `standard`), `include_diagnostics` |
| `delete_script` | Removes a script asset | `name`, `path` |
| `execute_code` | Runs arbitrary C# in-editor in memory | `code`, `compiler` (`auto`, `roslyn`, `codedom`), `safety_checks` |
| `manage_script` | Legacy script router (prefer `script_apply_edits`) | `action` (`create`, `read`, `delete`), `name`, `path` |
| `manage_script_capabilities` | Returns supported structured ops and limits | *(None)* |

### 2. Scene & GameObject Manipulation
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `manage_scene` | Scene CRUD, hierarchy, active scene | `action` (`get_hierarchy`, `get_active`, `create`, `load`, `save`, `add_to_build`) |
| `manage_gameobject` | CRUD on GameObjects | `action` (`create`, `modify`, `delete`, `duplicate`, `move_relative`, `look_at`) |
| `find_gameobjects` | Search scene by name, tag, layer, component | `name`, `tag`, `layer`, `component_type`, `path`, `page_size` |
| `manage_components` | Add, remove, set properties on components | `action` (`add`, `remove`, `set_property`), `target`, `component_type`, `properties` |
| `manage_prefabs` | Prefab assets and Prefab Stage | `action` (`get_info`, `create_from_gameobject`, `modify_contents`, `open_prefab_stage`, `save_prefab_stage`) |

### 3. Assets, Data & Materials
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `manage_asset` | Asset database CRUD and search | `action` (`search`, `import`, `create`, `modify`, `delete`, `move`, `refresh`), `filter_type` |
| `manage_scriptable_object`| Create and patch ScriptableObject assets | `action` (`create`, `modify`), `type_name`, `target`, `patches`, `folder_path`, `asset_name` |
| `manage_material` | Material creation, shaders, properties | `action` (`create`, `set_material_color`, `set_material_shader_property`, `assign_material_to_renderer`) |
| `manage_shader` | Shader script management | `action` (`create`, `read`, `update`, `delete`), `name`, `path` |
| `manage_texture` | Procedural texture/sprite generation | `action` (`create`, `modify`, `create_sprite`), `pattern`, `palette`, `fill_color`, `as_sprite` |
| `import_model` / `_file` | Import 3D meshes (GLTF/FBX/OBJ) | `source_path`, `destination_path`, `import_materials` |
| `generate_model` / `_image` / `_audio` | AI asset generation | `prompt`, `output_path`, `asset_type` |

### 4. Physics, Camera, VFX & Graphics
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `manage_physics` | Physics settings, collision matrix, queries | `action` (`get_settings`, `set_collision_matrix`, `raycast`, `overlap_circle`), `dimension` (`2D`, `3D`) |
| `manage_camera` | Camera presets, Cinemachine Brain & capture | `action` (`create_camera`, `ensure_brain`, `set_target`, `set_lens`, `screenshot`, `screenshot_multiview`) |
| `manage_graphics` | URP volume effects, lighting, baking, stats | `action` (`volume_create`, `volume_add_effect`, `stats_get`, `skybox_set_ambient`, `feature_add`) |
| `manage_vfx` | ParticleSystem, VFX Graph, LineRenderer | `action` (`particle_play`, `vfx_set_property`, `line_set_positions`), `target`, `properties` |
| `manage_animation` | Animator controllers, states, clips | `action` (`animator_play`, `controller_create`, `controller_add_state`, `clip_create`) |
| `manage_probuilder` | In-editor 3D ProBuilder geometry | `action` (`create_shape`, `extrude_face`, `set_material`), `target`, `properties` |

### 5. UI Toolkit
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `manage_ui` | UXML/USS documents & UIDocument | `action` (`create`, `update`, `attach_ui_document`, `get_visual_tree`, `add_classes`, `style`) |

### 6. Diagnostics, Tests & Editor Control
| Tool | Primary Purpose | Key Parameters |
| :--- | :--- | :--- |
| `read_console` | Query Unity Editor console logs & errors | `types` (`["Error", "Warning", "Log"]`), `count`, `filter_text` |
| `refresh_unity` | Trigger AssetDatabase refresh & compile | `compile` (`true`/`false`), `scope` (`Assets`, `All`), `wait_for_ready` (`true`) |
| `run_tests` | Run EditMode / PlayMode NUnit test runner | `mode` (`EditMode`, `PlayMode`), `test_names`, `assembly_names` |
| `get_test_job` | Poll test execution results | `job_id` |
| `manage_editor` | Play/Pause/Stop, tags, layers, undo/redo | `action` (`play`, `pause`, `stop`, `add_tag`, `add_layer`, `undo`, `redo`) |
| `manage_profiler` | Read profiler counters & memory snapshots | `action` (`profiler_start`, `counters_get`, `snapshot_take`, `snapshot_diff`) |
| `unity_reflect` | Live C# reflection verification | `action` (`search`, `get_type`, `get_member`), `class_name`, `member_name`, `query` |
| `unity_docs` | Official Unity ScriptReference & Manual docs| `action` (`get_doc`, `get_manual`, `get_package_doc`, `lookup`), `class_name`, `slug` |
| `manage_tools` | Enable / disable tool groups | `action` (`list_groups`, `activate`, `deactivate`, `sync`), `group` |
| `debug_request_context` | MCP transport diagnostics | *(None)* |

---

## 6. Standard Unity MCP Execution Recipes

### Recipe 1: Writing a Script & Ensuring Safe Domain Reload
```
1. Write C# code (e.g. Assets/Scripts/Managers/NewFeatureManager.cs).
2. Call refresh_unity(compile=true, wait_for_ready=true).
3. Read resource mcpforunity://editor/state -> check data.compilation.is_compiling == false.
4. Call read_console(types=["Error", "Exception"], count="10").
5. If errors exist -> fix script immediately before attempting to use the component.
```

### Recipe 2: Setting up Scene GameObjects & Component Links
```
1. Check existing scene hierarchy: manage_scene(action="get_hierarchy", page_size=50).
2. Create GameObject: manage_gameobject(action="create", name="DeductionBoard", parent="_Managers").
3. Add Component: manage_components(action="add", target="DeductionBoard", component_type="DeductionBoardController").
4. Set Properties: manage_components(action="set_property", target="DeductionBoard", component_type="DeductionBoardController", properties={"_autoLinkOnDiscover": true}).
5. Save Scene: manage_scene(action="save").
```

### Recipe 3: Creating & Initializing ScriptableObjects
```
1. Create ScriptableObject asset:
   manage_scriptable_object(action="create", type_name="EvidenceSO", folder_path="Assets/Data/Case001", asset_name="EVD_NECKLACE_01")
2. Modify properties:
   manage_scriptable_object(action="modify", target="Assets/Data/Case001/EVD_NECKLACE_01.asset", patches=[
     {"path": "evidenceID", "value": "EVD_NECKLACE"},
     {"path": "evidenceName", "value": "Stolen Diamond Necklace"},
     {"path": "category", "value": 0},
     {"path": "description", "value": "An expensive heirloom necklace found in the suspect's desk."}
   ])
```

### Recipe 4: Verifying APIs to Prevent Hallucinations
```
1. Search Type: unity_reflect(action="search", query="CinemachineCamera", scope="packages")
2. Check Members: unity_reflect(action="get_type", class_name="Unity.Cinemachine.CinemachineCamera")
3. Inspect Property: unity_reflect(action="get_member", class_name="Unity.Cinemachine.CinemachineCamera", member_name="Target")
4. Consult Docs: unity_docs(action="get_doc", class_name="CinemachineCamera")
```

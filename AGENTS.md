# AGENTS.md - Multi-Agent Operating Standard & Unity 3D/2D MCP Reference

This document serves as the single source of truth for all autonomous and pair-programming AI agents (Antigravity, Cursor, Windsurf, Claude Code, OpenAI Codex, Devin, Roo Code) operating in the **CIT2101_2D** repository.

---

## 1. Project Identity & Architecture

**Project:** `CIT2101_2D` (*Case Closed* - Detective Investigation & Interrogation Game)  
**Engine:** Unity 6 (`6000.3.20f1`)  
**Pipeline & Tooling:** Universal Render Pipeline (URP 17.3), Input System 1.19, Cinemachine 3.1, UGUI 2.0 & UI Toolkit, MCPForUnity (`com.coplaydev.unity-mcp`).

### Architecture Pattern: Separation of Concerns (SoC) + YAGNI

```
Assets/Scripts/
├── Enums/       -> Pure Enum declarations (One enum per file).
├── Data/        -> ScriptableObjects & serialized data models (CaseSO, EvidenceSO, DialogueTreeSO).
├── Services/    -> Pure C# domain logic (zero MonoBehaviour dependencies, 100% unit-testable).
├── Managers/    -> MonoBehaviour controllers bridging engine lifecycle, audio, and state.
├── Gameplay/    -> World-space interactive actors, cameras, and character rendering.
├── UI/          -> Canvas views, modals, and screen coordinators.
├── Prototype/   -> Scene bootstrappers, level switchers, and mock data initializers.
└── Editor/      -> Custom Unity Editor tools, asset generators, and EditMode tests.
```

---

## 2. Mandatory Rules of Engagement for Agents

1. **Verify Unity APIs First**:
   - Unity 6 and URP/Cinemachine 3 APIs differ significantly from older Unity versions (e.g. `CinemachineCamera` vs `CinemachineVirtualCamera`, `linearVelocity` vs `velocity`).
   - Use `unity_reflect` and `unity_docs` or Context7 to verify exact type signatures and namespaces before writing C# code.
2. **Never Break Script Compilation**:
   - Unity will stop updating its domain reload if any script has compilation errors.
   - After creating or editing scripts, immediately call `refresh_unity(compile=true, wait_for_ready=true)` and inspect `read_console(types=["Error"])`.
3. **Strict Layer Separation**:
   - Never put business logic (scoring algorithms, string formatting, contradiction matching) inside `MonoBehaviour` Update loops or UI scripts. Place it in `Assets/Scripts/Services/` as pure C# classes.
   - Controllers in `Assets/Scripts/Managers/` instantiate their respective service classes in `Awake()`.
4. **Clean Serialization**:
   - Use `[SerializeField] private Type _fieldName;` and expose public readonly properties.
   - Add `[Header("...")]` and `[Tooltip("...")]` for inspector fields.
   - Never expose public mutable fields (`public int score;` is prohibited; use `public int Score => _score;`).

---

## 3. Unity & C# Performance & Memory Best Practices

### A. Zero-Garbage Collection (GC) in Runtime Loops
- No object instantiations (`new`), string formatting/concatenation, boxing, or LINQ inside `Update()`, `FixedUpdate()`, or `LateUpdate()`.
- Use pre-allocated buffers with non-allocating physics queries (`Physics2D.OverlapCircleNonAlloc`, `Physics2D.RaycastNonAlloc`).
- Cache all `GetComponent<T>()`, `Camera.main`, `Animator.StringToHash()`, and `Shader.PropertyToID()` in `Awake()`.

### B. Event Handling & Lifecycle Safety
- Subscribe to events in `OnEnable()` and **always unsubscribe in `OnDisable()`**.
- Explicitly check `if (target != null)` rather than using null-conditional `?.` on `UnityEngine.Object` subclasses when destruction lifecycle is relevant.

### C. UI & Rendering Best Practices
- Segment dynamic text and animated UI into isolated sub-canvases to prevent full-canvas batch invalidations.
- Disable `Raycast Target` on static `Image` and `TextMeshProUGUI` components.
- Configure audio assets: `Streaming` for long BGM tracks, `Decompress On Load` for low-latency SFX.

---

## 4. Unity MCP Server Reference & Integration Protocol

The Unity MCP server provides 48 tools and rich resources to inspect and control the active Unity Editor session.

### A. Session Routing & Instance Selection
- Query connected sessions: `mcpforunity://instances`
- Pin active session: `set_active_instance(instance="<Name@hash>")`
- Or pass `unity_instance="<Name@hash>"` on any individual tool call.

### B. State Inspection vs Mutation
- **Use Resources to Read State:**
  - `mcpforunity://editor/state`: Check `data.compilation.is_compiling` and `data.advice.ready_for_tools`.
  - `mcpforunity://scene/active`: Active scene metadata and root GameObjects.
  - `mcpforunity://scene/gameobject/{id}`: Detailed transform, layer, tag, and component list.
  - `mcpforunity://custom-tools`: Dynamic project-specific tools.
- **Use Tools to Mutate Engine State:**
  - Use `manage_gameobject`, `manage_components`, `manage_prefabs`, `manage_scene`, `manage_editor`.

### C. Payload Paging Guidelines
- `manage_scene(action="get_hierarchy")`: Start with `page_size: 50` and follow `next_cursor`.
- `manage_gameobject(action="get_components")`: Start with `include_properties: false` and small `page_size: 10-25`.
- `manage_asset(action="search")`: Use `page_size: 25-50` and keep `generate_preview: false`.

---

## 5. Comprehensive Unity MCP Tool Reference

```
┌────────────────────────────────────────────────────────────────────────┐
│                          UNITY MCP TOOLS MATRIX                        │
├──────────────────────┬─────────────────────────────────────────────────┤
│ Category             │ Available Tools                                 │
├──────────────────────┼─────────────────────────────────────────────────┤
│ Scripting & Code     │ create_script, script_apply_edits,               │
│                      │ apply_text_edits, validate_script, delete_script│
│                      │ execute_code, manage_script,                    │
│                      │ manage_script_capabilities                      │
├──────────────────────┼─────────────────────────────────────────────────┤
│ Scene & Hierarchy    │ manage_scene, manage_gameobject,                │
│                      │ find_gameobjects, manage_components,            │
│                      │ manage_prefabs                                  │
├──────────────────────┼─────────────────────────────────────────────────┤
│ Assets & Data        │ manage_asset, manage_scriptable_object,         │
│                      │ manage_material, manage_shader, manage_texture, │
│                      │ import_model, import_model_file,                │
│                      │ generate_model, generate_image, generate_audio  │
├──────────────────────┼─────────────────────────────────────────────────┤
│ Physics & Graphics   │ manage_physics, manage_graphics, manage_camera, │
│                      │ manage_vfx, manage_animation, manage_probuilder │
├──────────────────────┼─────────────────────────────────────────────────┤
│ UI                   │ manage_ui                                       │
├──────────────────────┼─────────────────────────────────────────────────┤
│ Diagnostics & Tests  │ read_console, refresh_unity, run_tests,         │
│                      │ get_test_job, manage_profiler, unity_reflect,   │
│                      │ unity_docs, manage_tools, debug_request_context │
└──────────────────────┴─────────────────────────────────────────────────┘
```

### Key Tool Definitions & Usage

#### 1. Code & Compilation
- `create_script(name, path, script_type, contents, namespace)`: Creates a C# script asset in `Assets/`.
- `script_apply_edits(name, path, edits, options)`: Structured method/class edits. Preferred over raw string replacement:
  - `op`: `replace_method`, `insert_method`, `delete_method`, `anchor_insert`, `anchor_replace`.
- `validate_script(uri, level, include_diagnostics)`: Validates syntax and returns compiler diagnostics.
- `execute_code(code, compiler, safety_checks)`: Compiles and runs arbitrary C# in-editor in memory without creating files.
- `read_console(types, count, filter_text)`: Reads logs, warnings, and compiler errors from the Unity Editor Console.
- `refresh_unity(compile, scope, wait_for_ready)`: Triggers AssetDatabase refresh and compilation domain reload.

#### 2. GameObjects & Components
- `find_gameobjects(name, tag, layer, component_type, path)`: Locates GameObjects by query and returns instance IDs.
- `manage_gameobject(action, name, parent, position, rotation, scale)`: CRUD operations on GameObjects (`create`, `modify`, `delete`, `duplicate`, `move_relative`).
- `manage_components(action, target, component_type, properties)`: Attach, remove, or configure component fields (`add`, `remove`, `set_property`).
- `manage_scene(action, name, path, page_size, cursor)`: Scene management (`get_hierarchy`, `get_active`, `create`, `load`, `save`).
- `manage_prefabs(action, prefab_path, target, components_to_add)`: Inspect, instantiate, or modify prefabs.

#### 3. ScriptableObjects & Assets
- `manage_scriptable_object(action, type_name, folder_path, asset_name, patches)`: Creates and mutates `.asset` ScriptableObject files via SerializedObject property paths.
- `manage_asset(action, path, filter_type, page_size)`: Searches, creates, moves, or deletes assets in the project.
- `manage_material(action, name, shader_name, properties)`: Creates and assigns materials and shaders.
- `manage_texture(action, width, height, pattern, palette, as_sprite)`: Procedurally creates textures/sprites.

#### 4. Graphics, Physics & Camera
- `manage_physics(action, dimension, settings, origin, direction)`: Configures 2D/3D physics matrices and performs spatial raycasts/overlaps.
- `manage_camera(action, preset, target, screenshot)`: Manages Cinemachine Brain, Virtual Cameras, lens properties, and captures screenshots.
- `manage_graphics(action, volume_profile, effect_type, stats_get)`: Manages URP Volumes, post-processing overrides, lighting baking, and draw call stats.
- `manage_vfx(action, target, properties)`: Configures ParticleSystem, VisualEffect Graph, LineRenderer, and TrailRenderer.
- `manage_animation(action, animator_target, clip_name, controller_path)`: Creates and manages Animator Controllers, states, transitions, and AnimationClips.

#### 5. Verification & Testing
- `unity_reflect(action, class_name, member_name, query, scope)`: Reflects live C# types from loaded assemblies to prevent hallucinations.
- `unity_docs(action, class_name, member_name, slug, queries)`: Fetches official ScriptReference and Manual documentation.
- `run_tests(mode, test_names)`: Starts EditMode / PlayMode NUnit test runner.
- `get_test_job(job_id)`: Checks test results, passes, and failure callstacks.

---

## 6. Standard Execution Recipes for Agents

### Recipe A: Adding a New Feature Script
1. Write the script using standard tools or `create_script`.
2. Call `refresh_unity(compile=true, wait_for_ready=true)`.
3. Check `read_console(types=["Error", "Exception"], count="10")`.
4. If errors are reported, fix them immediately.

### Recipe B: Constructing a Scene Hierarchy via MCP
1. Fetch parent ID or name: `find_gameobjects(name="_Managers")`.
2. Create child GameObject: `manage_gameobject(action="create", name="NewSystem", parent="_Managers")`.
3. Add component: `manage_components(action="add", target="NewSystem", component_type="NewSystemManager")`.
4. Set serialized properties: `manage_components(action="set_property", target="NewSystem", component_type="NewSystemManager", properties={"_enabled": true})`.
5. Save the scene: `manage_scene(action="save")`.

### Recipe C: Verifying Unity APIs Before Generation
1. Check class: `unity_reflect(action="search", query="CinemachineCamera")`.
2. Check members: `unity_reflect(action="get_type", class_name="Unity.Cinemachine.CinemachineCamera")`.
3. Check doc examples: `unity_docs(action="get_doc", class_name="CinemachineCamera")`.

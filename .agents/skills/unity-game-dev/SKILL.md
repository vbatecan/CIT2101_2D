---
name: unity-game-dev
description: >-
  Expert guidance and multi-step procedures for Unity 2D/3D game development, C# scripting,
  and comprehensive usage of the Unity MCP server (MCPForUnity). Use whenever creating or modifying
  Unity scripts, configuring GameObjects/Components/Prefabs, running tests, checking editor console,
  verifying Unity APIs, managing ScriptableObjects, or automating the Unity Editor.
---

# Unity Game Development & Unity MCP Skill

This skill provides step-by-step procedures for developing game features in Unity and controlling the Unity Editor via the Unity MCP server.

---

## 1. Core Principles & Philosophy

1. **Verify Before Coding**: Unity APIs change across versions (e.g. Unity 6 vs 2022 LTS). Always use `unity_reflect` and `unity_docs` or Context7 to verify class signatures and package namespaces before writing new C# scripts.
2. **Compile-Before-Use Rule**: Unity requires a domain reload after creating or modifying C# scripts. Always verify compilation success via `read_console` before attempting to attach new components or modify serialized properties.
3. **Payload Safety**: Unity scenes and asset hierarchies can produce massive JSON payloads. Always use pagination (`page_size: 25-50`) and request metadata first (`include_properties: false`, `generate_preview: false`).
4. **Architecture Separation**:
   - **Data**: ScriptableObjects in `Assets/Scripts/Data/`.
   - **Logic**: Pure C# Services in `Assets/Scripts/Services/` (zero MonoBehaviour dependencies, easily unit-tested).
   - **Engine Bridges**: MonoBehaviour Managers in `Assets/Scripts/Managers/`.
   - **Visuals / Views**: Gameplay and UI controllers in `Assets/Scripts/Gameplay/` and `Assets/Scripts/UI/`.

---

## 2. Standard Procedures

### Procedure A: Creating a New C# Script & Attaching to GameObject

```
[Step 1: Create Script] ──> [Step 2: Refresh/Compile] ──> [Step 3: Check Console]
                                                                  │
                                                          (If compilation OK)
                                                                  │
                                                                  ▼
[Step 6: Verify State]  <── [Step 5: Set Properties]  <── [Step 4: Attach Component]
```

1. **Write Script**:
   - Use `create_script` or standard file writing to create the C# file under `Assets/Scripts/...`.
   - Ensure the class name exactly matches the filename.
2. **Trigger Compilation & Wait**:
   - Call `refresh_unity(compile=true, wait_for_ready=true)` or read `mcpforunity://editor/state` until `data.compilation.is_compiling` is false and `data.advice.ready_for_tools` is true.
3. **Check Console**:
   - Call `read_console(types=["Error", "Exception"], count="10")`.
   - If compiler errors exist, fix them before proceeding.
4. **Attach Component**:
   - Use `manage_components(action="add", target="<GameObject_Name_or_ID>", component_type="<FullClassName>")`.
5. **Configure Serialized Properties**:
   - Use `manage_components(action="set_property", target="<GameObject_Name_or_ID>", component_type="<FullClassName>", properties={...})`.

---

### Procedure B: Modifying Existing C# Scripts

1. **Structured Method Edits (Preferred)**:
   - Use `script_apply_edits` with `op: "replace_method"` or `op: "insert_method"` for safe syntax boundaries.
   - Example:
     ```json
     {
       "name": "CaseManager",
       "path": "Assets/Scripts/Managers",
       "edits": [
         {
           "op": "replace_method",
           "className": "CaseManager",
           "methodName": "EvaluateCurrentProgress",
           "replacement": "public float EvaluateCurrentProgress() { return _evaluationService.CalculateProgress(activeCase); }"
         }
       ],
       "options": {"validate": "standard"}
     }
     ```
2. **Line/Range Edits**:
   - Use `apply_text_edits` or workspace `replace_file_content`.
3. **Validate & Check Console**:
   - Call `validate_script(uri="Assets/Scripts/Managers/CaseManager.cs", include_diagnostics=true)`.
   - Call `read_console(types=["Error"])`.

---

### Procedure C: Managing ScriptableObject Assets

1. **Create Asset**:
   - Call `manage_scriptable_object(action="create", type_name="CaseSO", folder_path="Assets/Data/Case001", asset_name="CASE_001_Data")`.
2. **Populate Properties**:
   - Call `manage_scriptable_object(action="modify", target="Assets/Data/Case001/CASE_001_Data.asset", patches=[{"path": "caseTitle", "value": "The Missing Necklace"}, {"path": "levelNumber", "value": 1}])`.

---

### Procedure D: Querying and Modifying GameObjects & Scene Hierarchy

1. **Find Target GameObject**:
   - Use `find_gameobjects(name="Character_Suspect_Left")` or search by tag/layer/component.
2. **Inspect GameObject & Components**:
   - Read resource `mcpforunity://scene/gameobject/{id}/components` or call `manage_components(action="get_property", ...)`.
3. **Modify Transforms or State**:
   - Use `manage_gameobject(action="modify", target="<id_or_name>", position=[x, y, z], active=true)`.

---

### Procedure E: Verifying Unity APIs (Avoid Hallucinations)

1. **Search Type**:
   - Call `unity_reflect(action="search", query="Light2D", scope="all")`.
2. **Get Type Summary**:
   - Call `unity_reflect(action="get_type", class_name="UnityEngine.Rendering.Universal.Light2D")`.
3. **Get Member Signature**:
   - Call `unity_reflect(action="get_member", class_name="UnityEngine.Rendering.Universal.Light2D", member_name="intensity")`.
4. **Fetch Official Docs & Examples**:
   - Call `unity_docs(action="get_doc", class_name="Light2D")`.

---

### Procedure F: Running Tests & Checking Profiler

1. **Execute Tests**:
   - Call `run_tests(mode="EditMode")` or `run_tests(mode="PlayMode")`.
   - Use `get_test_job(job_id="<job_id>")` to inspect test results and failures.
2. **Profile Rendering / Memory**:
   - Call `manage_graphics(action="stats_get")` to inspect draw calls, batches, and triangle counts.
   - Call `manage_profiler(action="counters_get", category="Memory")` to verify zero memory leaks.

# AGENTS.md — Case Closed development rules

This is the canonical repository guide for AI agents working on CIT2101_2D.
Read it before making changes. GEMINI.md is the Gemini entry point; shared rules belong here.
Current user instructions take precedence over repository guides and bundled skills.

## Scope and production-data protection

- Work only on the requested task. Diagnosis and review do not authorize implementation.
- **Do not touch Unity scenes unless the user explicitly requests the exact scene edit.**
  This includes scene YAML, hierarchy, transforms, active states, component assignments, prefab overrides, saves, restores, and reverts.
- Treat prefabs, ScriptableObject assets, existing .meta files, GUIDs, Inspector references, and hand-authored layouts as production data.
  Code cleanup does not authorize changes to them.
- Never run scene rebuilders, bulk layout generators, automatic UI repairs, or asset regeneration as verification.
- Never restore a dirty scene to HEAD merely because it has a diff. It may contain user work.
  Inspect and report unexpected changes; do not guess their author or cause.
- Do not start/stop Play Mode, load scenes, create temporary scene objects, or execute in-editor code just to inspect code.
  Manual gameplay checks must respect the user's editor session and scene restrictions.
- Preserve UnityEvent-facing methods, serialized names/types, PlayerPrefs keys, scene names, scoring, unlock conditions, and event timing.
- On “stop,” stop actions immediately. Do not resume an earlier refactor during a newly scoped task.

## Start with evidence

1. Read the latest request and relevant local instructions.
2. Run `git status --short`, `git diff --stat`, and `git diff --cached --stat`.
   Record existing changes, including staged additions and deletions.
3. Inspect relevant code and callers before changing an API.
4. State the small implementation scope and protected behavior.
5. Keep changes incremental and reviewable.

An empty `git diff` means no unstaged changes, not a clean repository.
Check both the index and working tree before claiming a file is unchanged.
Never stage, unstage, commit, reset, or revert user work unless requested.
Concurrent changes belong to the user until evidence establishes otherwise.

<!-- CODEGRAPH_START -->
## CodeGraph

When .codegraph/ exists, use CodeGraph before grep/find or raw code reads to locate or understand indexed code.

- Prefer the discovered `codegraph_explore` MCP tool, naming relevant symbols/files and the question.
- CLI fallback: `codegraph explore "<symbols or question>"`.
- Treat returned current source as already read. Use targeted `rg` or file reads for omitted sections, configuration, documentation, or stale/unavailable indexing.
- Do not repeat broad queries when they omit the requested code.
- Without .codegraph/, skip CodeGraph. Do not initialize, rebuild, or upgrade it without a request.
<!-- CODEGRAPH_END -->

## Project and architecture

Case Closed is a Unity 6 detective investigation and interrogation game.
ProjectSettings/ProjectVersion.txt and Packages/manifest.json are authoritative for versions;
Packages/packages-lock.json records resolved dependencies.
Current editor: 6000.3.20f1; URP 17.3, Input System 1.19, Cinemachine 3.1, UGUI, and UI Toolkit.

| Directory | Responsibility |
| --- | --- |
| Assets/Scripts/Enums | Shared enum declarations |
| Assets/Scripts/Data | Authored ScriptableObjects and serialized models |
| Assets/Scripts/Services | Domain rules, formatting, calculations, and plain C# session state |
| Assets/Scripts/Managers | Unity lifecycle adapters and workflow coordination |
| Assets/Scripts/Gameplay | World interaction, rendering, cameras, and pointer behavior |
| Assets/Scripts/UI | Views, input intent, presentation, and panel coordination |
| Assets/Scripts/Prototype | Existing case initialization and bootstrap code |
| Assets/Scripts/Editor | Editor utilities; no project unit tests |

### Runtime ownership

The refactor is partial. Inspect actual callers rather than assuming every boundary is enforced.

| State or rule | Current owner / adapter |
| --- | --- |
| Active case, investigator, evidence discovery/examination, hotspots, clues, contradictions, timer state | CaseSessionState through CaseManager |
| Suspect, dialogue tree/node, challenge and failure-reaction state | InterrogationSessionState through InterrogationManager |
| Pending deduction clue pair | DeductionSelectionState through DeductionBoardController |
| Evidence operations and connection matching | EvidenceService and DeductionService |
| Timer calculations, scoring, notebook formatting | CaseTimerService, CaseEvaluationService, NotebookFormattingService |
| Persisted settings and campaign progression | GameSettingsService / CaseProgressionService and PlayerPrefs adapters |
| Scene/UI/audio effects | Existing managers and views; migration remains incomplete |

- ScriptableObjects supply authored configuration. Never write session progress back into them.
  Legacy evidence flags may remain serialized for compatibility; runtime code must use session queries.
- Expose runtime collections through read-only APIs; mutate them through named operations.
- Preserve reset/retry boundaries, duplicate handling, and state visible to subscribers when events fire.
- Domain rules must not depend on UI, scene searches, audio, input, or manager singletons.
- Prefer cohesive existing services over one-method wrappers, speculative interfaces, a global event bus, or a DI framework.
- Existing singleton access is compatibility debt. Avoid spreading it; cache dependencies at lifecycle boundaries.
- Target changes in DialogueUI, CaseFileNotebookUI, and TableEvidenceItem carefully.
  Extract cohesive logic only as needed; preserve components and serialized references.
- Add assembly definitions only when dependency direction and serialized-script compatibility are verified.

## C# and lifecycle rules

- New Inspector fields use `[SerializeField] private`, useful Header/Tooltip attributes, and read-only public properties where needed.
- Preserve exact existing serialized names when changing visibility. Do not blanket-rename fields to add underscores.
  Use FormerlySerializedAs for necessary, verified renames. Retain legacy fields when removal would discard serialized data.
- Keep component filenames and class names aligned; preserve existing script GUIDs.
- Initialize local dependencies in Awake; connect cross-object dependencies at an appropriate lifecycle boundary.
- Subscribe in OnEnable and unsubscribe in OnDisable. Registration must be idempotent.
  Store the actual publisher so unsubscription does not resolve a different singleton.
  Use named handlers when later unsubscription is required; document different lifetime pairings.
- Use Unity-aware null checks for destroyed UnityEngine.Object instances; ?. and ?? do not implement Unity's destroyed-object semantics.
- Cache component/camera lookups and shader/animator IDs. Avoid searches, LINQ, closures, boxing, and string allocations in frame loops.
  Update displayed text when its displayed value changes.
- Verify unfamiliar Unity 6/package APIs through live reflection or official documentation before using them.
  Do not blindly copy old Cinemachine or physics signatures.
- OnValidate and editor initialization must not silently repair, reparent, regenerate, or save production objects.
- Canvas, raycast, sorting, and audio-import optimizations require relevant task scope; advice is not permission to change assets.

## Verification policy: no unit tests

The user has explicitly removed the project's unit tests.

- Do not create, restore, maintain, or run project NUnit, EditMode, PlayMode suites, fixtures, or test-only harnesses unless explicitly requested.
- Do not introduce test infrastructure or launch a second Unity import to run tests.
- Test Framework/NUnit can remain transitive dependencies of Unity tooling. Do not break unrelated packages to remove them.
- Verify source changes through caller/reference inspection, focused diff review, compilation, and Console diagnostics.
- After a C# batch, request refresh/compilation and wait for readiness **only when compatible with the user's scene/editor constraints**.
  Inspect Error and Exception entries afterward. If live verification cannot safely proceed, report compilation as unverified.
- An empty Console alone does not prove compilation succeeded. Confirm readiness and that changed scripts compiled.
  Do not clear Console history to manufacture a clean result.
- When authorized, manually check affected flows: load/retry, investigator selection, evidence/hotspots, notebook,
  dialogue/challenge, deduction, conclusion, timer, settings, or progression.
- Report exactly what was checked. Compilation alone does not prove behavior preservation.

## Unity MCP use

- Discover actual tools/resources and schemas in the current session; do not assume a fixed tool count or historical parameter list.
- Confirm the intended Unity project/instance before editor actions.
- Prefer read-only resources and bounded Console queries. Page hierarchy/assets (roughly 25–50 entries);
  request component metadata before full properties.
- Use supported compilation and Console tools with their actual schemas. Wait for readiness where supported.
- A timeout/disconnection during domain reload is inconclusive. Inspect current state before retrying.
- Do not hard-code previous socket paths, instance IDs, tokens, or ports.
  Prefer supported MCP capabilities over custom raw-socket commands.
- Refresh can invoke project callbacks and does not authorize scene saves.
  If unexpected scene changes appear, stop editor mutations, inspect staged/unstaged diffs, and avoid unsupported causal claims.
- Never print access tokens, credentials, or full authenticated process arguments.

## Worktree and handoff discipline

- Use targeted patches. Preserve unrelated edits and the Git index.
- Before large temporary builds/imports, check disk capacity and avoid duplicate Library imports.
  Never delete the main Library or workspace as a troubleshooting shortcut.
- Clean up only explicitly identified task-owned temporary files; prefer recoverable operations.
- Before handoff run `git diff --check` and inspect scoped staged/unstaged diffs for asset, scene, prefab, and GUID churn.
- Summarize the result, key files, verification evidence, and limitations.
  Describe this task's changes without claiming earlier staged work as new work.

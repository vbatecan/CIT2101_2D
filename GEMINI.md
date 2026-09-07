# GEMINI.md — Gemini entry point for Case Closed

Read [AGENTS.md](AGENTS.md) completely before acting. It is the canonical project policy for architecture,
serialization, scene protection, CodeGraph, MCP, validation, and Git handling.
Keep shared rules there instead of maintaining a second tool catalog.
Current user instructions override repository guides and skills.

## Required workflow

1. Identify the current request; do not automatically resume an earlier refactor.
2. Inspect both staged and unstaged changes. An empty working-tree diff does not mean a clean file.
3. Use CodeGraph first when indexed, then targeted reads for omitted information.
4. Make the smallest complete change that satisfies the task and preserves behavior.
5. Verify references and diffs; compile and inspect Console when editor/scene constraints permit.
6. Report completed work and unverified behavior accurately. Stop immediately when asked.

## Project-specific reminders

- **Do not edit, save, restore, revert, regenerate, or reorganize scenes without an explicit request for that exact operation.**
  Do not undo staged scene changes or blame refresh for them without evidence.
- Preserve prefab overrides, authored ScriptableObjects, Inspector assignments, script GUIDs, and serialized names/types.
- **No project unit tests:** do not add or run NUnit/EditMode/PlayMode suites or test harnesses unless explicitly requested.
  Use source inspection, compilation, Console diagnostics, and authorized manual gameplay checks.
- Runtime progress belongs in existing session-state classes, not authored evidence or case assets.
- Preserve public UnityEvent entry points, PlayerPrefs semantics, event order, and retry behavior.
- Event subscriptions must be removable and idempotent; retain the original publisher for unsubscription.
- Discover actual Unity MCP schemas. Do not assume historical tool lists, instance IDs, sockets, or parameter types.
- Never run layout repairs, scene generators, or production asset saves as a validation shortcut.
- Preserve the Git index and unrelated work. Do not stage, unstage, commit, or revert without a request.

See AGENTS.md for the current state-ownership map and complete operational rules.

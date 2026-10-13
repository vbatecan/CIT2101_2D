# Case Closed design system

This is the visual specification for Case Closed's Unity UI. Use it for new UI and changes to existing screens. It establishes a consistent direction; it does not mean every existing screen has already been migrated. Follow [AGENTS.md](AGENTS.md) for editing, scene protection, and verification.

## Direction

The interface belongs to a detective's desk: warm paper, dark ink, restrained brass edges, folders, and evidence stamps. The illustrated investigation room supplies the atmosphere. Controls and readable information use native Unity UI.

Use light surfaces with dark text, compact rectangular panels, clear alignment, and modest depth. Reserve red for danger, failed deductions, and critical time states. A case title is information, not an alert.

Keep characters, environments, evidence photographs, and meaningful pictograms as artwork. Render titles, timers, navigation labels, dialogue, and case information as editable text. Do not bake functional text into images.

## Color tokens

Hex values are sRGB. Enter them through Unity's color picker; use full opacity for text. Surface opacity is explicit per component.

| Token | Value | Use |
| --- | --- | --- |
| `ink` | `#0F0A05` | Main text, icons, and headings |
| `ink-muted` | `#4D3B2B` | Supporting text and metadata |
| `paper` | `#F2E8D1` | Secondary controls and compact panels |
| `paper-page` | `#FAF7ED` | Notebook pages, dossier bodies, and long reading surfaces |
| `paper-brass` | `#F0C980` | Primary actions and the investigation header |
| `brass-edge` | `#A67D42` | Thin borders and divider rules |
| `wood` | `#A96E49` | Decorative framing and room accents |
| `danger-surface` | `#E8AD94` | Destructive-action backgrounds |
| `danger-ink` | `#8F3329` | Critical timer digits, failure labels, and stamps |
| `warning-ink` | `#7A430C` | Warning text on light surfaces |
| `success-ink` | `#2E7D33` | Completion marks and successful deductions |
| `disabled-surface` | `#DBD1BD` | Unavailable controls |

Use `ink` on every button surface, including destructive buttons. Danger is expressed through the surface and action label, without sacrificing legibility.

`warning-ink` is a new standard. The current header timer uses `#8F520F`; align it with the darker token when that component next receives a design change. Existing success green comes from the `UITheme` defaults and must be checked against the actual surface before use on small text.

### Interaction colors

| Button variant | Rest | Hover / selected | Pressed |
| --- | --- | --- | --- |
| Primary | `#F0C980` | `#FCE0A3` | `#D9AD63` |
| Secondary | `#F2E8D1` | `#FCF7E8` | `#E0CFAB` |
| Danger | `#E8AD94` | `#F7C4A8` | `#D49178` |

Keep labels opaque during transitions. Keyboard selection receives the same visible emphasis as hover. Disabled controls use a muted surface and remain recognizably labeled; an unavailable action also needs an explanation where its reason is not obvious.

## Typography

Use **Liberation Sans SDF** for new TextMeshPro labels. The installed resource is `Fonts & Materials/LiberationSans SDF`. Existing native `Text` controls may retain their font until explicitly migrated; do not mix font families within a newly authored component.

Use bold uppercase for actions, short headings, and case identifiers. Use sentence case and regular weight for dialogue and body copy. Text outlines, glow, and heavy drop shadows are not part of the standard treatment on paper.

Sizes below are Unity reference units at a **1920 × 1080** Canvas reference resolution. They are starting values, not guarantees of readable rendered pixel sizes.

| Role | Default | Weight / case | Treatment |
| --- | --- | --- | --- |
| Main-menu display title | 64–80 | Bold, uppercase | Short, with clear space around it |
| Panel heading | 32–40 | Bold | Left aligned within reading panels |
| Active case title | 30 | Bold, uppercase | Centered; character spacing 2 |
| Timer | 26 | Bold, uppercase | Centered; character spacing 2 |
| Action label | 28 | Bold, uppercase | Centered; short verb-led wording |
| Dialogue / notebook body | 28–32 | Regular, sentence case | Left aligned; comfortable line spacing |
| Metadata | 24 | Regular or bold | Secondary ink, clearly grouped with its subject |

At the smallest supported window size, aim for at least 16 rendered pixels for reading text and 14 for compact labels. Check the actual Game view. Increase reference sizes or give the component more space before relying on auto-size. Existing navigation labels can be smaller than these targets and require a future scoped migration.

Keep short labels and timer digits on one line. Auto-size can handle modest variation, but must have an explicit minimum and maximum. Long case names must fit all authored cases. Body text wraps; it must not shrink indefinitely to fit a fixed box.

## Layout and surfaces

- Author layout with `RectTransform` anchors and offsets in the scene. Keep UI scale at `(1, 1, 1)` and let the Canvas control scaling.
- Use `Scale With Screen Size`, a 1920 × 1080 reference resolution, and the existing 0.5 width/height match as the starting configuration. Do not change an existing Canvas without task scope.
- Use a spacing scale of **4, 8, 12, 16, 24, 32, 48** reference units. Align related elements to a shared edge or center line.
- Reading panels use 24–32 units of inner padding. Compact labels use 12–20 horizontal units and 6–8 vertical units.
- Default surfaces are rectangular. Rounded corners are acceptable where an existing component already uses them, with consistent curvature within that screen.
- Use a thin brass border. The current header uses a Unity `Outline` with distance `(1.5, -1.5)` and `useGraphicAlpha = false`.
- Keep shadows subtle and limited to floating panels or interactive controls. Static header labels do not need shadows.
- Check safe screen margins and prevent overlap with characters, the evidence table, navigation, and modal close controls.

For narrow layouts, reduce unused space, wrap body content, or rearrange controls. Do not solve crowding by shrinking every label. Validate both the reference aspect ratio and the smallest supported window.

## Components

### Investigation header

The active case title and timer are native `Image` panels with TextMeshPro children. Both use `paper-brass`, `ink`, and `brass-edge`, matching the surrounding navigation.

| Element | Current scene specification |
| --- | --- |
| Case title panel | 832 × 64; centered at `(18, -67)` relative to the header's top-center anchor |
| Case title text | 20 horizontal and 6 vertical padding; 30-unit maximum, 22-unit minimum |
| Timer panel | 240 × 48; centered at `(18, -140)` relative to the same anchor |
| Timer text | 12 horizontal and 6 vertical padding; 26-unit maximum, 22-unit minimum |

Format the case label as `CASE 001: THE MISSING NECKLACE`; populate it from the active case data. Format the timer as `TIME  05:00`. Keep title and timer aligned on one center line and give each its own panel.

Normal timer text uses ink. Warning and critical states use their semantic ink tokens. Preserve existing thresholds, ticking audio, and pulse behavior. Color supports the changing countdown; it must not be the only indication of urgency.

The old title and timer artwork remains stored for compatibility, with its scene renderers disabled. Do not delete those assets or references as part of visual styling.

### Buttons

Use native `Button`, `Image`, and a text child. Primary actions use light brass, supporting actions use paper, and destructive actions use terracotta. The same visual structure applies to all variants.

Use short action labels such as `MAIN MENU`, `CASE NOTEBOOK`, `CONCLUDE CASE`, and `RETRY`. Add a separate non-raycasting `Image` for a meaningful icon. Keep close and navigation symbols recognizable.

Give controls adequate hit areas at the final rendered size. Small visible icons may sit inside larger transparent hit areas. Space neighboring actions so hover or press scaling does not cause overlap.

### Notebook, dossiers, and evidence

Use `paper-page` for reading bodies, ink for headings, and muted ink for supporting information. Use brass divider rules to separate sections. Group a label with its content; separate unrelated blocks with whitespace before adding borders.

Keep evidence photography and folder artwork intact. Put descriptions, counts, statuses, and button labels in Unity text. Use a word or symbol alongside colored success/failure marks.

### Dialogue and modals

Separate the speaker label from the body. Use paper and opaque ink with adequate contrast over the illustrated room. Preserve existing dialogue advancement and click-to-reveal behavior.

Use one clear heading, one main action, and a visible close or cancel control. A backdrop may dim the scene, but must not obscure the modal's labels. Keep modal controls inside its visual bounds.

## Motion and feedback

Keep movement brief and purposeful. Existing theme defaults use about 0.12 seconds for hover, 0.08 for press, and 0.25 for panel fades. Existing button scaling is approximately 1.032 on hover and 0.965 on press. These are reference values, not permission to change established interaction timing.

Dialogue typing follows the user's text-speed setting. Timer urgency retains its existing pulse and ticking behavior. Avoid decorative motion behind reading text or repeated whole-screen animation.

## Unity implementation rules

1. Use Unity MCP to author and save the targeted scene UI. Do not hand-edit scene YAML.
2. Prefer `Image`, `Button`, `RectTransform`, and TextMeshPro for functional interface elements. Keep runtime scripts responsible for data and interaction rather than layout creation.
3. Assign component references explicitly. Preserve script GUIDs, Inspector references, event bindings, and existing prefab overrides.
4. Keep decorative images and text non-raycasting. Only interactive surfaces should intercept pointer input.
5. For migrated components, enable an existing `useScenePresentation` option where available so runtime theme helpers preserve authored styling. Configure dynamic state colors explicitly.
6. Do not change all scenes or regenerate assets to enforce this document. Migrate components incrementally within the user's requested scope.

## Existing implementations and migration

The code currently contains overlapping definitions in `DetectiveUITheme`, `DetectiveButtonUI`, and `UITheme`. `UITheme` defaults still include dark controls and white heading/button text from an earlier visual direction. Their presence is not the specification for new paper UI.

The current reference implementation is the native case header in `Assets/Scenes/CaseManager.unity`, supported by `CaseTitleUI` and `CaseTimerUI`. The warm button palette is also documented in [UI and character art updates](docs/UI_AND_CHARACTER_ART_UPDATE.md).

Future consolidation of theme data must be an explicitly scoped implementation task. This document does not authorize broad code cleanup, scene restyling, or changes to gameplay.

## Review checklist

- Compare a Game-view capture with the surrounding interface, including normal, selected, pressed, disabled, warning, and critical states where relevant.
- Check every authored case title and representative long dialogue/notebook content for clipping and wrapping.
- Check text contrast against its actual composited surface; target 4.5:1 for normal text and 3:1 for large text. Validate rendered size before treating text as large.
- Check pointer targets, keyboard selection, and clear close/cancel controls.
- Confirm Canvas scaling, screen margins, and legibility at the smallest supported window.
- Verify changed scripts compile and inspect relevant Console diagnostics without clearing history. Follow the repository's no-unit-tests policy.
- Inspect the diff for unrelated scene, prefab, asset, and GUID changes. Preserve pre-existing work and state exactly what was verified.

# Reusable detective UI

The shared prefabs are in this folder so runtime screens can load them in both the Editor and player builds without changing existing scene references.

| Prefab | Purpose | Where to customize |
| --- | --- | --- |
| DetectiveButton | Rounded action button with a padded label and hover/focus feedback | Root DetectiveButton caption/background color, Button On Click, and RectTransform size |
| DetectiveCard | Rounded dossier surface with classification, title, body and footer slots | Child RectTransforms and Text components; add content under Footer for actions |
| DetectiveDialog | Modal backdrop containing a nested DetectiveCard and two nested DetectiveButton instances | Card and button prefab overrides; DetectiveDialog exposes the action buttons |

Edit the base card and button prefabs to update their nested instances. Use prefab variants for different layouts instead of copying their hierarchies. Shared palette and rounded-surface rendering live in DetectiveUITheme.cs. The runtime sprites use nine-slicing to keep corner radii consistent as controls resize.

The DetectiveButton component applies its caption and color when enabled. It leaves Button On Click bindings intact. DetectiveCard provides SetContent and ShowReportText; DetectiveDialog provides SetContent, SetActions, PrimaryButton and SecondaryButton. Screens own the action callbacks and game rules.

GameOverUI and UIManager's leave-case confirmation use the shared dialog and bind their existing handlers. The Case001 and Main scenes and the shared panel prefabs contain authored dialog instances, so their design is visible before Play Mode. Older scene copies receive an instance at runtime. Their legacy serialized fields remain for compatibility. ConclusionUI uses the shared card and action buttons for its runtime flow.

The Case001 and Main navigation, pause, confirmation and result buttons are authored DetectiveButton prefab instances. Panel_HeaderNav, Panel_InGameMenu, Panel_GameOver, Panel_ConclusionQuiz and Panel_ResultsScreen reuse the same button prefab. UIManager keeps authored instances and their Inspector references; its startup replacement remains a fallback for older controls. Conclude Case keeps its existing click availability and uses a muted surface until the case is ready.

The settings panels in MainMenu and Main contain a paper DetectiveCard and shared button instances for mute, reset and back actions. Existing sliders, toggles and serialized settings references remain in place. MainMenuUI updates mute captions and colors through the shared presentation so the muted state survives reopening the panel.

For another runtime screen, load a prefab component from Resources and instantiate it under the screen's canvas:

```csharp
GameObject prefab = Resources.Load<GameObject>("UI/DetectiveDialog");
DetectiveDialog view = Instantiate(prefab, panelTransform, false).GetComponent<DetectiveDialog>();
view.SetContent("INVESTIGATION BUREAU", "Review the evidence", message, titleColor);
view.SetActions("Continue", "Back");
view.PrimaryButton.onClick.AddListener(OnContinueClicked);
view.SecondaryButton.onClick.AddListener(OnBackClicked);
```

Bind handlers once per instance. Reuse SetContent when reopening the same dialog. When subscribing to external publishers, unsubscribe at the screen's usual lifecycle boundary.

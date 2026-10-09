using UnityEngine;
using UnityEditor;
using TMPro;
using CaseClosed.UI;
using CaseClosed.Data;

namespace CaseClosed.Editor
{
    /// <summary>
    /// One-click Editor tool under Tools/CaseClosed/Apply Fonts that finds every TMP_Text
    /// in the current scene and applies the right ThemedText style by role without altering text content.
    /// </summary>
    public static class ApplyFontsTool
    {
        [MenuItem("Tools/CaseClosed/Apply Fonts")]
        public static void ApplyFontsToScene()
        {
            TMP_Text[] allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Apply Case Closed Fonts");
            int group = Undo.GetCurrentGroup();

            foreach (TMP_Text text in allTexts)
            {
                if (text == null) continue;

                ThemedText themed = text.GetComponent<ThemedText>();
                if (themed == null)
                {
                    themed = Undo.AddComponent<ThemedText>(text.gameObject);
                }
                else
                {
                    Undo.RecordObject(themed, "Update Font Role");
                }

                // Infer role from name or parent context
                string goName = text.gameObject.name.ToLowerInvariant();
                string parentName = text.transform.parent != null ? text.transform.parent.name.ToLowerInvariant() : "";

                if (goName.Contains("title") || parentName.Contains("title") || goName.Contains("logo"))
                {
                    themed.Role = FontRole.DisplayTitle;
                }
                else if (goName.Contains("heading") || goName.Contains("header") || goName.Contains("plaque") || goName.Contains("case"))
                {
                    themed.Role = FontRole.HeadingPlaque;
                }
                else if (goName.Contains("btn") || goName.Contains("button") || parentName.Contains("button") || parentName.Contains("btn"))
                {
                    themed.Role = FontRole.UIButton;
                }
                else if (goName.Contains("dialog") || goName.Contains("dialogue") || goName.Contains("speech") || goName.Contains("body"))
                {
                    themed.Role = FontRole.BodyDialogue;
                }
                else if (goName.Contains("typewriter") || goName.Contains("clue") || goName.Contains("evidence") || goName.Contains("notebook") || goName.Contains("desc"))
                {
                    themed.Role = FontRole.TypewriterCaseFile;
                }
                else
                {
                    themed.Role = FontRole.BodyDialogue;
                }

                themed.ApplyStyle();
                EditorUtility.SetDirty(text.gameObject);
                count++;
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[CaseClosed:Editor] Styled {count} TMP_Text components with ThemedText typography.");
            EditorUtility.DisplayDialog("Apply Fonts Complete", $"Successfully applied typography styling to {count} text elements.", "OK");
        }
    }
}

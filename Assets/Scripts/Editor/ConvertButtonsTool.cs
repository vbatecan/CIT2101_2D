using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CaseClosed.UI;
using CaseClosed.Data;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor tool under Tools/CaseClosed/Convert Buttons to ThemedButton that upgrades
    /// standard uGUI Buttons to ThemedButton in place, preserving existing OnClick listeners.
    /// </summary>
    public static class ConvertButtonsTool
    {
        [MenuItem("Tools/CaseClosed/Convert Buttons to ThemedButton")]
        public static void ConvertButtonsInScene()
        {
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Convert Buttons to ThemedButton");
            int group = Undo.GetCurrentGroup();

            foreach (Button btn in buttons)
            {
                if (btn == null) continue;

                ThemedButton themed = btn.GetComponent<ThemedButton>();
                if (themed == null)
                {
                    themed = Undo.AddComponent<ThemedButton>(btn.gameObject);
                }
                else
                {
                    Undo.RecordObject(themed, "Update ThemedButton");
                }

                string goName = btn.gameObject.name.ToLowerInvariant();
                if (goName.Contains("quit") || goName.Contains("exit") || goName.Contains("danger") || goName.Contains("delete"))
                {
                    themed.Variant = ButtonVariant.Danger;
                }
                else if (goName.Contains("close") || goName.Contains("back") || goName.Contains("pause") || goName.Contains("menu") || goName.Contains("tab"))
                {
                    themed.Variant = goName.Contains("tab") ? ButtonVariant.TabButton : ButtonVariant.IconButton;
                }
                else if (goName.Contains("folder") || goName.Contains("card"))
                {
                    themed.Variant = ButtonVariant.FolderCardButton;
                }
                else
                {
                    themed.Variant = ButtonVariant.Primary;
                }

                themed.ResolveReferences();
                themed.ApplyVariantStyle();
                EditorUtility.SetDirty(btn.gameObject);
                count++;
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[CaseClosed:Editor] Upgraded {count} buttons to ThemedButton.");
            EditorUtility.DisplayDialog("Convert Buttons Complete", $"Successfully upgraded {count} buttons to ThemedButton.", "OK");
        }
    }
}

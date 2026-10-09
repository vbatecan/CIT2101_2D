using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    [Serializable]
    public class MoodExpression
    {
        public string mood; // e.g. "Normal", "Nervous", "Defensive", "Shocked"
        public Sprite expressionSprite;
    }

    /// <summary>
    /// Single source of truth ScriptableObject for a character/suspect, linking
    /// dialogue strips, portrait boxes, facial expressions, and dossier descriptions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "Case Closed/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public Color nameColor = new Color(0.96f, 0.78f, 0.35f, 1f);

        [Header("Dialogue & Portrait Sprites")]
        [Tooltip("The dialogue banner strip from Assets/Assets/DIALOG BOXES")]
        public Sprite dialogSprite;

        [Tooltip("The portrait box frame from Assets/Assets/CHARACTERS/Boxes")]
        public Sprite portraitBox;

        [Tooltip("Optional suspect dossier folder tab sprite")]
        public Sprite suspectFolderSprite;

        [Header("Facial Expressions")]
        public List<MoodExpression> expressions = new List<MoodExpression>();

        [Header("Dossier Description")]
        [TextArea(3, 6)]
        public string descriptionText;
        [Tooltip("Authored dossier graphic from CHARACTERS/Descriptions")]
        public Sprite descriptionGraphic;

        public Sprite GetExpression(string mood)
        {
            if (expressions != null)
            {
                foreach (var exp in expressions)
                {
                    if (exp != null && exp.mood.Equals(mood, StringComparison.OrdinalIgnoreCase))
                    {
                        return exp.expressionSprite;
                    }
                }
            }
            return portraitBox;
        }
    }
}

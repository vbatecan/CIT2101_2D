using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    /// <summary>Shared authored art for both asset-backed and runtime-created character profiles.</summary>
    [CreateAssetMenu(fileName = "CharacterExpressions", menuName = "Case Closed/Character Expression Library")]
    public class CharacterExpressionLibrarySO : ScriptableObject
    {
        [Serializable]
        private class CharacterArt
        {
            public string characterId;
            public List<ExpressionSpriteMapping> expressions = new List<ExpressionSpriteMapping>();
        }

        [SerializeField, Tooltip("Expression sprites keyed by the character IDs used in dialogue.")]
        private List<CharacterArt> characters = new List<CharacterArt>();

        public Sprite GetSprite(string characterId, CharacterExpression expression)
        {
            if (string.IsNullOrEmpty(characterId) || characters == null) return null;
            foreach (CharacterArt character in characters)
            {
                if (character == null || character.characterId != characterId || character.expressions == null) continue;
                foreach (ExpressionSpriteMapping mapping in character.expressions)
                {
                    if (mapping != null && mapping.expression == expression && mapping.sprite != null)
                        return mapping.sprite;
                }
            }
            return null;
        }
    }
}

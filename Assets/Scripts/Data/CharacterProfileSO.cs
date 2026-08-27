using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    /// <summary>
    /// Maps a specific facial expression enum to its corresponding visual sprite asset.
    /// </summary>
    [Serializable]
    public class ExpressionSpriteMapping
    {
        /// <summary>The emotional expression mapped to this sprite.</summary>
        public CharacterExpression expression;

        /// <summary>The 2D sprite visual representing the expression.</summary>
        public Sprite sprite;
    }

    /// <summary>
    /// ScriptableObject defining suspect/witness identity, dossier background, personality, and visual expressions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterProfile", menuName = "Case Closed/Character Profile")]
    public class CharacterProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;
        public string fullName;
        public int age;
        public string occupation;
        public string relationshipToVictim;
        public PersonalityTrait personalityTrait;

        [Header("Dossier Details")]
        [TextArea(3, 5)]
        public string background;
        [TextArea(2, 4)]
        public string knownConflicts;
        [TextArea(2, 4)]
        public string possibleMotives;
        [TextArea(2, 4)]
        public string alibi;

        [Header("Sprites & Visual Expressions")]
        public Sprite defaultSittingPose;
        public List<ExpressionSpriteMapping> expressions = new List<ExpressionSpriteMapping>();

        /// <summary>
        /// Retrieves the matching portrait sprite for a requested character expression, falling back to the default pose.
        /// </summary>
        /// <param name="expr">The character expression to look up.</param>
        /// <returns>The mapped <see cref="Sprite"/> if found; otherwise, <see cref="defaultSittingPose"/>.</returns>
        public Sprite GetSpriteForExpression(CharacterExpression expr)
        {
            if (expressions != null)
            {
                foreach (var mapping in expressions)
                {
                    if (mapping != null && mapping.expression == expr)
                        return mapping.sprite;
                }
            }
            return defaultSittingPose;
        }
    }
}

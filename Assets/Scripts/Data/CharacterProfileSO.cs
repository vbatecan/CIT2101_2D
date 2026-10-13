using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    [Serializable]
    public class ExpressionSpriteMapping
    {
        public CharacterExpression expression;

        public Sprite sprite;
    }

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

        public Sprite GetSpriteForExpression(CharacterExpression expr)
        {
            if (expressions != null)
            {
                foreach (var mapping in expressions)
                {
                    if (mapping != null && mapping.expression == expr && mapping.sprite != null)
                        return mapping.sprite;
                }
            }
            return defaultSittingPose;
        }
    }
}

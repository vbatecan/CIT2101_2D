using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    public enum PersonalityTrait
    {
        Calm,
        Nervous,
        Defensive,
        Aggressive,
        Secretive,
        Confident
    }

    public enum CharacterExpression
    {
        Neutral,
        Curious,
        Nervous,
        Angry,
        Sad,
        Surprised,
        Defensive,
        Shocked,
        Thinking,
        Smug
    }

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
            foreach (var mapping in expressions)
            {
                if (mapping.expression == expr)
                    return mapping.sprite;
            }
            return defaultSittingPose;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    [Serializable]
    public class DialogueNodeChoice
    {
        public string choiceText;
        public string targetNodeId;
        public string requiredClueId;
    }

    [Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public string speakerId;
        public string speakerName;
        public CharacterExpression expression = CharacterExpression.Neutral;

        [TextArea(3, 6)]
        public string statementText;

        [Header("Challenge System")]
        public bool isChallengeable = false;
        public string targetContradictionRuleId;

        [Header("Branching Navigation")]
        public List<DialogueNodeChoice> choices = new List<DialogueNodeChoice>();
        public string defaultNextNodeId;

        [Header("Story Rewards")]
        public List<string> unlockEvidenceOnComplete = new List<string>();
    }

    [CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Case Closed/Dialogue Tree")]
    public class DialogueTreeSO : ScriptableObject
    {
        public string treeId;
        public string characterId;
        public string startNodeId;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        public DialogueNode GetNodeById(string nodeId)
        {
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null && node.nodeId == nodeId) return node;
                }
            }
            return null;
        }
    }
}

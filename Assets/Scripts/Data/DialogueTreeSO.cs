using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    [Serializable]
    public class DialogueNodeChoice
    {
        public string choiceText;
        public string targetNodeId;
        public string requiredClueId; // Optional lock
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
            foreach (var node in nodes)
            {
                if (node.nodeId == nodeId) return node;
            }
            return null;
        }
    }
}

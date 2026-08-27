using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    /// <summary>
    /// Represents a branching dialogue choice that redirects conversation or requires unlocked clues.
    /// </summary>
    [Serializable]
    public class DialogueNodeChoice
    {
        public string choiceText;
        public string targetNodeId;
        public string requiredClueId; // Optional lock
    }

    /// <summary>
    /// Represents an individual dialogue statement with speaker info, challengeable flag, and branching options.
    /// </summary>
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

    /// <summary>
    /// ScriptableObject defining an interrogation dialogue tree for a character.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Case Closed/Dialogue Tree")]
    public class DialogueTreeSO : ScriptableObject
    {
        public string treeId;
        public string characterId;
        public string startNodeId;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        /// <summary>
        /// Retrieves a specific dialogue node by its identifier.
        /// </summary>
        /// <param name="nodeId">The unique node identifier.</param>
        /// <returns>The matching <see cref="DialogueNode"/> if found; otherwise, null.</returns>
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

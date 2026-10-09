using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("The text displayed on the choice button.")]
        public string choiceText = "Press for more details";

        [Tooltip("Target line index within this DialogueData (-1 to proceed sequentially, or specific branch).")]
        public int targetLineIndex = -1;

        [Tooltip("Optional dialogue tree node ID to jump to in InterrogationManager.")]
        public string targetNodeId = "";
    }

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Name displayed on the speaker badge (e.g. 'Charl Vonn Pascual', 'Detective').")]
        public string speakerName = "Suspect";

        [Tooltip("Character identifier (e.g. 'CHAR_CHARL_PASCUAL', 'Detective').")]
        public string speakerId = "";

        [Tooltip("Which suspect slot is actively speaking (dims the other suspect).")]
        public CharacterSlot speakerSlot = CharacterSlot.PrimarySuspect;

        [TextArea(2, 5)]
        [Tooltip("The dialogue text typed onto the screen.")]
        public string text = "";

        [Tooltip("Optional expression sprite to display in the portrait frame or suspect display.")]
        public Sprite portraitSprite;

        [Tooltip("Emotional expression applied to the character portrait.")]
        public CharacterExpression expression = CharacterExpression.Neutral;

        [Tooltip("Optional choice branches presented after this line finishes typing.")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        public bool HasChoices => choices != null && choices.Count > 0;
    }

    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "Case Closed/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Conversation Metadata")]
        public string dialogueId = "DLG_CASE002_INTRO";
        public string conversationTitle = "Interrogation: Charl Vonn Pascual";

        [Header("Dialogue Sequence")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        public int LineCount => lines != null ? lines.Count : 0;

        public DialogueLine GetLine(int index)
        {
            if (lines != null && index >= 0 && index < lines.Count)
            {
                return lines[index];
            }
            return null;
        }
    }
}

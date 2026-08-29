using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Prototype Level 1 Initializer: Generates the "Missing Necklace" case data at runtime.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class Case01Initializer : MonoBehaviour
    {
        /// <summary>Whether to automatically initialize and load Case 01 on Start.</summary>
        public bool initializeOnStart = false;

        /// <summary>
        /// Automatically loads Case 01 on start if <see cref="initializeOnStart"/> is enabled.
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                CaseSO case01 = CreateCase01Data();
                CaseManager.Instance?.LoadCase(case01);

                if (InterrogationManager.Instance != null && case01.primarySuspect != null && case01.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(case01.primarySuspect, case01.dialogueTrees[0]);
                }
            }
        }

        /// <summary>
        /// Creates and populates the runtime <see cref="CaseSO"/> data for Level 1: The Missing Necklace.
        /// </summary>
        /// <returns>A fully configured <see cref="CaseSO"/> ScriptableObject instance.</returns>
        public CaseSO CreateCase01Data()
        {
            CaseSO c = ScriptableObject.CreateInstance<CaseSO>();
            c.levelNumber = 1;
            c.caseId = "LEVEL_01";
            c.caseTitle = "LEVEL 1: The Missing Necklace";
            c.dateAndLocation = "Stormy Evening - High-Society Manor Study";
            c.incidentDescription = "A valuable family necklace disappeared from the manor safe during a stormy evening gathering.";
            c.objective = "Interrogate Vince Angelo Batecan, inspect table evidence, disprove his kitchen alibi, and uncover the truth.";
            c.victimInfo = "Kirby Raymundo (Aristocrat - Proud & Demanding Owner)";
            if (CaseManager.Instance != null && CaseManager.Instance.selectedInvestigator != null)
            {
                c.leadInvestigator = CaseManager.Instance.selectedInvestigator;
            }

            // Primary Suspect: Vince Angelo Batecan (Nephew)
            CharacterProfileSO vince = ScriptableObject.CreateInstance<CharacterProfileSO>();
            vince.characterId = "CHAR_VINCE_BATECAN";
            vince.fullName = "Vince Angelo Batecan";
            vince.age = 25;
            vince.occupation = "Nephew of Kirby Raymundo";
            vince.relationshipToVictim = "Nephew";
            vince.personalityTrait = PersonalityTrait.Defensive;
            vince.alibi = "Claims he stayed in the kitchen from 8:30 PM until everyone started shouting.";
            vince.possibleMotives = "Urgent gambling debts owed to local loan sharks.";
            vince.knownConflicts = "Frequently argued with uncle Kirby Raymundo over financial allowance.";
            c.primarySuspect = vince;

            // Evidence Items
            // 1. Family Photograph
            EvidenceSO evPhoto = ScriptableObject.CreateInstance<EvidenceSO>();
            evPhoto.id = "EVD_FAMILY_PHOTO";
            evPhoto.evidenceName = "Family Photograph";
            evPhoto.category = EvidenceCategory.Photograph;
            evPhoto.baseDescription = "A photograph taken at 8:45 PM showing the study doorway.";
            evPhoto.detailedObservation = "A distinct silhouette matching Vince is visible standing near the study door.";
            evPhoto.unlockedClueText = "Vince silhouette spotted near study doorway at 8:45 PM.";
            evPhoto.startsDiscovered = true;

            EvidenceHotspot spotDoor = new EvidenceHotspot();
            spotDoor.hotspotId = "SPOT_DOORWAY_SILHOUETTE";
            spotDoor.hotspotTitle = "Study Doorway Silhouette";
            spotDoor.normalizedPosition = new Vector2(0.3f, 0.5f);
            spotDoor.observationText = "Silhouette matching Vince standing right outside study room at 8:45 PM.";
            spotDoor.clueUnlockedId = "CLUE_VINCE_AT_DOOR";
            evPhoto.hotspots.Add(spotDoor);
            c.evidenceItems.Add(evPhoto);

            // 2. Broken Teacup
            EvidenceSO evTeacup = ScriptableObject.CreateInstance<EvidenceSO>();
            evTeacup.id = "EVD_BROKEN_TEACUP";
            evTeacup.evidenceName = "Broken Teacup";
            evTeacup.category = EvidenceCategory.PhysicalClue;
            evTeacup.baseDescription = "Found shattered inside the locked study, right near the safe.";
            evTeacup.unlockedClueText = "Teacup shattered directly in front of the safe during break-in.";
            evTeacup.startsDiscovered = true;
            c.evidenceItems.Add(evTeacup);

            // 3. Kitchen Receipt / Log
            EvidenceSO evKitchenLog = ScriptableObject.CreateInstance<EvidenceSO>();
            evKitchenLog.id = "EVD_KITCHEN_LOG";
            evKitchenLog.evidenceName = "Kitchen Pantry Log";
            evKitchenLog.category = EvidenceCategory.Document;
            evKitchenLog.baseDescription = "Logbook entry noting the kitchen pantry was locked by staff from 8:30 PM to 9:15 PM.";
            evKitchenLog.unlockedClueText = "Kitchen pantry was locked by staff from 8:30 PM to 9:15 PM; Vince could not have been inside!";
            evKitchenLog.startsDiscovered = true;
            c.evidenceItems.Add(evKitchenLog);

            // Dialogue Tree
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_VINCE_01";
            tree.characterId = vince.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Contradictory Alibi)
            DialogueNode node1 = new DialogueNode();
            node1.nodeId = "NODE_01";
            node1.speakerId = vince.characterId;
            node1.speakerName = vince.fullName;
            node1.expression = CharacterExpression.Defensive;
            node1.statementText = "I never went near the study! I stayed in the kitchen from 8:30 PM until everyone started shouting!";
            node1.isChallengeable = true;
            node1.targetContradictionRuleId = "RULE_VINCE_ALIBI_LIE";
            tree.nodes.Add(node1);

            // Node 2 (Nervous Confession)
            DialogueNode node2 = new DialogueNode();
            node2.nodeId = "NODE_02_CONFESSION";
            node2.speakerId = vince.characterId;
            node2.speakerName = vince.fullName;
            node2.expression = CharacterExpression.Nervous;
            node2.statementText = "W-what?! The kitchen pantry log? Fine! The kitchen was locked... I needed money to clear my debts, so I took the necklace!";
            node2.isChallengeable = false;
            tree.nodes.Add(node2);

            c.dialogueTrees.Add(tree);

            // Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_VINCE_ALIBI_LIE";
            rule1.ruleTitle = "Locked Kitchen Contradiction";
            rule1.targetStatementNodeId = "NODE_01";
            rule1.requiredEvidenceId = "EVD_KITCHEN_LOG";
            rule1.reactionExpression = CharacterExpression.Nervous;
            rule1.reactionDialogue = "Vince shifts nervously and stammers: \"Wait... the kitchen log shows it was locked by staff? I... I...\"";
            rule1.unlockedDialogueNodeId = "NODE_02_CONFESSION";
            rule1.unlockedClueId = "CLUE_VINCE_STAGED_BREAKIN";
            rule1.unlockedClueText = "Vince Angelo Batecan confessed to staging the break-in for debt money!";
            c.contradictionRules.Add(rule1);

            // Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_VINCE_TIMELINE";
            conn1.connectionTitle = "Photo Silhouette & Kitchen Log";
            conn1.clueA_Id = "CLUE_VINCE_AT_DOOR";
            conn1.clueB_Id = "EVD_KITCHEN_LOG_BASE_CLUE";
            conn1.resultClueId = "CLUE_VINCE_OUTSIDE_STUDY";
            conn1.resultClueTitle = "Vince Was Outside Study At 8:45 PM";
            conn1.deductionText = "The kitchen log proves Vince was not in the kitchen, while the photo places him right outside the study door!";
            c.clueConnections.Add(conn1);

            // Conclusion Questions
            ConclusionQuestion q1 = new ConclusionQuestion();
            q1.questionId = "Q_SUSPECT";
            q1.questionText = "Who stole Kirby Raymundo's necklace?";
            q1.options = new List<string> { "Vince Angelo Batecan", "Kirby Raymundo", "House Staff" };
            q1.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion();
            q2.questionId = "Q_MOTIVE";
            q2.questionText = "What was Vince's motive?";
            q2.options = new List<string> { "Pay Off Debts", "Jealousy", "Accidental Spillage" };
            q2.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion();
            q3.questionId = "Q_EVIDENCE";
            q3.questionText = "Which evidence disproved Vince's kitchen alibi?";
            q3.options = new List<string> { "Kitchen Pantry Log", "Broken Teacup", "Family Photograph" };
            q3.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q3);

            return c;
        }
    }
}

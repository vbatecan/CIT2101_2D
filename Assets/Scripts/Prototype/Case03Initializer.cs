using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Prototype Level 3 Initializer: Generates the "The Last Call" case data at runtime.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class Case03Initializer : MonoBehaviour
    {
        /// <summary>Whether to automatically initialize and load Case 03 on Start.</summary>
        public bool initializeOnStart = false;

        /// <summary>
        /// Automatically loads Case 03 on start if <see cref="initializeOnStart"/> is enabled.
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                CaseSO case03 = CreateCase03Data();
                CaseManager.Instance?.LoadCase(case03);

                if (InterrogationManager.Instance != null && case03.primarySuspect != null && case03.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(case03.primarySuspect, case03.dialogueTrees[0]);
                }
            }
        }

        /// <summary>
        /// Creates and populates the runtime <see cref="CaseSO"/> data for Level 3: The Last Call.
        /// </summary>
        /// <returns>A fully configured <see cref="CaseSO"/> ScriptableObject instance.</returns>
        public CaseSO CreateCase03Data()
        {
            CaseSO c = ScriptableObject.CreateInstance<CaseSO>();
            c.levelNumber = 3;
            c.caseId = "LEVEL_03";
            c.caseTitle = "LEVEL 3: The Last Call";
            c.dateAndLocation = "After Hours - Downtown Coffee Shop Office";
            c.incidentDescription = "Tech startup founder Kurt Miguel Ancheta's secret prototype drive went missing from his bag after a late meeting.";
            c.objective = "Interrogate Shanaia Ortega, examine digital logs and CCTV stills, expose her false departure claim, and recover the stolen prototype.";
            c.victimInfo = "Kurt Miguel Ancheta (Startup Founder - Distressed Victim)";
            if (CaseManager.Instance != null && CaseManager.Instance.selectedInvestigator != null)
            {
                c.leadInvestigator = CaseManager.Instance.selectedInvestigator;
            }

            // Primary Witness / Suspect: Shanaia Ortega (Lead Software Developer)
            CharacterProfileSO shanaia = ScriptableObject.CreateInstance<CharacterProfileSO>();
            shanaia.characterId = "CHAR_SHANAIA_ORTEGA";
            shanaia.fullName = "Shanaia Ortega";
            shanaia.age = 27;
            shanaia.occupation = "Lead Software Developer & Partner";
            shanaia.relationshipToVictim = "Business Partner";
            shanaia.personalityTrait = PersonalityTrait.Calm;
            shanaia.alibi = "Claims she went straight home at 5:30 PM and never contacted Kurt or returned to the cafe.";
            shanaia.possibleMotives = "Steal proprietary code before getting fired.";
            c.primarySuspect = shanaia;

            // Secondary Witness / Key Informant: Shan Jaraba
            CharacterProfileSO shan = ScriptableObject.CreateInstance<CharacterProfileSO>();
            shan.characterId = "CHAR_SHAN_JARABA";
            shan.fullName = "Shan Jaraba";
            shan.age = 29;
            shan.occupation = "Cafe Manager & Key Informant";
            shan.relationshipToVictim = "Cafe Manager";
            shan.personalityTrait = PersonalityTrait.Secretive;
            shan.alibi = "Working at register until 7:30 PM closing.";
            c.additionalSuspects.Add(shan);

            // Register Investigator Profile: Detective Miguel Borja
            CharacterProfileSO miguel = ScriptableObject.CreateInstance<CharacterProfileSO>();
            miguel.characterId = "CHAR_MIGUEL_BORJA";
            miguel.fullName = "Detective Miguel Borja";
            miguel.age = 36;
            miguel.occupation = "Lead Digital Forensics Detective";
            miguel.personalityTrait = PersonalityTrait.Methodical;
            miguel.background = "High-tech forensics specialist with expertise in electronic logs, cyber forensics, and spotting technical alibi inconsistencies.";
            CaseManager.Instance?.RegisterAvailableInvestigator(miguel);

            // Evidence Items
            // 1. Victim's Smartphone Log
            EvidenceSO evPhoneLog = ScriptableObject.CreateInstance<EvidenceSO>();
            evPhoneLog.id = "EVD_SMARTPHONE_LOG";
            evPhoneLog.evidenceName = "Victim's Smartphone Call Log";
            evPhoneLog.category = EvidenceCategory.DigitalRecord;
            evPhoneLog.baseDescription = "Call log extracted from Kurt Miguel Ancheta's phone.";
            evPhoneLog.detailedObservation = "Shows an unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.unlockedClueText = "Unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.startsDiscovered = true;
            c.evidenceItems.Add(evPhoneLog);

            // 2. Coffee Shop CCTV Still
            EvidenceSO evCctv = ScriptableObject.CreateInstance<EvidenceSO>();
            evCctv.id = "EVD_CCTV_STILL";
            evCctv.evidenceName = "Coffee Shop CCTV Frame";
            evCctv.category = EvidenceCategory.Photograph;
            evCctv.baseDescription = "Security footage capture from the back exit camera.";
            evCctv.detailedObservation = "Clearly shows Shanaia's distinct jacket entering the back exit door at 7:10 PM.";
            evCctv.unlockedClueText = "Shanaia's jacket captured entering cafe back exit at 7:10 PM.";
            evCctv.startsDiscovered = true;

            EvidenceHotspot spotJacket = new EvidenceHotspot();
            spotJacket.hotspotId = "SPOT_DISTINCT_JACKET";
            spotJacket.hotspotTitle = "Shanaia's Custom Jacket";
            spotJacket.normalizedPosition = new Vector2(0.4f, 0.6f);
            spotJacket.observationText = "Shanaia entering back door of cafe at 7:10 PM, 1.5 hours after claiming she left!";
            spotJacket.clueUnlockedId = "CLUE_SHANAIA_RETURNED";
            evCctv.hotspots.Add(spotJacket);
            c.evidenceItems.Add(evCctv);

            // 3. Resignation Letter Draft
            EvidenceSO evDraft = ScriptableObject.CreateInstance<EvidenceSO>();
            evDraft.id = "EVD_RESIGNATION_LETTER";
            evDraft.evidenceName = "Termination Notice Draft";
            evDraft.category = EvidenceCategory.Document;
            evDraft.baseDescription = "Drafted letter found inside Kurt's briefcase.";
            evDraft.unlockedClueText = "Kurt planned to fire Shanaia for secretly selling company data to rival firms.";
            evDraft.startsDiscovered = true;
            c.evidenceItems.Add(evDraft);

            // Dialogue Tree for Shanaia Ortega
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_SHANAIA_01";
            tree.characterId = shanaia.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Contradictory Statement)
            DialogueNode node1 = new DialogueNode();
            node1.nodeId = "NODE_01";
            node1.speakerId = shanaia.characterId;
            node1.speakerName = shanaia.fullName;
            node1.expression = CharacterExpression.Calm;
            node1.statementText = "Once our 5:30 PM meeting wrapped up, I went straight home. I didn't contact Kurt or return to the cafe for the rest of the night.";
            node1.isChallengeable = true;
            node1.targetContradictionRuleId = "RULE_SHANAIA_TIMELINE_LIE";
            tree.nodes.Add(node1);

            // Node 2 (Angry / Shocked Confession)
            DialogueNode node2 = new DialogueNode();
            node2.nodeId = "NODE_02_CONFESSION";
            node2.speakerId = shanaia.characterId;
            node2.speakerName = shanaia.fullName;
            node2.expression = CharacterExpression.Angry;
            node2.statementText = "What?! You found the CCTV camera footage? Kurt was going to fire me and take my code! I snuck back in at 7:10 PM to take what belongs to me!";
            node2.isChallengeable = false;
            tree.nodes.Add(node2);

            c.dialogueTrees.Add(tree);

            // Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_SHANAIA_TIMELINE_LIE";
            rule1.ruleTitle = "False Departure Claim";
            rule1.targetStatementNodeId = "NODE_01";
            rule1.requiredEvidenceId = "EVD_CCTV_STILL";
            rule1.reactionExpression = CharacterExpression.Shocked;
            rule1.reactionDialogue = "Shanaia's calm veneer snaps into fury: \"CCTV at the back exit? How did you get access to Shan Jaraba's private feeds?\"";
            rule1.unlockedDialogueNodeId = "NODE_02_CONFESSION";
            rule1.unlockedClueId = "CLUE_SHANAIA_CONFESSED";
            rule1.unlockedClueText = "Shanaia Ortega confessed to sneaking back at 7:10 PM and stealing the prototype drive!";
            c.contradictionRules.Add(rule1);

            // Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_SHANAIA_DIGITAL_TRAIL";
            conn1.connectionTitle = "CCTV Entry & Encrypted Phone Call";
            conn1.clueA_Id = "CLUE_SHANAIA_RETURNED";
            conn1.clueB_Id = "EVD_SMARTPHONE_LOG_BASE_CLUE";
            conn1.resultClueId = "CLUE_PROTOTYPE_THEFT_TIMELINE";
            conn1.resultClueTitle = "Shanaia Was In Office At Theft Time";
            conn1.deductionText = "CCTV footage places Shanaia at the back door at 7:10 PM, right before her 7:15 PM encrypted phone call!";
            c.clueConnections.Add(conn1);

            // Conclusion Questions
            ConclusionQuestion q1 = new ConclusionQuestion();
            q1.questionId = "Q_SUSPECT";
            q1.questionText = "Who stole Kurt Miguel Ancheta's prototype drive?";
            q1.options = new List<string> { "Shanaia Ortega (Lead Developer)", "Shan Jaraba (Cafe Manager)", "External Hacker" };
            q1.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion();
            q2.questionId = "Q_MOTIVE";
            q2.questionText = "What motivated Shanaia to steal the drive?";
            q2.options = new List<string> { "Steal prototype before being fired", "Accidental mix-up", "Blackmail" };
            q2.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion();
            q3.questionId = "Q_EVIDENCE";
            q3.questionText = "Which evidence disproved Shanaia's alibi?";
            q3.options = new List<string> { "Coffee Shop CCTV Frame", "Termination Notice Draft", "Victim's Smartphone Call Log" };
            q3.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q3);

            return c;
        }
    }
}

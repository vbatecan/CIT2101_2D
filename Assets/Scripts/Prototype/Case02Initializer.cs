using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Prototype Level 2 Initializer: Generates the "Shattered Mirror" case data at runtime.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class Case02Initializer : MonoBehaviour
    {
        /// <summary>Whether to automatically initialize and load Case 02 on Start.</summary>
        public bool initializeOnStart = false;

        /// <summary>
        /// Automatically loads Case 02 on start if <see cref="initializeOnStart"/> is enabled.
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                CaseSO case02 = CreateCase02Data();
                CaseManager.Instance?.LoadCase(case02);

                if (InterrogationManager.Instance != null && case02.primarySuspect != null && case02.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(case02.primarySuspect, case02.dialogueTrees[0]);
                }
            }
        }

        /// <summary>
        /// Creates and populates the runtime <see cref="CaseSO"/> data for Level 2: The Shattered Mirror.
        /// </summary>
        /// <returns>A fully configured <see cref="CaseSO"/> ScriptableObject instance.</returns>
        public CaseSO CreateCase02Data()
        {
            CaseSO c = ScriptableObject.CreateInstance<CaseSO>();
            c.levelNumber = 2;
            c.caseId = "LEVEL_02";
            c.caseTitle = "LEVEL 2: The Shattered Mirror";
            c.dateAndLocation = "11:00 PM - Upscale Art Gallery Back Office";
            c.incidentDescription = "Gallery owner Paul Gabriel Camacho claims an intruder broke through the exterior alley window and stole a priceless painting.";
            c.objective = "Interrogate Charl Vonn Pascual and Paul Gabriel Camacho, inspect the physical glass evidence, catch the false testimony, and expose the insurance fraud.";
            c.victimInfo = "Paul Gabriel Camacho (Gallery Owner - Secretive & Dramatic)";
            if (CaseManager.Instance != null && CaseManager.Instance.selectedInvestigator != null)
            {
                c.leadInvestigator = CaseManager.Instance.selectedInvestigator;
            }

            // Primary Witness / Suspect: Charl Vonn Pascual (Night Security Guard)
            CharacterProfileSO charl = ScriptableObject.CreateInstance<CharacterProfileSO>();
            charl.characterId = "CHAR_CHARL_PASCUAL";
            charl.fullName = "Charl Vonn Pascual";
            charl.age = 30;
            charl.occupation = "Night Security Guard";
            charl.relationshipToVictim = "Employee of Paul Gabriel Camacho";
            charl.personalityTrait = PersonalityTrait.Calm;
            charl.alibi = "Claims he was standing right outside the office door when he heard glass shatter at 11:00 PM.";
            charl.possibleMotives = "Bribed by gallery owner.";
            c.primarySuspect = charl;

            // Secondary Suspect: Paul Gabriel Camacho (Gallery Owner)
            CharacterProfileSO paul = ScriptableObject.CreateInstance<CharacterProfileSO>();
            paul.characterId = "CHAR_PAUL_CAMACHO";
            paul.fullName = "Paul Gabriel Camacho";
            paul.age = 42;
            paul.occupation = "Art Gallery Owner";
            paul.relationshipToVictim = "Owner / Victim";
            paul.personalityTrait = PersonalityTrait.Secretive;
            paul.alibi = "Claims he was at home when the alarm triggered.";
            paul.possibleMotives = "Insurance payout to save failing gallery.";
            c.additionalSuspects.Add(paul);

            // Register Investigator Profile: Detective Kyle Gabriel Pastrana
            CharacterProfileSO kyle = ScriptableObject.CreateInstance<CharacterProfileSO>();
            kyle.characterId = "CHAR_KYLE_PASTRANA";
            kyle.fullName = "Detective Kyle Gabriel Pastrana";
            kyle.age = 34;
            kyle.occupation = "Lead Field Detective";
            kyle.personalityTrait = PersonalityTrait.Observant;
            kyle.background = "Seasoned field investigator with sharp instincts for physical evidence, crime scene reconstruction, and spotting fabrications.";
            CaseManager.Instance?.RegisterAvailableInvestigator(kyle);

            // Evidence Items
            // 1. Crime Scene Photo (Window Frame)
            EvidenceSO evWindowPhoto = ScriptableObject.CreateInstance<EvidenceSO>();
            evWindowPhoto.id = "EVD_WINDOW_PHOTO";
            evWindowPhoto.evidenceName = "Window Frame Crime Scene Photo";
            evWindowPhoto.category = EvidenceCategory.Photograph;
            evWindowPhoto.baseDescription = "Photograph of the shattered back office window taken from the alley.";
            evWindowPhoto.detailedObservation = "Glass shards are scattered OUTSIDE on the alley pavement, proving the window was broken from the INSIDE.";
            evWindowPhoto.unlockedClueText = "Glass shards scattered outside on pavement prove window was broken from INSIDE!";
            evWindowPhoto.startsDiscovered = true;

            EvidenceHotspot spotGlass = new EvidenceHotspot();
            spotGlass.hotspotId = "SPOT_OUTSIDE_GLASS";
            spotGlass.hotspotTitle = "Outer Glass Distribution";
            spotGlass.normalizedPosition = new Vector2(0.5f, 0.2f);
            spotGlass.observationText = "Glass fragments lay outside on alley cobblestone rather than inside office floor.";
            spotGlass.clueUnlockedId = "CLUE_BROKEN_FROM_INSIDE";
            evWindowPhoto.hotspots.Add(spotGlass);
            c.evidenceItems.Add(evWindowPhoto);

            // 2. Security Guard Shift Log
            EvidenceSO evShiftLog = ScriptableObject.CreateInstance<EvidenceSO>();
            evShiftLog.id = "EVD_SHIFT_LOG";
            evShiftLog.evidenceName = "Security Guard Shift Log";
            evShiftLog.category = EvidenceCategory.Document;
            evShiftLog.baseDescription = "Electronic keycard log showing guard movements throughout the night.";
            evShiftLog.unlockedClueText = "Log shows Charl was checking the East Perimeter gate at 11:00 PM, far away from the office!";
            evShiftLog.startsDiscovered = true;
            c.evidenceItems.Add(evShiftLog);

            // 3. Insurance Policy Document
            EvidenceSO evInsurance = ScriptableObject.CreateInstance<EvidenceSO>();
            evInsurance.id = "EVD_INSURANCE_POLICY";
            evInsurance.evidenceName = "Art Insurance Policy";
            evInsurance.category = EvidenceCategory.Document;
            evInsurance.baseDescription = "Insurance policy agreement for the stolen painting.";
            evInsurance.unlockedClueText = "Paul doubled the insurance payout value of the painting just 48 hours prior to the theft.";
            evInsurance.startsDiscovered = true;
            c.evidenceItems.Add(evInsurance);

            // Dialogue Tree for Charl Vonn Pascual
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_CHARL_01";
            tree.characterId = charl.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Contradictory Guard Statement)
            DialogueNode node1 = new DialogueNode();
            node1.nodeId = "NODE_01";
            node1.speakerId = charl.characterId;
            node1.speakerName = charl.fullName;
            node1.expression = CharacterExpression.Calm;
            node1.statementText = "I was standing right outside the office door when I heard the window shatter from the alley at 11:00 PM.";
            node1.isChallengeable = true;
            node1.targetContradictionRuleId = "RULE_CHARL_LOCATION_LIE";
            tree.nodes.Add(node1);

            // Node 2 (Exposed Guard Confession)
            DialogueNode node2 = new DialogueNode();
            node2.nodeId = "NODE_02_CONFESSION";
            node2.speakerId = charl.characterId;
            node2.speakerName = charl.fullName;
            node2.expression = CharacterExpression.Nervous;
            node2.statementText = "Fine! The shift log doesn't lie... I was at the East Gate. Mr. Paul Camacho paid me 2,000 credits to lie about being outside his door and stage the break-in!";
            node2.isChallengeable = false;
            tree.nodes.Add(node2);

            c.dialogueTrees.Add(tree);

            // Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_CHARL_LOCATION_LIE";
            rule1.ruleTitle = "False Guard Guard Location";
            rule1.targetStatementNodeId = "NODE_01";
            rule1.requiredEvidenceId = "EVD_SHIFT_LOG";
            rule1.reactionExpression = CharacterExpression.Nervous;
            rule1.reactionDialogue = "Charl loses his calm composure: \"The keycard shift log? Ah... I forgot the electronic scanners record timestamps...\"";
            rule1.unlockedDialogueNodeId = "NODE_02_CONFESSION";
            rule1.unlockedClueId = "CLUE_PAUL_STAGED_BURGLARY";
            rule1.unlockedClueText = "Paul Camacho paid Charl to lie and stage the inside window breakage for insurance fraud!";
            c.contradictionRules.Add(rule1);

            // Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_INSURANCE_FRAUD";
            conn1.connectionTitle = "Outside Glass & Doubled Insurance";
            conn1.clueA_Id = "CLUE_BROKEN_FROM_INSIDE";
            conn1.clueB_Id = "EVD_INSURANCE_POLICY_BASE_CLUE";
            conn1.resultClueId = "CLUE_INSURANCE_FRAUD_PROOF";
            conn1.resultClueTitle = "Proof of Staged Burglary Fraud";
            conn1.deductionText = "The window broken from the inside combined with Paul doubling insurance 48h earlier confirms staged fraud!";
            c.clueConnections.Add(conn1);

            // Conclusion Questions
            ConclusionQuestion q1 = new ConclusionQuestion();
            q1.questionId = "Q_SUSPECT";
            q1.questionText = "Who orchestrated the fake burglary?";
            q1.options = new List<string> { "Paul Gabriel Camacho (Gallery Owner)", "Charl Vonn Pascual (Security Guard)", "External Intruder" };
            q1.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion();
            q2.questionId = "Q_MOTIVE";
            q2.questionText = "What was the motive for staging the crime?";
            q2.options = new List<string> { "Insurance Fraud Payout", "Personal Grudge", "Covering Defective Art" };
            q2.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion();
            q3.questionId = "Q_EVIDENCE";
            q3.questionText = "Which evidence proved the window was broken from inside?";
            q3.options = new List<string> { "Window Frame Photo (Glass Outside)", "Shift Log", "Insurance Policy" };
            q3.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q3);

            return c;
        }
    }
}

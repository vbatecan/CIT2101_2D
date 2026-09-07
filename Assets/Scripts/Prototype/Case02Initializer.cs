using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Prototype Level 2 Initializer: Generates the "Shattered Mirror" case data at runtime.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class Case02Initializer : MonoBehaviour
    {
        [Header("Case Data Source (Optional)")]
        [Tooltip("Pre-configured Case ScriptableObject asset (e.g. Case02_Data.asset). If assigned, this asset is loaded directly instead of generating procedural mock data.")]
        [SerializeField] private CaseSO _caseDataAsset;
        public CaseSO CaseDataAsset
        {
            get => _caseDataAsset;
            set => _caseDataAsset = value;
        }

        /// <summary>Whether to automatically initialize and load Case 02 on Start.</summary>
        public bool initializeOnStart = false;

        [Header("Suspect Portraits & Visuals")]
        public Sprite guardSuspectSprite;
        public Sprite ownerSuspectSprite;

        [Header("Evidence Sprites - Table POV")]
        public Sprite windowPhotoTableSprite;
        public Sprite securityLogTableSprite;
        public Sprite insurancePolicyTableSprite;

        [Header("Evidence Sprites - Top POV (Inspect / Zoomed)")]
        public Sprite windowPhotoZoomedSprite;
        public Sprite securityLogZoomedSprite;
        public Sprite insurancePolicyZoomedSprite;

        /// <summary>
        /// Automatically loads Case 02 on start if <see cref="initializeOnStart"/> is enabled.
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                CaseSO case02 = CreateCase02Data();
                CaseManager.Instance?.LoadCase(case02);

                if (InterrogationManager.Instance != null && case02 != null && case02.primarySuspect != null && case02.dialogueTrees != null && case02.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(case02.primarySuspect, case02.dialogueTrees[0]);
                }

                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
            }
        }

        /// <summary>
        /// Creates and populates the runtime <see cref="CaseSO"/> data for Level 2: The Shattered Mirror,
        /// or returns the assigned pre-configured <see cref="CaseSO"/> data asset if available.
        /// </summary>
        /// <returns>A fully configured <see cref="CaseSO"/> ScriptableObject instance.</returns>
        public CaseSO CreateCase02Data()
        {
            if (_caseDataAsset != null)
            {
                if (CaseManager.Instance != null && CaseManager.Instance.selectedInvestigator != null)
                {
                    _caseDataAsset.leadInvestigator = CaseManager.Instance.selectedInvestigator;
                }
                return _caseDataAsset;
            }

            if (CaseManager.Instance != null && CaseManager.Instance.activeCase != null && CaseManager.Instance.activeCase.levelNumber == 2)
            {
                if (CaseManager.Instance.selectedInvestigator != null)
                {
                    CaseManager.Instance.activeCase.leadInvestigator = CaseManager.Instance.selectedInvestigator;
                }
                return CaseManager.Instance.activeCase;
            }

            CaseSO c = ScriptableObject.CreateInstance<CaseSO>();
            c.levelNumber = 2;
            c.caseId = "LEVEL_02";
            c.caseTitle = "LEVEL 2: The Shattered Mirror";
            c.dateAndLocation = "11:00 PM - Upscale Art Gallery Back Office";
            c.incidentDescription = "Gallery owner Paul Gabriel Camacho claims an intruder broke through the exterior alley window and stole a priceless painting.";
            c.objective = "Interrogate Charl Vonn Pascual and Paul Gabriel Camacho, inspect the physical glass evidence, catch the false testimony, and expose the insurance fraud.";
            c.victimInfo = "Paul Gabriel Camacho (Gallery Owner - Secretive & Dramatic)";
            c.totalKeyEvidenceCount = 3;
            c.totalContradictionsCount = 1;
            c.hasTimeLimit = true;
            c.timeLimitSeconds = 300f; // 5 mins default
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
            charl.defaultSittingPose = guardSuspectSprite;
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
            paul.defaultSittingPose = ownerSuspectSprite;
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
            evWindowPhoto.normalSprite = windowPhotoTableSprite;
            evWindowPhoto.zoomedSprite = windowPhotoZoomedSprite;
            evWindowPhoto.topPovSprite = windowPhotoZoomedSprite;
            evWindowPhoto.baseDescription = "Photograph of the shattered back office window taken from the alley.";
            evWindowPhoto.detailedObservation = "Glass shards are scattered OUTSIDE on the alley pavement, proving the window was broken from the INSIDE.";
            evWindowPhoto.unlockedClueText = "Glass shards scattered outside on pavement prove window was broken from INSIDE!";
            evWindowPhoto.startsDiscovered = false;

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
            evShiftLog.normalSprite = securityLogTableSprite;
            evShiftLog.zoomedSprite = securityLogZoomedSprite;
            evShiftLog.topPovSprite = securityLogZoomedSprite;
            evShiftLog.baseDescription = "Electronic keycard log showing guard movements throughout the night.";
            evShiftLog.detailedObservation = "Digital badge log printout highlighting: 11:00 PM - Charl Pascual scanned at East Gate.";
            evShiftLog.unlockedClueText = "Log shows Charl was checking the East Perimeter gate at 11:00 PM, far away from the office!";
            evShiftLog.startsDiscovered = false;
            evShiftLog.requiredDialogueNodeId = "NODE_02_WINDOW_LEAD";
            evShiftLog.dialogueNodeToTriggerOnInspect = "NODE_03_SHIFT_LEAD";
            c.evidenceItems.Add(evShiftLog);

            // 3. Insurance Policy Document
            EvidenceSO evInsurance = ScriptableObject.CreateInstance<EvidenceSO>();
            evInsurance.id = "EVD_INSURANCE_POLICY";
            evInsurance.evidenceName = "Art Insurance Policy";
            evInsurance.category = EvidenceCategory.Document;
            evInsurance.normalSprite = insurancePolicyTableSprite;
            evInsurance.zoomedSprite = insurancePolicyZoomedSprite;
            evInsurance.topPovSprite = insurancePolicyZoomedSprite;
            evInsurance.baseDescription = "Insurance policy agreement for the stolen painting.";
            evInsurance.detailedObservation = "Policy rider with amendment stamped 48 hours before the incident doubling coverage to $500,000.";
            evInsurance.unlockedClueText = "Paul doubled the insurance payout value of the painting just 48 hours prior to the theft.";
            evInsurance.startsDiscovered = false;
            evInsurance.requiredDialogueNodeId = "NODE_03_SHIFT_LEAD";
            c.evidenceItems.Add(evInsurance);

            // Dialogue Tree for Charl Vonn Pascual and Paul Gabriel Camacho
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_CHARL_01";
            tree.characterId = charl.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Opening Guard Statement - Charl)
            DialogueNode node1 = new DialogueNode();
            node1.nodeId = "NODE_01";
            node1.speakerId = charl.characterId;
            node1.speakerName = charl.fullName;
            node1.expression = CharacterExpression.Calm;
            node1.statementText = "I was standing right outside the office door when I heard the window shatter from the alley at 11:00 PM.";
            node1.defaultNextNodeId = "NODE_01B_PAUL_DEFENDS";
            tree.nodes.Add(node1);

            // Node 1B (Gallery Owner Backing - Paul)
            DialogueNode node1b = new DialogueNode();
            node1b.nodeId = "NODE_01B_PAUL_DEFENDS";
            node1b.speakerId = paul.characterId;
            node1b.speakerName = paul.fullName;
            node1b.expression = CharacterExpression.Calm;
            node1b.statementText = "Officer Pascual has guarded my gallery for three years, Detective. A street thief broke through that back alley window to steal my prize painting!";
            node1b.defaultNextNodeId = "NODE_01C_DETECTIVE_INTERVIEW";
            tree.nodes.Add(node1b);

            // Node 1C (Detective Pressing)
            DialogueNode node1c = new DialogueNode();
            node1c.nodeId = "NODE_01C_DETECTIVE_INTERVIEW";
            node1c.speakerName = "Detective";
            node1c.statementText = "Mr. Camacho, you claim you were at home when the alarm rang. Let the guard speak. Charl, describe exactly what you saw around that window.";
            node1c.defaultNextNodeId = "NODE_02_WINDOW_LEAD";
            tree.nodes.Add(node1c);

            // Node 2 (Window Lead - Charl)
            DialogueNode node2 = new DialogueNode();
            node2.nodeId = "NODE_02_WINDOW_LEAD";
            node2.speakerId = charl.characterId;
            node2.speakerName = charl.fullName;
            node2.expression = CharacterExpression.Calm;
            node2.statementText = "The alley window was broken from outside. Check the crime scene photo of the window frame if you doubt me.";
            node2.unlockEvidenceOnComplete.Add("EVD_SHIFT_LOG");
            node2.defaultNextNodeId = "NODE_02B_PAUL_DIVERT";
            tree.nodes.Add(node2);

            // Node 2B (Paul Diverting Attention - Paul)
            DialogueNode node2b = new DialogueNode();
            node2b.nodeId = "NODE_02B_PAUL_DIVERT";
            node2b.speakerId = paul.characterId;
            node2b.speakerName = paul.fullName;
            node2b.expression = CharacterExpression.Defensive;
            node2b.statementText = "Don't waste time harassing my staff, Detective! We should be tracking the black market, not inspecting window glass!";
            node2b.defaultNextNodeId = "NODE_02C_DETECTIVE_PHOTO";
            tree.nodes.Add(node2b);

            // Node 2C (Detective Table Direction)
            DialogueNode node2c = new DialogueNode();
            node2c.nodeId = "NODE_02C_DETECTIVE_PHOTO";
            node2c.speakerName = "Detective";
            node2c.statementText = "We follow the physical evidence. The window frame photo and the guard's keycard shift log will reveal who was present at 11:00 PM.";
            tree.nodes.Add(node2c);

            // Node 3 (Shift Lead - Charl)
            DialogueNode node3 = new DialogueNode();
            node3.nodeId = "NODE_03_SHIFT_LEAD";
            node3.speakerId = charl.characterId;
            node3.speakerName = charl.fullName;
            node3.expression = CharacterExpression.Nervous;
            node3.statementText = "The shift log is routine. It will show I was near the office, exactly as I said.";
            node3.unlockEvidenceOnComplete.Add("EVD_INSURANCE_POLICY");
            node3.defaultNextNodeId = "NODE_03B_CONFIRMATION";
            tree.nodes.Add(node3);

            // Node 3B (Detective Evidence Confrontation)
            DialogueNode node3b = new DialogueNode();
            node3b.nodeId = "NODE_03B_CONFIRMATION";
            node3b.speakerName = "Detective";
            node3b.statementText = "The crime scene photo shows glass shards scattered OUTSIDE into the alley. That window was smashed from INSIDE the gallery. Why does your story place you outside?";
            node3b.defaultNextNodeId = "NODE_03C_PAUL_DEFENSIVE";
            tree.nodes.Add(node3b);

            // Node 3C (Paul Defensive on Motive - Paul)
            DialogueNode node3c = new DialogueNode();
            node3c.nodeId = "NODE_03C_PAUL_DEFENSIVE";
            node3c.speakerId = paul.characterId;
            node3c.speakerName = paul.fullName;
            node3c.expression = CharacterExpression.Defensive;
            node3c.statementText = "Broken from inside?! That's preposterous! What reason would anyone here have to damage my own gallery?!";
            node3c.defaultNextNodeId = "NODE_03D_DETECTIVE_POLICY";
            tree.nodes.Add(node3c);

            // Node 3D (Detective Stamping Policy)
            DialogueNode node3d = new DialogueNode();
            node3d.nodeId = "NODE_03D_DETECTIVE_POLICY";
            node3d.speakerName = "Detective";
            node3d.statementText = "A half-million dollar insurance policy rider stamped two days ago might answer that question, Mr. Camacho. Charl, one final chance: where were you?";
            node3d.defaultNextNodeId = "NODE_04_FINAL_STATEMENT";
            tree.nodes.Add(node3d);

            // Node 4 (Final Guard Statement - Charl)
            DialogueNode node4 = new DialogueNode();
            node4.nodeId = "NODE_04_FINAL_STATEMENT";
            node4.speakerId = charl.characterId;
            node4.speakerName = charl.fullName;
            node4.expression = CharacterExpression.Calm;
            node4.statementText = "I was outside that office at 11:00 PM. The keycard record cannot say otherwise.";
            node4.isChallengeable = true;
            node4.targetContradictionRuleId = "RULE_CHARL_LOCATION_LIE";
            tree.nodes.Add(node4);

            // Node 5 (Exposed Guard Confession - Charl)
            DialogueNode node5 = new DialogueNode();
            node5.nodeId = "NODE_05_CONFESSION";
            node5.speakerId = charl.characterId;
            node5.speakerName = charl.fullName;
            node5.expression = CharacterExpression.Nervous;
            node5.statementText = "Fine! The shift log doesn't lie. I was at the East Gate. Mr. Paul Camacho paid me 2,000 credits to stage the break-in from inside!";
            node5.defaultNextNodeId = "NODE_05B_PAUL_OUTRAGED";
            tree.nodes.Add(node5);

            // Node 5B (Paul Panicking & Furious - Paul)
            DialogueNode node5b = new DialogueNode();
            node5b.nodeId = "NODE_05B_PAUL_OUTRAGED";
            node5b.speakerId = paul.characterId;
            node5b.speakerName = paul.fullName;
            node5b.expression = CharacterExpression.Angry;
            node5b.statementText = "You incompetent fool, Pascual! Shut your mouth! Detective, he's fabricating lies to cover for his own negligence!";
            node5b.defaultNextNodeId = "NODE_05C_CHARL_EVIDENCE";
            tree.nodes.Add(node5b);

            // Node 5C (Charl Countering Paul - Charl)
            DialogueNode node5c = new DialogueNode();
            node5c.nodeId = "NODE_05C_CHARL_EVIDENCE";
            node5c.speakerId = charl.characterId;
            node5c.speakerName = charl.fullName;
            node5c.expression = CharacterExpression.Defensive;
            node5c.statementText = "I kept your encrypted wire transfer receipt, Paul! You told me you needed the 500,000 insurance payout to save the gallery from bankruptcy!";
            node5c.defaultNextNodeId = "NODE_05D_DETECTIVE_CLOSE";
            tree.nodes.Add(node5c);

            // Node 5D (Detective Conclusion)
            DialogueNode node5d = new DialogueNode();
            node5d.nodeId = "NODE_05D_DETECTIVE_CLOSE";
            node5d.speakerName = "Detective";
            node5d.statementText = "Glass on the alley cobblestones, a fraudulent shift alibi, and a doubled insurance policy. Paul Camacho and Charl Pascual, you are both under arrest for conspiracy and insurance fraud.";
            tree.nodes.Add(node5d);

            c.dialogueTrees.Add(tree);

            // Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_CHARL_LOCATION_LIE";
            rule1.ruleTitle = "False Guard Guard Location";
            rule1.targetStatementNodeId = "NODE_04_FINAL_STATEMENT";
            rule1.requiredEvidenceId = "EVD_SHIFT_LOG";
            rule1.reactionExpression = CharacterExpression.Nervous;
            rule1.reactionDialogue = "Charl loses his calm composure: \"The keycard shift log? Ah... I forgot the electronic scanners record timestamps...\"";
            rule1.unlockedDialogueNodeId = "NODE_05_CONFESSION";
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
            q1.questionText = "Who organized the staged burglary at the art gallery?";
            q1.options = new List<string> { "Paul Gabriel Camacho", "Charl Vonn Pascual", "An Unknown Thief" };
            q1.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion();
            q2.questionId = "Q_ACCOMPLICE";
            q2.questionText = "Who was Paul's paid accomplice?";
            q2.options = new List<string> { "Kirby Raymundo", "Charl Vonn Pascual", "Vince Angelo Batecan" };
            q2.correctOptionIndex = 1;
            c.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion();
            q3.questionId = "Q_MOTIVE";
            q3.questionText = "What was Paul's motive for staging the burglary?";
            q3.options = new List<string> { "Revenge against Charl", "To obtain a $500,000 insurance payout", "To steal another painting" };
            q3.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q3);

            ConclusionQuestion q4 = new ConclusionQuestion();
            q4.questionId = "Q_WINDOW";
            q4.questionText = "What did the window crime scene reveal?";
            q4.options = new List<string> { "The window was broken from the inside.", "The window was broken from the outside.", "The window was already damaged before the incident." };
            q4.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q4);

            ConclusionQuestion q5 = new ConclusionQuestion();
            q5.questionId = "Q_INSURANCE";
            q5.questionText = "What suspicious change was made to the painting's insurance policy?";
            q5.options = new List<string> { "The insurance was cancelled.", "Coverage was reduced to $100,000.", "Coverage increased from $250,000 to $500,000." };
            q5.correctOptionIndex = 2;
            c.conclusionQuestions.Add(q5);

            return c;
        }
    }
}

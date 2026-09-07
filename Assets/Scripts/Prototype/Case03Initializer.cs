using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Prototype Level 3 Initializer: Generates the "The Last Call" case data at runtime.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class Case03Initializer : MonoBehaviour
    {
        [Header("Case Data Source (Optional)")]
        [Tooltip("Pre-configured Case ScriptableObject asset. If assigned, this asset is loaded directly instead of generating procedural mock data.")]
        [SerializeField] private CaseSO _caseDataAsset;
        public CaseSO CaseDataAsset
        {
            get => _caseDataAsset;
            set => _caseDataAsset = value;
        }

        /// <summary>Whether to automatically initialize and load Case 03 on Start.</summary>
        public bool initializeOnStart = false;

        [Header("Character Portraits")]
        public Sprite shanaiaSuspectSprite;
        public Sprite shanWitnessSprite;

        [Header("Evidence Sprites - Table POV")]
        public Sprite phoneLogTableSprite;
        public Sprite cctvTableSprite;
        public Sprite terminationTableSprite;

        [Header("Evidence Sprites - Top POV (Inspect / Zoomed)")]
        public Sprite phoneLogZoomedSprite;
        public Sprite cctvZoomedSprite;
        public Sprite terminationZoomedSprite;

        /// <summary>
        /// Automatically loads Case 03 on start if <see cref="initializeOnStart"/> is enabled.
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                CaseSO case03 = CreateCase03Data();
                CaseManager.Instance?.LoadCase(case03);

                if (InterrogationManager.Instance != null && case03 != null && case03.primarySuspect != null && case03.dialogueTrees != null && case03.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(case03.primarySuspect, case03.dialogueTrees[0]);
                }

                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
            }
        }

        /// <summary>
        /// Creates and populates the runtime <see cref="CaseSO"/> data for Level 3: The Last Call,
        /// or returns the assigned pre-configured <see cref="CaseSO"/> data asset if available.
        /// </summary>
        /// <returns>A fully configured <see cref="CaseSO"/> ScriptableObject instance.</returns>
        public CaseSO CreateCase03Data()
        {
            if (_caseDataAsset != null)
            {
                if (CaseManager.Instance != null && CaseManager.Instance.selectedInvestigator != null)
                {
                    _caseDataAsset.leadInvestigator = CaseManager.Instance.selectedInvestigator;
                }
                return _caseDataAsset;
            }

            if (CaseManager.Instance != null && CaseManager.Instance.activeCase != null && CaseManager.Instance.activeCase.levelNumber == 3)
            {
                if (CaseManager.Instance.selectedInvestigator != null)
                {
                    CaseManager.Instance.activeCase.leadInvestigator = CaseManager.Instance.selectedInvestigator;
                }
                return CaseManager.Instance.activeCase;
            }

            CaseSO c = ScriptableObject.CreateInstance<CaseSO>();
            c.levelNumber = 3;
            c.caseId = "LEVEL_03";
            c.caseTitle = "LEVEL 3: The Last Call";
            c.dateAndLocation = "After Hours - Downtown Coffee Shop Office";
            c.incidentDescription = "Tech startup founder Kurt Miguel Ancheta's secret prototype drive went missing from his bag after a late meeting.";
            c.objective = "Interrogate Shanaia Ortega, examine digital logs and CCTV stills, expose her false departure claim, and recover the stolen prototype.";
            c.victimInfo = "Kurt Miguel Ancheta (Startup Founder - Distressed Victim)";
            c.totalKeyEvidenceCount = 3;
            c.totalContradictionsCount = 1;
            c.hasTimeLimit = true;
            c.timeLimitSeconds = 300f;
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
            shanaia.defaultSittingPose = shanaiaSuspectSprite;
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
            shan.defaultSittingPose = shanWitnessSprite;
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
            evPhoneLog.normalSprite = phoneLogTableSprite;
            evPhoneLog.zoomedSprite = phoneLogZoomedSprite;
            evPhoneLog.topPovSprite = phoneLogZoomedSprite;
            evPhoneLog.baseDescription = "Call log extracted from Kurt Miguel Ancheta's phone.";
            evPhoneLog.detailedObservation = "Shows an unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.unlockedClueText = "Unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.startsDiscovered = false;
            c.evidenceItems.Add(evPhoneLog);

            // 2. Coffee Shop CCTV Still
            EvidenceSO evCctv = ScriptableObject.CreateInstance<EvidenceSO>();
            evCctv.id = "EVD_CCTV_STILL";
            evCctv.evidenceName = "Coffee Shop CCTV Frame";
            evCctv.category = EvidenceCategory.Photograph;
            evCctv.normalSprite = cctvTableSprite;
            evCctv.zoomedSprite = cctvZoomedSprite;
            evCctv.topPovSprite = cctvZoomedSprite;
            evCctv.baseDescription = "Security footage capture from the back exit camera.";
            evCctv.detailedObservation = "Clearly shows Shanaia's distinct jacket entering the back exit door at 7:10 PM.";
            evCctv.unlockedClueText = "Shanaia's jacket captured entering cafe back exit at 7:10 PM.";
            evCctv.startsDiscovered = false;
            evCctv.requiredDialogueNodeId = "NODE_02_PHONE_LEAD";
            evCctv.dialogueNodeToTriggerOnInspect = "NODE_03_CCTV_LEAD";

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
            evDraft.normalSprite = terminationTableSprite;
            evDraft.zoomedSprite = terminationZoomedSprite;
            evDraft.topPovSprite = terminationZoomedSprite;
            evDraft.baseDescription = "Drafted letter found inside Kurt's briefcase.";
            evDraft.unlockedClueText = "Kurt planned to fire Shanaia for secretly selling company data to rival firms.";
            evDraft.startsDiscovered = false;
            evDraft.requiredDialogueNodeId = "NODE_03_CCTV_LEAD";
            c.evidenceItems.Add(evDraft);

            // Dialogue Tree for Shanaia Ortega and Shan Jaraba
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_SHANAIA_01";
            tree.characterId = shanaia.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Opening Statement - Shanaia)
            DialogueNode node1 = new DialogueNode();
            node1.nodeId = "NODE_01";
            node1.speakerId = shanaia.characterId;
            node1.speakerName = shanaia.fullName;
            node1.expression = CharacterExpression.Calm;
            node1.statementText = "Once our 5:30 PM meeting wrapped up, I went straight home. I didn't contact Kurt or return to the cafe for the rest of the night.";
            node1.defaultNextNodeId = "NODE_01B_SHAN_STATEMENT";
            tree.nodes.Add(node1);

            // Node 1B (Manager Testimony - Shan)
            DialogueNode node1b = new DialogueNode();
            node1b.nodeId = "NODE_01B_SHAN_STATEMENT";
            node1b.speakerId = shan.characterId;
            node1b.speakerName = shan.fullName;
            node1b.expression = CharacterExpression.Thinking;
            node1b.statementText = "Detective, as cafe manager on duty until 7:30 PM closing, I heard the back exit service chime ring around 7:10 PM. Someone returned through the alley.";
            node1b.defaultNextNodeId = "NODE_01C_SHANAIA_DISMISS";
            tree.nodes.Add(node1b);

            // Node 1C (Shanaia Dismissive - Shanaia)
            DialogueNode node1c = new DialogueNode();
            node1c.nodeId = "NODE_01C_SHANAIA_DISMISS";
            node1c.speakerId = shanaia.characterId;
            node1c.speakerName = shanaia.fullName;
            node1c.expression = CharacterExpression.Defensive;
            node1c.statementText = "Shan, mind your own business! You were balancing the cash register at the front counter. You couldn't see through the back corridor!";
            node1c.defaultNextNodeId = "NODE_01D_DETECTIVE_INTERVIEW";
            tree.nodes.Add(node1c);

            // Node 1D (Detective Interview)
            DialogueNode node1d = new DialogueNode();
            node1d.nodeId = "NODE_01D_DETECTIVE_INTERVIEW";
            node1d.speakerName = "Detective";
            node1d.statementText = "Let Shan finish, Shanaia. Kurt's smartphone call log and the security captures will establish whether anyone returned.";
            node1d.defaultNextNodeId = "NODE_02_PHONE_LEAD";
            tree.nodes.Add(node1d);

            // Node 2 (Phone Lead - Shanaia)
            DialogueNode node2 = new DialogueNode();
            node2.nodeId = "NODE_02_PHONE_LEAD";
            node2.speakerId = shanaia.characterId;
            node2.speakerName = shanaia.fullName;
            node2.expression = CharacterExpression.Calm;
            node2.statementText = "Kurt's phone contains nothing useful. You should focus on the afternoon meeting, not his private calls.";
            node2.unlockEvidenceOnComplete.Add("EVD_CCTV_STILL");
            node2.defaultNextNodeId = "NODE_02B_SHAN_ALARMED";
            tree.nodes.Add(node2);

            // Node 2B (Shan Mentioning Call - Shan)
            DialogueNode node2b = new DialogueNode();
            node2b.nodeId = "NODE_02B_SHAN_ALARMED";
            node2b.speakerId = shan.characterId;
            node2b.speakerName = shan.fullName;
            node2b.expression = CharacterExpression.Nervous;
            node2b.statementText = "Actually, Kurt seemed frantic right after an encrypted incoming call at 7:15 PM. He slammed his office door and locked it.";
            node2b.defaultNextNodeId = "NODE_02C_DETECTIVE_CCTV";
            tree.nodes.Add(node2b);

            // Node 2C (Detective Table Direction)
            DialogueNode node2c = new DialogueNode();
            node2c.nodeId = "NODE_02C_DETECTIVE_CCTV";
            node2c.speakerName = "Detective";
            node2c.statementText = "An encrypted call at 7:15 PM... The security still from the cafe's back exit camera should verify who arrived right before that call.";
            tree.nodes.Add(node2c);

            // Node 3 (CCTV Lead - Shanaia)
            DialogueNode node3 = new DialogueNode();
            node3.nodeId = "NODE_03_CCTV_LEAD";
            node3.speakerId = shanaia.characterId;
            node3.speakerName = shanaia.fullName;
            node3.expression = CharacterExpression.Nervous;
            node3.statementText = "The back exit camera is unreliable. It could not possibly show me there after I left.";
            node3.unlockEvidenceOnComplete.Add("EVD_RESIGNATION_LETTER");
            node3.defaultNextNodeId = "NODE_03B_SHAN_JACKET";
            tree.nodes.Add(node3);

            // Node 3B (Shan Identifying Jacket - Shan)
            DialogueNode node3b = new DialogueNode();
            node3b.nodeId = "NODE_03B_SHAN_JACKET";
            node3b.speakerId = shan.characterId;
            node3b.speakerName = shan.fullName;
            node3b.expression = CharacterExpression.Thinking;
            node3b.statementText = "Shanaia, that embroidered denim jacket in the camera still... you wore that exact jacket to work today. No one else has one.";
            node3b.defaultNextNodeId = "NODE_03C_CONFIRMATION";
            tree.nodes.Add(node3b);

            // Node 3C (Detective Confirmation)
            DialogueNode node3c = new DialogueNode();
            node3c.nodeId = "NODE_03C_CONFIRMATION";
            node3c.speakerName = "Detective";
            node3c.statementText = "The call log records a 10-minute incoming call from your phone at 7:15 PM, and the camera captures your return at 7:10 PM. What were you doing in Kurt's office?";
            node3c.defaultNextNodeId = "NODE_03D_DETECTIVE_DRAFT";
            tree.nodes.Add(node3c);

            // Node 3D (Detective Termination Notice Draft)
            DialogueNode node3d = new DialogueNode();
            node3d.nodeId = "NODE_03D_DETECTIVE_DRAFT";
            node3d.speakerName = "Detective";
            node3d.statementText = "And why did Kurt have a termination notice drafted in his briefcase accusing you of data theft?";
            node3d.defaultNextNodeId = "NODE_04_FINAL_STATEMENT";
            tree.nodes.Add(node3d);

            // Node 4 (Final Statement - Shanaia)
            DialogueNode node4 = new DialogueNode();
            node4.nodeId = "NODE_04_FINAL_STATEMENT";
            node4.speakerId = shanaia.characterId;
            node4.speakerName = shanaia.fullName;
            node4.expression = CharacterExpression.Calm;
            node4.statementText = "Once I left at 5:30 PM, I never returned to that cafe. That is the timeline.";
            node4.isChallengeable = true;
            node4.targetContradictionRuleId = "RULE_SHANAIA_TIMELINE_LIE";
            tree.nodes.Add(node4);

            // Node 5 (Angry / Shocked Confession - Shanaia)
            DialogueNode node5 = new DialogueNode();
            node5.nodeId = "NODE_05_CONFESSION";
            node5.speakerId = shanaia.characterId;
            node5.speakerName = shanaia.fullName;
            node5.expression = CharacterExpression.Angry;
            node5.statementText = "Fine! Kurt discovered I was exporting our proprietary ordering code to a competitor! He drafted that notice to ruin me, so I broke in at 7:10 PM to wipe his drive!";
            node5.defaultNextNodeId = "NODE_05B_SHAN_DISAPPOINTED";
            tree.nodes.Add(node5);

            // Node 5B (Shan Disappointed - Shan)
            DialogueNode node5b = new DialogueNode();
            node5b.nodeId = "NODE_05B_SHAN_DISAPPOINTED";
            node5b.speakerId = shan.characterId;
            node5b.speakerName = shan.fullName;
            node5b.expression = CharacterExpression.Sad;
            node5b.statementText = "Shanaia... Kurt gave you your start in tech. How could you steal his life's work for rival money?";
            node5b.defaultNextNodeId = "NODE_05C_SHANAIA_BITTER";
            tree.nodes.Add(node5b);

            // Node 5C (Shanaia Bitter - Shanaia)
            DialogueNode node5c = new DialogueNode();
            node5c.nodeId = "NODE_05C_SHANAIA_BITTER";
            node5c.speakerId = shanaia.characterId;
            node5c.speakerName = shanaia.fullName;
            node5c.expression = CharacterExpression.Angry;
            node5c.statementText = "He took credit for all my software optimizations while paying me barista wages! I took back what was mine!";
            node5c.defaultNextNodeId = "NODE_05D_DETECTIVE_CLOSE";
            tree.nodes.Add(node5c);

            // Node 5D (Detective Conclusion)
            DialogueNode node5d = new DialogueNode();
            node5d.nodeId = "NODE_05D_DETECTIVE_CLOSE";
            node5d.speakerName = "Detective";
            node5d.statementText = "Your digital footprint betrayed your timeline. Shanaia Ortega, you are under arrest for commercial espionage, cyber theft, and unlawful entry.";
            tree.nodes.Add(node5d);

            c.dialogueTrees.Add(tree);

            // Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_SHANAIA_TIMELINE_LIE";
            rule1.ruleTitle = "False Departure Claim";
            rule1.targetStatementNodeId = "NODE_04_FINAL_STATEMENT";
            rule1.requiredEvidenceId = "EVD_CCTV_STILL";
            rule1.reactionExpression = CharacterExpression.Shocked;
            rule1.reactionDialogue = "Shanaia's calm veneer snaps into fury: \"CCTV at the back exit? How did you get access to Shan Jaraba's private feeds?\"";
            rule1.unlockedDialogueNodeId = "NODE_05_CONFESSION";
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
            q2.questionText = "What was Shanaia's motive for stealing the prototype?";
            q2.options = new List<string> { "To get revenge on Kurt", "To avoid termination and gain leverage", "To purchase the cafe" };
            q2.correctOptionIndex = 1;
            c.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion();
            q3.questionId = "Q_RETURN_EVIDENCE";
            q3.questionText = "Which evidence showed that Shanaia returned to the cafe?";
            q3.options = new List<string> { "Termination Notice", "Kurt's Call Log", "Cafe CCTV" };
            q3.correctOptionIndex = 2;
            c.conclusionQuestions.Add(q3);

            ConclusionQuestion q4 = new ConclusionQuestion();
            q4.questionId = "Q_CALL_TIME";
            q4.questionText = "At approximately what time did Shanaia's encrypted call appear in Kurt's call log?";
            q4.options = new List<string> { "5:30 PM", "7:15 PM", "8:00 PM" };
            q4.correctOptionIndex = 1;
            c.conclusionQuestions.Add(q4);

            ConclusionQuestion q5 = new ConclusionQuestion();
            q5.questionId = "Q_TERMINATION_REASON";
            q5.questionText = "Why was Shanaia being terminated?";
            q5.options = new List<string> { "Selling proprietary company data", "Stealing company money", "Repeatedly missing work" };
            q5.correctOptionIndex = 0;
            c.conclusionQuestions.Add(q5);

            return c;
        }
    }
}

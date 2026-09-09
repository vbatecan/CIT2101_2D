using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;

namespace CaseClosed.Editor
{
    public static class Case03AssetGenerator
    {
        private const string FolderPath = "Assets/Data/Case003";

        [InitializeOnLoadMethod]
        private static void AutoGenerateIfMissing()
        {
            if (!File.Exists(Path.Combine(FolderPath, "Case03_Data.asset")))
            {
                GenerateAllCase03Assets();
            }
        }

        [MenuItem("Case Closed/Generate Case 003 ScriptableObjects", false, 22)]
        public static void GenerateAllCase03Assets()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                AssetDatabase.Refresh();
            }

            // 1. Load Sprites
            Sprite shanaiaSprite = LoadSprite("Assets/Assets/CHARACTERS/Shania Ortega.png", "NEWSHANIA_0") ?? LoadSprite("Assets/Assets/CHARACTERS/Shania Ortega.png");
            Sprite shanSprite = LoadSprite("Assets/Assets/CHARACTERS/Shan Jaraba.png", "NEWSHAN_0") ?? LoadSprite("Assets/Assets/CHARACTERS/Shan Jaraba.png");

            Sprite phoneTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/PhonePOV.png", "PhonePOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/PhonePOV.png");
            Sprite phoneZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/PhoneTOP.png", "PhoneTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/PhoneTOP.png");

            Sprite cctvTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/CctvPOV.png", "CctvPOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/CctvPOV.png");
            Sprite cctvZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/CctvTOP.png", "CctvTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/CctvTOP.png");

            Sprite terminationTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/TerminationPOV.png", "TerminationPOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/TerminationPOV.png");
            Sprite terminationZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/TerminationTOP.png", "TerminationTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/TerminationTOP.png");

            // 2. Suspects & Characters
            CharacterProfileSO shanaia = ScriptableObject.CreateInstance<CharacterProfileSO>();
            shanaia.characterId = "CHAR_SHANAIA_ORTEGA";
            shanaia.fullName = "Shanaia Ortega";
            shanaia.age = 27;
            shanaia.occupation = "Lead Software Developer & Partner";
            shanaia.relationshipToVictim = "Business Partner";
            shanaia.personalityTrait = PersonalityTrait.Calm;
            shanaia.alibi = "Claims she went straight home at 5:30 PM and never contacted Kurt or returned to the cafe.";
            shanaia.possibleMotives = "Steal proprietary code before getting fired.";
            shanaia.knownConflicts = "Furious about low compensation and Kurt taking sole credit for her software optimizations.";
            shanaia.defaultSittingPose = shanaiaSprite;
            SaveAsset(shanaia, $"{FolderPath}/Char_ShanaiaOrtega.asset");

            CharacterProfileSO shan = ScriptableObject.CreateInstance<CharacterProfileSO>();
            shan.characterId = "CHAR_SHAN_JARABA";
            shan.fullName = "Shan Jaraba";
            shan.age = 29;
            shan.occupation = "Cafe Manager & Key Informant";
            shan.relationshipToVictim = "Cafe Manager";
            shan.personalityTrait = PersonalityTrait.Secretive;
            shan.alibi = "Working at register until 7:30 PM closing.";
            shan.possibleMotives = "None directly; witnessed back entrance chime and frantic phone call.";
            shan.knownConflicts = "Heard heated argument between Shanaia and Kurt before afternoon meeting.";
            shan.defaultSittingPose = shanSprite;
            SaveAsset(shan, $"{FolderPath}/Char_ShanJaraba.asset");

            CharacterProfileSO miguel = AssetDatabase.LoadAssetAtPath<CharacterProfileSO>("Assets/Data/Case001/Char_MiguelBorja.asset");
            if (miguel == null)
            {
                miguel = ScriptableObject.CreateInstance<CharacterProfileSO>();
                miguel.characterId = "CHAR_MIGUEL_BORJA";
                miguel.fullName = "Detective Miguel Borja";
                miguel.age = 36;
                miguel.occupation = "Lead Digital Forensics Detective";
                miguel.personalityTrait = PersonalityTrait.Methodical;
                miguel.background = "Analytical cyber forensics specialist skilled in digital trails, encrypted logs, and meticulous investigative deduction.";
                SaveAsset(miguel, $"{FolderPath}/Char_MiguelBorja.asset");
            }

            // 3. Evidence Items
            // 3.1 Smartphone Call Log
            EvidenceSO evPhoneLog = ScriptableObject.CreateInstance<EvidenceSO>();
            evPhoneLog.id = "EVD_SMARTPHONE_LOG";
            evPhoneLog.evidenceName = "Victim's Smartphone Call Log";
            evPhoneLog.category = EvidenceCategory.DigitalRecord;
            evPhoneLog.normalSprite = phoneTableSprite;
            evPhoneLog.zoomedSprite = phoneZoomSprite;
            evPhoneLog.topPovSprite = phoneZoomSprite;
            evPhoneLog.baseDescription = "Call log extracted from Kurt Miguel Ancheta's phone.";
            evPhoneLog.detailedObservation = "Shows an unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.unlockedClueText = "Unanswered 10-minute encrypted call received from Shanaia at 7:15 PM!";
            evPhoneLog.startsDiscovered = true;
            SaveAsset(evPhoneLog, $"{FolderPath}/Evidence_SmartphoneLog.asset");

            // 3.2 Coffee Shop CCTV Frame
            EvidenceSO evCctv = ScriptableObject.CreateInstance<EvidenceSO>();
            evCctv.id = "EVD_CCTV_STILL";
            evCctv.evidenceName = "Coffee Shop CCTV Frame";
            evCctv.category = EvidenceCategory.Photograph;
            evCctv.normalSprite = cctvTableSprite;
            evCctv.zoomedSprite = cctvZoomSprite;
            evCctv.topPovSprite = cctvZoomSprite;
            evCctv.baseDescription = "Security footage capture from the back exit camera.";
            evCctv.detailedObservation = "Clearly shows Shanaia's distinct jacket entering the back exit door at 7:10 PM.";
            evCctv.unlockedClueText = "Shanaia's jacket captured entering cafe back exit at 7:10 PM.";
            evCctv.startsDiscovered = false;
            evCctv.requiredDialogueNodeId = "NODE_02_PHONE_LEAD";
            evCctv.dialogueNodeToTriggerOnInspect = "NODE_03_CCTV_LEAD";

            EvidenceHotspot spotJacket = new EvidenceHotspot
            {
                hotspotId = "SPOT_DISTINCT_JACKET",
                hotspotTitle = "Shanaia's Custom Jacket",
                normalizedPosition = new Vector2(0.4f, 0.6f),
                observationText = "Shanaia entering back door of cafe at 7:10 PM, 1.5 hours after claiming she left!",
                clueUnlockedId = "CLUE_SHANAIA_RETURNED"
            };
            evCctv.hotspots.Add(spotJacket);
            SaveAsset(evCctv, $"{FolderPath}/Evidence_CctvStill.asset");

            // 3.3 Termination Notice Draft
            EvidenceSO evDraft = ScriptableObject.CreateInstance<EvidenceSO>();
            evDraft.id = "EVD_RESIGNATION_LETTER";
            evDraft.evidenceName = "Termination Notice Draft";
            evDraft.category = EvidenceCategory.Document;
            evDraft.normalSprite = terminationTableSprite;
            evDraft.zoomedSprite = terminationZoomSprite;
            evDraft.topPovSprite = terminationZoomSprite;
            evDraft.baseDescription = "Drafted letter found inside Kurt's briefcase.";
            evDraft.detailedObservation = "Official termination notice drafted by Kurt citing corporate espionage and unauthorized data exports.";
            evDraft.unlockedClueText = "Kurt planned to fire Shanaia for secretly selling company data to rival firms.";
            evDraft.startsDiscovered = false;
            evDraft.requiredDialogueNodeId = "NODE_03_CCTV_LEAD";
            SaveAsset(evDraft, $"{FolderPath}/Evidence_TerminationNotice.asset");

            // 4. Dialogue Tree
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_SHANAIA_01";
            tree.characterId = shanaia.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Opening Statement - Shanaia)
            DialogueNode node1 = new DialogueNode
            {
                nodeId = "NODE_01",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Calm,
                statementText = "Once our 5:30 PM meeting wrapped up, I went straight home. I didn't contact Kurt or return to the cafe for the rest of the night.",
                defaultNextNodeId = "NODE_01B_SHAN_STATEMENT"
            };
            tree.nodes.Add(node1);

            // Node 1B (Manager Testimony - Shan)
            DialogueNode node1b = new DialogueNode
            {
                nodeId = "NODE_01B_SHAN_STATEMENT",
                speakerId = shan.characterId,
                speakerName = shan.fullName,
                expression = CharacterExpression.Thinking,
                statementText = "Detective, as cafe manager on duty until 7:30 PM closing, I heard the back exit service chime ring around 7:10 PM. Someone returned through the alley.",
                defaultNextNodeId = "NODE_01C_SHANAIA_DISMISS"
            };
            tree.nodes.Add(node1b);

            // Node 1C (Shanaia Dismissive - Shanaia)
            DialogueNode node1c = new DialogueNode
            {
                nodeId = "NODE_01C_SHANAIA_DISMISS",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "Shan, mind your own business! You were balancing the cash register at the front counter. You couldn't see through the back corridor!",
                defaultNextNodeId = "NODE_01D_DETECTIVE_INTERVIEW"
            };
            tree.nodes.Add(node1c);

            // Node 1D (Detective Interview)
            DialogueNode node1d = new DialogueNode
            {
                nodeId = "NODE_01D_DETECTIVE_INTERVIEW",
                speakerName = "Detective",
                statementText = "Let Shan finish, Shanaia. Kurt's smartphone call log and the security captures will establish whether anyone returned.",
                defaultNextNodeId = "NODE_02_PHONE_LEAD"
            };
            tree.nodes.Add(node1d);

            // Node 2 (Phone Lead - Shanaia)
            DialogueNode node2 = new DialogueNode
            {
                nodeId = "NODE_02_PHONE_LEAD",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Calm,
                statementText = "Kurt's phone contains nothing useful. You should focus on the afternoon meeting, not his private calls.",
                defaultNextNodeId = "NODE_02B_SHAN_ALARMED"
            };
            node2.unlockEvidenceOnComplete.Add("EVD_CCTV_STILL");
            tree.nodes.Add(node2);

            // Node 2B (Shan Mentioning Call - Shan)
            DialogueNode node2b = new DialogueNode
            {
                nodeId = "NODE_02B_SHAN_ALARMED",
                speakerId = shan.characterId,
                speakerName = shan.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "Actually, Kurt seemed frantic right after an encrypted incoming call at 7:15 PM. He slammed his office door and locked it.",
                defaultNextNodeId = "NODE_02C_DETECTIVE_CCTV"
            };
            tree.nodes.Add(node2b);

            // Node 2C (Detective Table Direction)
            DialogueNode node2c = new DialogueNode
            {
                nodeId = "NODE_02C_DETECTIVE_CCTV",
                speakerName = "Detective",
                statementText = "An encrypted call at 7:15 PM... The security still from the cafe's back exit camera should verify who arrived right before that call."
            };
            tree.nodes.Add(node2c);

            // Node 3 (CCTV Lead - Shanaia)
            DialogueNode node3 = new DialogueNode
            {
                nodeId = "NODE_03_CCTV_LEAD",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "The back exit camera is unreliable. It could not possibly show me there after I left.",
                defaultNextNodeId = "NODE_03B_SHAN_JACKET"
            };
            node3.unlockEvidenceOnComplete.Add("EVD_RESIGNATION_LETTER");
            tree.nodes.Add(node3);

            // Node 3B (Shan Identifying Jacket - Shan)
            DialogueNode node3b = new DialogueNode
            {
                nodeId = "NODE_03B_SHAN_JACKET",
                speakerId = shan.characterId,
                speakerName = shan.fullName,
                expression = CharacterExpression.Thinking,
                statementText = "Shanaia, that embroidered denim jacket in the camera still... you wore that exact jacket to work today. No one else has one.",
                defaultNextNodeId = "NODE_03C_CONFIRMATION"
            };
            tree.nodes.Add(node3b);

            // Node 3C (Detective Confirmation)
            DialogueNode node3c = new DialogueNode
            {
                nodeId = "NODE_03C_CONFIRMATION",
                speakerName = "Detective",
                statementText = "The call log records a 10-minute incoming call from your phone at 7:15 PM, and the camera captures your return at 7:10 PM. What were you doing in Kurt's office?",
                defaultNextNodeId = "NODE_03D_DETECTIVE_DRAFT"
            };
            tree.nodes.Add(node3c);

            // Node 3D (Detective Termination Notice Draft)
            DialogueNode node3d = new DialogueNode
            {
                nodeId = "NODE_03D_DETECTIVE_DRAFT",
                speakerName = "Detective",
                statementText = "And why did Kurt have a termination notice drafted in his briefcase accusing you of data theft?",
                defaultNextNodeId = "NODE_04_FINAL_STATEMENT"
            };
            tree.nodes.Add(node3d);

            // Node 4 (Final Statement - Shanaia)
            DialogueNode node4 = new DialogueNode
            {
                nodeId = "NODE_04_FINAL_STATEMENT",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Calm,
                statementText = "Once I left at 5:30 PM, I never returned to that cafe. That is the timeline.",
                isChallengeable = true,
                targetContradictionRuleId = "RULE_SHANAIA_TIMELINE_LIE"
            };
            tree.nodes.Add(node4);

            // Node 5 (Confession - Shanaia)
            DialogueNode node5 = new DialogueNode
            {
                nodeId = "NODE_05_CONFESSION",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Angry,
                statementText = "Fine! Kurt discovered I was exporting our proprietary ordering code to a competitor! He drafted that notice to ruin me, so I broke in at 7:10 PM to wipe his drive!",
                defaultNextNodeId = "NODE_05B_SHAN_DISAPPOINTED"
            };
            tree.nodes.Add(node5);

            // Node 5B (Shan Disappointed - Shan)
            DialogueNode node5b = new DialogueNode
            {
                nodeId = "NODE_05B_SHAN_DISAPPOINTED",
                speakerId = shan.characterId,
                speakerName = shan.fullName,
                expression = CharacterExpression.Sad,
                statementText = "Shanaia... Kurt gave you your start in tech. How could you steal his life's work for rival money?",
                defaultNextNodeId = "NODE_05C_SHANAIA_BITTER"
            };
            tree.nodes.Add(node5b);

            // Node 5C (Shanaia Bitter - Shanaia)
            DialogueNode node5c = new DialogueNode
            {
                nodeId = "NODE_05C_SHANAIA_BITTER",
                speakerId = shanaia.characterId,
                speakerName = shanaia.fullName,
                expression = CharacterExpression.Angry,
                statementText = "He took credit for all my software optimizations while paying me barista wages! I took back what was mine!",
                defaultNextNodeId = "NODE_05D_DETECTIVE_CLOSE"
            };
            tree.nodes.Add(node5c);

            // Node 5D (Detective Conclusion)
            DialogueNode node5d = new DialogueNode
            {
                nodeId = "NODE_05D_DETECTIVE_CLOSE",
                speakerName = "Detective",
                statementText = "Your digital footprint betrayed your timeline. Shanaia Ortega, you are under arrest for commercial espionage, cyber theft, and unlawful entry."
            };
            tree.nodes.Add(node5d);

            SaveAsset(tree, $"{FolderPath}/Dialogue_Shanaia01.asset");

            // 5. Contradiction Rule
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
            SaveAsset(rule1, $"{FolderPath}/Rule_ShanaiaTimelineLie.asset");

            // 6. Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_SHANAIA_DIGITAL_TRAIL";
            conn1.connectionTitle = "CCTV Entry & Encrypted Phone Call";
            conn1.clueA_Id = "CLUE_SHANAIA_RETURNED";
            conn1.clueB_Id = "EVD_SMARTPHONE_LOG_BASE_CLUE";
            conn1.resultClueId = "CLUE_PROTOTYPE_THEFT_TIMELINE";
            conn1.resultClueTitle = "Shanaia Was In Office At Theft Time";
            conn1.deductionText = "CCTV footage places Shanaia at the back door at 7:10 PM, right before her 7:15 PM encrypted phone call!";
            SaveAsset(conn1, $"{FolderPath}/Conn_ShanaiaDigitalTrail.asset");

            // 7. CaseSO
            CaseSO case03 = ScriptableObject.CreateInstance<CaseSO>();
            case03.levelNumber = 3;
            case03.caseId = "LEVEL_03";
            case03.caseTitle = "LEVEL 3: The Last Call";
            case03.dateAndLocation = "After Hours - Downtown Coffee Shop Office";
            case03.incidentDescription = "Tech startup founder Kurt Miguel Ancheta's secret prototype drive went missing from his bag after a late meeting.";
            case03.objective = "Interrogate Shanaia Ortega, examine digital logs and CCTV stills, expose her false departure claim, and recover the stolen prototype.";
            case03.victimInfo = "Kurt Miguel Ancheta (Startup Founder - Distressed Victim)";
            case03.leadInvestigator = miguel;
            case03.primarySuspect = shanaia;
            case03.additionalSuspects.Add(shan);
            case03.evidenceItems.Add(evPhoneLog);
            case03.evidenceItems.Add(evCctv);
            case03.evidenceItems.Add(evDraft);
            case03.dialogueTrees.Add(tree);
            case03.contradictionRules.Add(rule1);
            case03.clueConnections.Add(conn1);
            case03.hasTimeLimit = true;
            case03.timeLimitSeconds = 300f;
            case03.totalKeyEvidenceCount = 3;
            case03.totalContradictionsCount = 1;

            ConclusionQuestion q1 = new ConclusionQuestion
            {
                questionId = "Q_SUSPECT",
                questionText = "Who stole Kurt Miguel Ancheta's prototype drive?",
                options = new List<string> { "Shanaia Ortega (Lead Developer)", "Shan Jaraba (Cafe Manager)", "External Hacker" },
                correctOptionIndex = 0,
                pointValue = 200
            };
            case03.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion
            {
                questionId = "Q_MOTIVE",
                questionText = "What was Shanaia's motive for stealing the prototype?",
                options = new List<string> { "To get revenge on Kurt", "To avoid termination and gain leverage", "To purchase the cafe" },
                correctOptionIndex = 1,
                pointValue = 200
            };
            case03.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion
            {
                questionId = "Q_RETURN_EVIDENCE",
                questionText = "Which evidence showed that Shanaia returned to the cafe?",
                options = new List<string> { "Termination Notice", "Kurt's Call Log", "Cafe CCTV" },
                correctOptionIndex = 2,
                pointValue = 200
            };
            case03.conclusionQuestions.Add(q3);

            ConclusionQuestion q4 = new ConclusionQuestion
            {
                questionId = "Q_CALL_TIME",
                questionText = "At approximately what time did Shanaia's encrypted call appear in Kurt's call log?",
                options = new List<string> { "5:30 PM", "7:15 PM", "8:00 PM" },
                correctOptionIndex = 1,
                pointValue = 200
            };
            case03.conclusionQuestions.Add(q4);

            ConclusionQuestion q5 = new ConclusionQuestion
            {
                questionId = "Q_TERMINATION_REASON",
                questionText = "Why was Shanaia being terminated?",
                options = new List<string> { "Selling proprietary company data", "Stealing company money", "Repeatedly missing work" },
                correctOptionIndex = 0,
                pointValue = 200
            };
            case03.conclusionQuestions.Add(q5);

            SaveAsset(case03, $"{FolderPath}/Case03_Data.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Case03AssetGenerator] Case 003 ScriptableObject assets generated successfully in " + FolderPath);
        }

        private static Sprite LoadSprite(string path, string spriteName = null)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null) return sp;
            }

            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite fallback = null;
            foreach (var obj in all)
            {
                if (obj is Sprite s)
                {
                    if (fallback == null) fallback = s;
                    if (!string.IsNullOrEmpty(spriteName) && s.name == spriteName)
                        return s;
                }
            }
            return fallback;
        }

        private static void SaveAsset(Object asset, string path)
        {
            asset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            Object existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;

namespace CaseClosed.Editor
{
    public static class Case02AssetGenerator
    {
        private const string FolderPath = "Assets/Data/Case002";

        [MenuItem("Case Closed/Generate Case 002 ScriptableObjects", false, 21)]
        public static void GenerateAllCase02Assets()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                AssetDatabase.Refresh();
            }

            // 1. Load Sprites
            Sprite charlSprite = LoadSprite("Assets/Assets/CHARACTERS/Vonn Pascual.png", "Vonn_0") ?? LoadSprite("Assets/Assets/CHARACTERS/Vonn.png", "Vonn_0");
            Sprite paulSprite = LoadSprite("Assets/Assets/CHARACTERS/Paul Camacho.png", "Paul_0") ?? LoadSprite("Assets/Assets/CHARACTERS/Paul.png", "Paul_0");

            Sprite windowTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/WindowPOV.png", "WindowPOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/WindowPOV.png");
            Sprite windowZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/WindowTOP.png", "WindowTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/WindowTOP.png");

            Sprite shiftLogTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/SecuritylogPOV.png", "SecuritylogPOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/SecuritylogPOV.png");
            Sprite shiftLogZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/SecuritylogTOP.png", "SecuritylogTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/SecuritylogTOP.png");

            Sprite insuranceTableSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/InsurancePOV.png", "InsurancePOV_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/InsurancePOV.png");
            Sprite insuranceZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/InsuranceTOP.png", "InsuranceTOP_0") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/InsuranceTOP.png");

            // 2. Suspects & Characters
            CharacterProfileSO charl = ScriptableObject.CreateInstance<CharacterProfileSO>();
            charl.characterId = "CHAR_CHARL_PASCUAL";
            charl.fullName = "Charl Vonn Pascual";
            charl.age = 30;
            charl.occupation = "Night Security Guard";
            charl.relationshipToVictim = "Employee of Paul Gabriel Camacho";
            charl.personalityTrait = PersonalityTrait.Calm;
            charl.alibi = "Claims he was standing right outside the office door when he heard glass shatter at 11:00 PM.";
            charl.possibleMotives = "Bribed by gallery owner.";
            charl.defaultSittingPose = charlSprite;
            SaveOrUpdateAsset(charl, $"{FolderPath}/Char_CharlPascual.asset");

            CharacterProfileSO paul = ScriptableObject.CreateInstance<CharacterProfileSO>();
            paul.characterId = "CHAR_PAUL_CAMACHO";
            paul.fullName = "Paul Gabriel Camacho";
            paul.age = 42;
            paul.occupation = "Art Gallery Owner";
            paul.relationshipToVictim = "Owner / Victim";
            paul.personalityTrait = PersonalityTrait.Secretive;
            paul.alibi = "Claims he was at home when the alarm triggered.";
            paul.possibleMotives = "Insurance payout to save failing gallery.";
            paul.defaultSittingPose = paulSprite;
            SaveOrUpdateAsset(paul, $"{FolderPath}/Char_PaulCamacho.asset");

            // 3. Evidence Items
            // 3.1 Crime Scene Photo (Window Frame)
            EvidenceSO evWindowPhoto = ScriptableObject.CreateInstance<EvidenceSO>();
            evWindowPhoto.id = "EVD_WINDOW_PHOTO";
            evWindowPhoto.evidenceName = "Window Frame Crime Scene Photo";
            evWindowPhoto.category = EvidenceCategory.Photograph;
            evWindowPhoto.normalSprite = windowTableSprite;
            evWindowPhoto.zoomedSprite = windowZoomSprite;
            evWindowPhoto.topPovSprite = windowZoomSprite;
            evWindowPhoto.baseDescription = "Photograph of the shattered back office window taken from the alley.";
            evWindowPhoto.detailedObservation = "Glass shards are scattered OUTSIDE on the alley pavement, proving the window was broken from the INSIDE.";
            evWindowPhoto.unlockedClueText = "Glass shards scattered outside on pavement prove window was broken from INSIDE!";
            evWindowPhoto.startsDiscovered = true;

            EvidenceHotspot spotGlass = new EvidenceHotspot
            {
                hotspotId = "SPOT_OUTSIDE_GLASS",
                hotspotTitle = "Outer Glass Distribution",
                normalizedPosition = new Vector2(0.5f, 0.2f),
                radius = 0.1f,
                observationText = "Glass fragments lay outside on alley cobblestone rather than inside office floor.",
                clueUnlockedId = "CLUE_BROKEN_FROM_INSIDE"
            };
            evWindowPhoto.hotspots.Add(spotGlass);
            SaveOrUpdateAsset(evWindowPhoto, $"{FolderPath}/Evidence_WindowPhoto.asset");

            // 3.2 Security Guard Shift Log
            EvidenceSO evShiftLog = ScriptableObject.CreateInstance<EvidenceSO>();
            evShiftLog.id = "EVD_SHIFT_LOG";
            evShiftLog.evidenceName = "Security Guard Shift Log";
            evShiftLog.category = EvidenceCategory.Document;
            evShiftLog.normalSprite = shiftLogTableSprite;
            evShiftLog.zoomedSprite = shiftLogZoomSprite;
            evShiftLog.topPovSprite = shiftLogZoomSprite;
            evShiftLog.baseDescription = "Electronic keycard log showing guard movements throughout the night.";
            evShiftLog.detailedObservation = "Digital badge log printout highlighting: 11:00 PM - Charl Pascual scanned at East Gate.";
            evShiftLog.unlockedClueText = "Log shows Charl was checking the East Perimeter gate at 11:00 PM, far away from the office!";
            evShiftLog.startsDiscovered = false;
            evShiftLog.requiredDialogueNodeId = "NODE_02_WINDOW_LEAD";
            evShiftLog.dialogueNodeToTriggerOnInspect = "NODE_03_SHIFT_LEAD";
            SaveOrUpdateAsset(evShiftLog, $"{FolderPath}/Evidence_SecurityLog.asset");

            // 3.3 Art Insurance Policy
            EvidenceSO evInsurance = ScriptableObject.CreateInstance<EvidenceSO>();
            evInsurance.id = "EVD_INSURANCE_POLICY";
            evInsurance.evidenceName = "Art Insurance Policy";
            evInsurance.category = EvidenceCategory.Document;
            evInsurance.normalSprite = insuranceTableSprite;
            evInsurance.zoomedSprite = insuranceZoomSprite;
            evInsurance.topPovSprite = insuranceZoomSprite;
            evInsurance.baseDescription = "Insurance policy agreement for the stolen painting.";
            evInsurance.detailedObservation = "Policy rider with amendment stamped 48 hours before the incident doubling coverage to $500,000.";
            evInsurance.unlockedClueText = "Paul doubled the insurance payout value of the painting just 48 hours prior to the theft.";
            evInsurance.startsDiscovered = false;
            evInsurance.requiredDialogueNodeId = "NODE_03_SHIFT_LEAD";
            SaveOrUpdateAsset(evInsurance, $"{FolderPath}/Evidence_InsurancePolicy.asset");

            // 4. Dialogue Tree (TREE_CHARL_01)
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_CHARL_01";
            tree.characterId = charl.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Opening Guard Statement - Charl)
            DialogueNode node1 = new DialogueNode
            {
                nodeId = "NODE_01",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Calm,
                statementText = "I was standing right outside the office door when I heard the window shatter from the alley at 11:00 PM.",
                defaultNextNodeId = "NODE_01B_PAUL_DEFENDS",
                isChallengeable = false
            };
            tree.nodes.Add(node1);

            // Node 1B (Gallery Owner Backing - Paul)
            DialogueNode node1b = new DialogueNode
            {
                nodeId = "NODE_01B_PAUL_DEFENDS",
                speakerId = paul.characterId,
                speakerName = paul.fullName,
                expression = CharacterExpression.Calm,
                statementText = "Officer Pascual has guarded my gallery for three years, Detective. A street thief broke through that back alley window to steal my prize painting!",
                defaultNextNodeId = "NODE_01C_DETECTIVE_INTERVIEW",
                isChallengeable = false
            };
            tree.nodes.Add(node1b);

            // Node 1C (Detective Pressing)
            DialogueNode node1c = new DialogueNode
            {
                nodeId = "NODE_01C_DETECTIVE_INTERVIEW",
                speakerName = "Detective",
                statementText = "Mr. Camacho, you claim you were at home when the alarm rang. Let the guard speak. Charl, describe exactly what you saw around that window.",
                defaultNextNodeId = "NODE_02_WINDOW_LEAD",
                isChallengeable = false
            };
            tree.nodes.Add(node1c);

            // Node 2 (Window Lead - Charl)
            DialogueNode node2 = new DialogueNode
            {
                nodeId = "NODE_02_WINDOW_LEAD",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Calm,
                statementText = "The alley window was broken from outside. Check the crime scene photo of the window frame if you doubt me.",
                defaultNextNodeId = "NODE_02B_PAUL_DIVERT",
                isChallengeable = false
            };
            node2.unlockEvidenceOnComplete.Add("EVD_SHIFT_LOG");
            tree.nodes.Add(node2);

            // Node 2B (Paul Diverting Attention - Paul)
            DialogueNode node2b = new DialogueNode
            {
                nodeId = "NODE_02B_PAUL_DIVERT",
                speakerId = paul.characterId,
                speakerName = paul.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "Don't waste time harassing my staff, Detective! We should be tracking the black market, not inspecting window glass!",
                defaultNextNodeId = "NODE_02C_DETECTIVE_PHOTO",
                isChallengeable = false
            };
            tree.nodes.Add(node2b);

            // Node 2C (Detective Table Direction)
            DialogueNode node2c = new DialogueNode
            {
                nodeId = "NODE_02C_DETECTIVE_PHOTO",
                speakerName = "Detective",
                statementText = "We follow the physical evidence. The window frame photo and the guard's keycard shift log will reveal who was present at 11:00 PM.",
                defaultNextNodeId = "",
                isChallengeable = false
            };
            tree.nodes.Add(node2c);

            // Node 3 (Shift Lead - Charl)
            DialogueNode node3 = new DialogueNode
            {
                nodeId = "NODE_03_SHIFT_LEAD",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "The shift log is routine. It will show I was near the office, exactly as I said.",
                defaultNextNodeId = "NODE_03B_CONFIRMATION",
                isChallengeable = false
            };
            node3.unlockEvidenceOnComplete.Add("EVD_INSURANCE_POLICY");
            tree.nodes.Add(node3);

            // Node 3B (Detective Evidence Confrontation)
            DialogueNode node3b = new DialogueNode
            {
                nodeId = "NODE_03B_CONFIRMATION",
                speakerName = "Detective",
                statementText = "The crime scene photo shows glass shards scattered OUTSIDE into the alley. That window was smashed from INSIDE the gallery. Why does your story place you outside?",
                defaultNextNodeId = "NODE_03C_PAUL_DEFENSIVE",
                isChallengeable = false
            };
            tree.nodes.Add(node3b);

            // Node 3C (Paul Defensive on Motive - Paul)
            DialogueNode node3c = new DialogueNode
            {
                nodeId = "NODE_03C_PAUL_DEFENSIVE",
                speakerId = paul.characterId,
                speakerName = paul.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "Broken from inside?! That's preposterous! What reason would anyone here have to damage my own gallery?!",
                defaultNextNodeId = "NODE_03D_DETECTIVE_POLICY",
                isChallengeable = false
            };
            tree.nodes.Add(node3c);

            // Node 3D (Detective Stamping Policy)
            DialogueNode node3d = new DialogueNode
            {
                nodeId = "NODE_03D_DETECTIVE_POLICY",
                speakerName = "Detective",
                statementText = "A half-million dollar insurance policy rider stamped two days ago might answer that question, Mr. Camacho. Charl, one final chance: where were you?",
                defaultNextNodeId = "NODE_04_FINAL_STATEMENT",
                isChallengeable = false
            };
            tree.nodes.Add(node3d);

            // Node 4 (Final Guard Statement - Challengeable)
            DialogueNode node4 = new DialogueNode
            {
                nodeId = "NODE_04_FINAL_STATEMENT",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Calm,
                statementText = "I was outside that office at 11:00 PM. The keycard record cannot say otherwise.",
                defaultNextNodeId = "",
                isChallengeable = true,
                targetContradictionRuleId = "RULE_CHARL_LOCATION_LIE"
            };
            tree.nodes.Add(node4);

            // Node 5 (Exposed Guard Confession - Charl)
            DialogueNode node5 = new DialogueNode
            {
                nodeId = "NODE_05_CONFESSION",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "Fine! The shift log doesn't lie. I was at the East Gate. Mr. Paul Camacho paid me 2,000 credits to stage the break-in from inside!",
                defaultNextNodeId = "NODE_05B_PAUL_OUTRAGED",
                isChallengeable = false
            };
            tree.nodes.Add(node5);

            // Node 5B (Paul Panicking & Furious - Paul)
            DialogueNode node5b = new DialogueNode
            {
                nodeId = "NODE_05B_PAUL_OUTRAGED",
                speakerId = paul.characterId,
                speakerName = paul.fullName,
                expression = CharacterExpression.Angry,
                statementText = "You incompetent fool, Pascual! Shut your mouth! Detective, he's fabricating lies to cover for his own negligence!",
                defaultNextNodeId = "NODE_05C_CHARL_EVIDENCE",
                isChallengeable = false
            };
            tree.nodes.Add(node5b);

            // Node 5C (Charl Countering Paul - Charl)
            DialogueNode node5c = new DialogueNode
            {
                nodeId = "NODE_05C_CHARL_EVIDENCE",
                speakerId = charl.characterId,
                speakerName = charl.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "I kept your encrypted wire transfer receipt, Paul! You told me you needed the 500,000 insurance payout to save the gallery from bankruptcy!",
                defaultNextNodeId = "NODE_05D_DETECTIVE_CLOSE",
                isChallengeable = false
            };
            tree.nodes.Add(node5c);

            // Node 5D (Detective Conclusion)
            DialogueNode node5d = new DialogueNode
            {
                nodeId = "NODE_05D_DETECTIVE_CLOSE",
                speakerName = "Detective",
                statementText = "Glass on the alley cobblestones, a fraudulent shift alibi, and a doubled insurance policy. Paul Camacho and Charl Pascual, you are both under arrest for conspiracy and insurance fraud.",
                defaultNextNodeId = "",
                isChallengeable = false
            };
            tree.nodes.Add(node5d);

            SaveOrUpdateAsset(tree, $"{FolderPath}/Dialogue_Charl01.asset");

            // 5. Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_CHARL_LOCATION_LIE";
            rule1.ruleTitle = "False Guard Location";
            rule1.targetStatementNodeId = "NODE_04_FINAL_STATEMENT";
            rule1.requiredEvidenceId = "EVD_SHIFT_LOG";
            rule1.reactionExpression = CharacterExpression.Nervous;
            rule1.reactionDialogue = "Charl loses his calm composure: \"The keycard shift log? Ah... I forgot the electronic scanners record timestamps...\"";
            rule1.unlockedDialogueNodeId = "NODE_05_CONFESSION";
            rule1.unlockedClueId = "CLUE_PAUL_STAGED_BURGLARY";
            rule1.unlockedClueText = "Paul Camacho paid Charl to lie and stage the inside window breakage for insurance fraud!";
            rule1.scoreBonus = 100;
            SaveOrUpdateAsset(rule1, $"{FolderPath}/Rule_CharlLocationLie.asset");

            // 6. Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_INSURANCE_FRAUD";
            conn1.connectionTitle = "Outside Glass & Doubled Insurance";
            conn1.clueA_Id = "CLUE_BROKEN_FROM_INSIDE";
            conn1.clueB_Id = "EVD_INSURANCE_POLICY_BASE_CLUE";
            conn1.resultClueId = "CLUE_INSURANCE_FRAUD_PROOF";
            conn1.resultClueTitle = "Proof of Staged Burglary Fraud";
            conn1.deductionText = "The window broken from the inside combined with Paul doubling insurance 48h earlier confirms staged fraud!";
            SaveOrUpdateAsset(conn1, $"{FolderPath}/Conn_InsuranceFraud.asset");

            // 7. CaseSO Data
            CaseSO case02 = ScriptableObject.CreateInstance<CaseSO>();
            case02.levelNumber = 2;
            case02.caseId = "LEVEL_02";
            case02.caseTitle = "LEVEL 2: The Shattered Mirror";
            case02.dateAndLocation = "11:00 PM - Upscale Art Gallery Back Office";
            case02.incidentDescription = "Gallery owner Paul Gabriel Camacho claims an intruder broke through the exterior alley window and stole a priceless painting.";
            case02.objective = "Interrogate Charl Vonn Pascual and Paul Gabriel Camacho, inspect the physical glass evidence, catch the false testimony, and expose the insurance fraud.";
            case02.victimInfo = "Paul Gabriel Camacho (Gallery Owner - Secretive & Dramatic)";
            case02.primarySuspect = AssetDatabase.LoadAssetAtPath<CharacterProfileSO>($"{FolderPath}/Char_CharlPascual.asset");
            case02.additionalSuspects.Add(AssetDatabase.LoadAssetAtPath<CharacterProfileSO>($"{FolderPath}/Char_PaulCamacho.asset"));

            case02.evidenceItems.Add(AssetDatabase.LoadAssetAtPath<EvidenceSO>($"{FolderPath}/Evidence_WindowPhoto.asset"));
            case02.evidenceItems.Add(AssetDatabase.LoadAssetAtPath<EvidenceSO>($"{FolderPath}/Evidence_SecurityLog.asset"));
            case02.evidenceItems.Add(AssetDatabase.LoadAssetAtPath<EvidenceSO>($"{FolderPath}/Evidence_InsurancePolicy.asset"));

            case02.dialogueTrees.Add(AssetDatabase.LoadAssetAtPath<DialogueTreeSO>($"{FolderPath}/Dialogue_Charl01.asset"));
            case02.contradictionRules.Add(AssetDatabase.LoadAssetAtPath<ContradictionRuleSO>($"{FolderPath}/Rule_CharlLocationLie.asset"));
            case02.clueConnections.Add(AssetDatabase.LoadAssetAtPath<ClueConnectionSO>($"{FolderPath}/Conn_InsuranceFraud.asset"));

            case02.totalKeyEvidenceCount = 3;
            case02.totalContradictionsCount = 1;
            case02.parCompletionTimeSeconds = 300;
            case02.hasTimeLimit = true;
            case02.timeLimitSeconds = 300f;

            // Conclusion Questions (canonical from docs)
            case02.conclusionQuestions.Add(new ConclusionQuestion
            {
                questionId = "Q_SUSPECT",
                questionText = "Who organized the staged burglary at the art gallery?",
                options = new List<string> { "Paul Gabriel Camacho", "Charl Vonn Pascual", "An Unknown Thief" },
                correctOptionIndex = 0,
                pointValue = 200
            });

            case02.conclusionQuestions.Add(new ConclusionQuestion
            {
                questionId = "Q_ACCOMPLICE",
                questionText = "Who was Paul's paid accomplice?",
                options = new List<string> { "Kirby Raymundo", "Charl Vonn Pascual", "Vince Angelo Batecan" },
                correctOptionIndex = 1,
                pointValue = 200
            });

            case02.conclusionQuestions.Add(new ConclusionQuestion
            {
                questionId = "Q_MOTIVE",
                questionText = "What was Paul's motive for staging the burglary?",
                options = new List<string> { "To obtain a $500,000 insurance payout", "Revenge against Charl", "To steal another painting" },
                correctOptionIndex = 0,
                pointValue = 200
            });

            case02.conclusionQuestions.Add(new ConclusionQuestion
            {
                questionId = "Q_WINDOW",
                questionText = "What did the window crime scene reveal?",
                options = new List<string> { "The window was broken from the inside.", "The window was broken from the outside.", "The window was already damaged before the incident." },
                correctOptionIndex = 0,
                pointValue = 200
            });

            case02.conclusionQuestions.Add(new ConclusionQuestion
            {
                questionId = "Q_INSURANCE",
                questionText = "What suspicious change was made to the painting's insurance policy?",
                options = new List<string> { "The insurance was cancelled.", "Coverage was reduced to $100,000.", "Coverage increased from $250,000 to $500,000." },
                correctOptionIndex = 2,
                pointValue = 200
            });

            SaveOrUpdateAsset(case02, $"{FolderPath}/Case02_Data.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Case02AssetGenerator] Case 002 ScriptableObject assets generated and synchronized successfully in " + FolderPath);
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

        private static void SaveOrUpdateAsset(Object asset, string path)
        {
            string assetName = Path.GetFileNameWithoutExtension(path);
            asset.name = assetName;

            Object existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                existing.name = assetName;
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}

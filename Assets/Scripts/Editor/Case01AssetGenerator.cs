using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;

namespace CaseClosed.Editor
{
    public static class Case01AssetGenerator
    {
        private const string FolderPath = "Assets/Data/Case001";

        [MenuItem("Case Closed/Generate Case 001 ScriptableObjects", false, 20)]
        public static void GenerateAllCase01Assets()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                AssetDatabase.Refresh();
            }

            // 1. Sprites
            Sprite maleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/Case001_Male.png");
            Sprite femaleSprite = LoadSprite("Assets/Assets/CHARACTERS/Jane Reyes.png");
            Sprite photoSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/DoorwayPOV.png", "DoorwayPOV_0");
            Sprite photoZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/DoorwayTOP.png", "DoorwayTOP_0");
            Sprite teacupSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/TeacupPOv.png", "TeacupPOv_2") ?? LoadSprite("Assets/Assets/EVIDENCES/TablePOV/TeacupPOv.png");
            Sprite teacupZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/TeacupTOP.png", "TeacupTOP_1") ?? LoadSprite("Assets/Assets/EVIDENCES/TopPOV/TeacupTOP.png");
            Sprite kitchenSprite = LoadSprite("Assets/Assets/EVIDENCES/TablePOV/KitchenPOV.png", "KitchenPOV_0");
            Sprite kitchenZoomSprite = LoadSprite("Assets/Assets/EVIDENCES/TopPOV/KitchenlogTOP.png", "KitchenlogTOP_0");

            // 2. Suspects & Characters
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
            vince.defaultSittingPose = maleSprite;
            SaveAsset(vince, $"{FolderPath}/Char_VinceBatecan.asset");

            CharacterProfileSO jane = ScriptableObject.CreateInstance<CharacterProfileSO>();
            jane.characterId = "CHAR_CASE1_FEMALE";
            jane.fullName = "Jane Reyes";
            jane.age = 24;
            jane.occupation = "Manor Guest & Key Witness";
            jane.relationshipToVictim = "Family Acquaintance";
            jane.personalityTrait = PersonalityTrait.Observant;
            jane.alibi = "Sat in the dining room talking with guests until 9:00 PM.";
            jane.possibleMotives = "None directly known; witnessed Vince running toward the garden.";
            jane.knownConflicts = "Noticed Vince arguing heatedly with Uncle Kirby before dinner.";
            jane.defaultSittingPose = femaleSprite;
            SaveAsset(jane, $"{FolderPath}/Char_JaneReyes.asset");

            CharacterProfileSO kyle = ScriptableObject.CreateInstance<CharacterProfileSO>();
            kyle.characterId = "CHAR_KYLE_PASTRANA";
            kyle.fullName = "Detective Kyle Gabriel Pastrana";
            kyle.age = 34;
            kyle.occupation = "Lead Field Detective";
            kyle.personalityTrait = PersonalityTrait.Observant;
            kyle.background = "Veteran lead field detective with sharp intuition for physical clues, crime scenes, and catching suspect contradictions.";
            SaveAsset(kyle, $"{FolderPath}/Char_KylePastrana.asset");

            CharacterProfileSO miguel = ScriptableObject.CreateInstance<CharacterProfileSO>();
            miguel.characterId = "CHAR_MIGUEL_BORJA";
            miguel.fullName = "Detective Miguel Borja";
            miguel.age = 36;
            miguel.occupation = "Lead Digital Forensics Detective";
            miguel.personalityTrait = PersonalityTrait.Methodical;
            miguel.background = "Analytical cyber forensics specialist skilled in digital trails, encrypted logs, and meticulous investigative deduction.";
            SaveAsset(miguel, $"{FolderPath}/Char_MiguelBorja.asset");

            // 3. Evidence Items
            EvidenceSO evPhoto = ScriptableObject.CreateInstance<EvidenceSO>();
            evPhoto.id = "EVD_FAMILY_PHOTO";
            evPhoto.evidenceName = "Family Photograph";
            evPhoto.category = EvidenceCategory.Photograph;
            evPhoto.normalSprite = photoSprite;
            evPhoto.zoomedSprite = photoZoomSprite;
            evPhoto.topPovSprite = photoZoomSprite;
            evPhoto.baseDescription = "A photograph taken at 8:45 PM showing the study doorway.";
            evPhoto.detailedObservation = "A distinct silhouette matching Vince is visible standing near the study door.";
            evPhoto.unlockedClueText = "Vince silhouette spotted near study doorway at 8:45 PM.";
            evPhoto.startsDiscovered = false;
            EvidenceHotspot spotDoor = new EvidenceHotspot
            {
                hotspotId = "SPOT_DOORWAY_SILHOUETTE",
                hotspotTitle = "Study Doorway Silhouette",
                normalizedPosition = new Vector2(0.3f, 0.5f),
                observationText = "Silhouette matching Vince standing right outside study room at 8:45 PM.",
                clueUnlockedId = "CLUE_VINCE_AT_DOOR"
            };
            evPhoto.hotspots.Add(spotDoor);
            SaveAsset(evPhoto, $"{FolderPath}/Evidence_FamilyPhoto.asset");

            EvidenceSO evTeacup = ScriptableObject.CreateInstance<EvidenceSO>();
            evTeacup.id = "EVD_BROKEN_TEACUP";
            evTeacup.evidenceName = "Broken Teacup";
            evTeacup.category = EvidenceCategory.PhysicalClue;
            evTeacup.normalSprite = teacupSprite;
            evTeacup.zoomedSprite = teacupZoomSprite;
            evTeacup.topPovSprite = teacupZoomSprite;
            evTeacup.baseDescription = "Found shattered inside the locked study, right near the safe.";
            evTeacup.unlockedClueText = "Teacup shattered directly in front of the safe during break-in.";
            evTeacup.startsDiscovered = false;
            SaveAsset(evTeacup, $"{FolderPath}/Evidence_BrokenTeacup.asset");

            EvidenceSO evKitchen = ScriptableObject.CreateInstance<EvidenceSO>();
            evKitchen.id = "EVD_KITCHEN_LOG";
            evKitchen.evidenceName = "Kitchen Pantry Log";
            evKitchen.category = EvidenceCategory.Document;
            evKitchen.normalSprite = kitchenSprite;
            evKitchen.zoomedSprite = kitchenZoomSprite;
            evKitchen.topPovSprite = kitchenZoomSprite;
            evKitchen.baseDescription = "Logbook entry noting the kitchen pantry was locked by staff from 8:30 PM to 9:15 PM.";
            evKitchen.unlockedClueText = "Kitchen pantry was locked by staff from 8:30 PM to 9:15 PM; Vince could not have been inside!";
            evKitchen.startsDiscovered = false;
            SaveAsset(evKitchen, $"{FolderPath}/Evidence_KitchenLog.asset");

            // 4. Dialogue Tree
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_VINCE_01";
            tree.characterId = vince.characterId;
            tree.startNodeId = "NODE_01";

            // Node 1 (Opening Alibi - Vince)
            DialogueNode node1 = new DialogueNode
            {
                nodeId = "NODE_01",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "I never went near the study! I stayed in the kitchen from 8:30 PM until everyone started shouting!",
                defaultNextNodeId = "NODE_01B_JANE_INTERJECTION"
            };
            tree.nodes.Add(node1);

            // Node 1B (Witness Interjection - Jane)
            DialogueNode node1b = new DialogueNode
            {
                nodeId = "NODE_01B_JANE_INTERJECTION",
                speakerId = jane.characterId,
                speakerName = jane.fullName,
                expression = CharacterExpression.Curious,
                statementText = "Wait, Vince... that isn't true. I was walking past the corridor at 8:40 PM, and I saw you arguing heatedly with Uncle Kirby near the study.",
                defaultNextNodeId = "NODE_01C_VINCE_RETORT"
            };
            tree.nodes.Add(node1b);

            // Node 1C (Defensive Retort - Vince)
            DialogueNode node1c = new DialogueNode
            {
                nodeId = "NODE_01C_VINCE_RETORT",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Angry,
                statementText = "Stay out of this, Jane! You were drinking wine with guests in the dining hall! You couldn't possibly see who was in the corridor!",
                defaultNextNodeId = "NODE_01D_DETECTIVE_PRESS"
            };
            tree.nodes.Add(node1c);

            // Node 1D (Investigator Query - Detective)
            DialogueNode node1d = new DialogueNode
            {
                nodeId = "NODE_01D_DETECTIVE_PRESS",
                speakerName = "Detective",
                statementText = "Both of you, calm down. Vince, Ms. Reyes places you outside the study right before the safe was breached. Explain your presence there.",
                defaultNextNodeId = "NODE_02_ROOM_LEAD"
            };
            tree.nodes.Add(node1d);

            // Node 2 (First Evidence Lead - Vince)
            DialogueNode node2 = new DialogueNode
            {
                nodeId = "NODE_02_ROOM_LEAD",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "The study was locked, Detective! Even if I walked through the hallway, there is nothing in that room connecting me to the necklace.",
                defaultNextNodeId = "NODE_02B_JANE_HEARD_CRASH"
            };
            node2.unlockEvidenceOnComplete.Add("EVD_BROKEN_TEACUP");
            tree.nodes.Add(node2);

            // Node 2B (Crash Observation - Jane)
            DialogueNode node2b = new DialogueNode
            {
                nodeId = "NODE_02B_JANE_HEARD_CRASH",
                speakerId = jane.characterId,
                speakerName = jane.fullName,
                expression = CharacterExpression.Thinking,
                statementText = "Actually, Detective... while we were in the dining hall, everyone heard a loud porcelain crash from inside the study at 8:45 PM.",
                defaultNextNodeId = "NODE_02C_DETECTIVE_INSPECT"
            };
            tree.nodes.Add(node2b);

            // Node 2C (Detective Table Direction)
            DialogueNode node2c = new DialogueNode
            {
                nodeId = "NODE_02C_DETECTIVE_INSPECT",
                speakerName = "Detective",
                statementText = "A porcelain crash inside a locked study... We should examine that broken teacup on the table."
            };
            tree.nodes.Add(node2c);

            // Node 3 (Teacup Lead - Vince)
            DialogueNode node3 = new DialogueNode
            {
                nodeId = "NODE_03_TEACUP_LEAD",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "That broken cup? Alright, so I heard it fall too! But I was nowhere near the safe! The pantry log will prove I was in the kitchen.",
                defaultNextNodeId = "NODE_03B_JANE_KITCHEN_LOCK"
            };
            node3.unlockEvidenceOnComplete.Add("EVD_KITCHEN_LOG");
            tree.nodes.Add(node3);

            // Node 3B (Kitchen Lock Observation - Jane)
            DialogueNode node3b = new DialogueNode
            {
                nodeId = "NODE_03B_JANE_KITCHEN_LOCK",
                speakerId = jane.characterId,
                speakerName = jane.fullName,
                expression = CharacterExpression.Thinking,
                statementText = "Vince, don't you remember? The butler locked the kitchen pantry before 8:30 PM to prepare for the late tea service.",
                defaultNextNodeId = "NODE_03C_DETECTIVE_CHALLENGE"
            };
            tree.nodes.Add(node3b);

            // Node 3C (Detective Warning - Detective)
            DialogueNode node3c = new DialogueNode
            {
                nodeId = "NODE_03C_DETECTIVE_CHALLENGE",
                speakerName = "Detective",
                statementText = "The kitchen log is on the desk, Vince. If you were truly locked in that pantry, the staff records will verify it. Are you certain that is your story?",
                defaultNextNodeId = "NODE_04_FINAL_ALIBI"
            };
            tree.nodes.Add(node3c);

            // Node 4 (Contradictory Alibi - Vince)
            DialogueNode node4 = new DialogueNode
            {
                nodeId = "NODE_04_FINAL_ALIBI",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "I stayed in that kitchen the entire time! Inspect the pantry log yourself—you cannot prove otherwise!",
                isChallengeable = true,
                targetContradictionRuleId = "RULE_VINCE_ALIBI_LIE"
            };
            tree.nodes.Add(node4);

            // Node 5 (Confession - Vince)
            DialogueNode node5 = new DialogueNode
            {
                nodeId = "NODE_05_CONFESSION",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "W-what?! The kitchen pantry was locked by staff from 8:30 to 9:15 PM?! Fine! I needed money to clear my gambling debts, so I took the necklace!",
                defaultNextNodeId = "NODE_05B_JANE_SHOCKED"
            };
            tree.nodes.Add(node5);

            // Node 5B (Jane Shocked)
            DialogueNode node5b = new DialogueNode
            {
                nodeId = "NODE_05B_JANE_SHOCKED",
                speakerId = jane.characterId,
                speakerName = jane.fullName,
                expression = CharacterExpression.Sad,
                statementText = "Vince... Uncle Kirby took you in and paid your tuition! How could you betray our family like this?!",
                defaultNextNodeId = "NODE_05C_VINCE_DESPERATE"
            };
            tree.nodes.Add(node5b);

            // Node 5C (Vince Desperate)
            DialogueNode node5c = new DialogueNode
            {
                nodeId = "NODE_05C_VINCE_DESPERATE",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "He was cutting off my allowance, Jane! The loan sharks gave me until midnight! I panicked and grabbed the necklace from the safe!",
                defaultNextNodeId = "NODE_05D_DETECTIVE_CLOSE"
            };
            tree.nodes.Add(node5c);

            // Node 5D (Detective Conclusion)
            DialogueNode node5d = new DialogueNode
            {
                nodeId = "NODE_05D_DETECTIVE_CLOSE",
                speakerName = "Detective",
                statementText = "That's enough. You broke the teacup during the theft, and the pantry log shattered your false alibi. Vince Angelo Batecan, you are under arrest."
            };
            tree.nodes.Add(node5d);
            SaveAsset(tree, $"{FolderPath}/Dialogue_Vince01.asset");

            // 5. Contradiction Rule
            ContradictionRuleSO rule1 = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule1.ruleId = "RULE_VINCE_ALIBI_LIE";
            rule1.ruleTitle = "Locked Kitchen Contradiction";
            rule1.targetStatementNodeId = "NODE_04_FINAL_ALIBI";
            rule1.requiredEvidenceId = "EVD_KITCHEN_LOG";
            rule1.reactionExpression = CharacterExpression.Nervous;
            rule1.reactionDialogue = "Vince shifts nervously and stammers: \"Wait... the kitchen log shows it was locked by staff? I... I...\"";
            rule1.unlockedDialogueNodeId = "NODE_05_CONFESSION";
            rule1.unlockedClueId = "CLUE_VINCE_STAGED_BREAKIN";
            rule1.unlockedClueText = "Vince Angelo Batecan confessed to staging the break-in for debt money!";
            SaveAsset(rule1, $"{FolderPath}/Rule_VinceAlibiLie.asset");

            // 6. Clue Connection
            ClueConnectionSO conn1 = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn1.connectionId = "CONN_VINCE_TIMELINE";
            conn1.connectionTitle = "Photo Silhouette & Kitchen Log";
            conn1.clueA_Id = "CLUE_VINCE_AT_DOOR";
            conn1.clueB_Id = "EVD_KITCHEN_LOG_BASE_CLUE";
            conn1.resultClueId = "CLUE_VINCE_OUTSIDE_STUDY";
            conn1.resultClueTitle = "Vince Was Outside Study At 8:45 PM";
            conn1.deductionText = "The kitchen log proves Vince was not in the kitchen, while the photo places him right outside the study door!";
            SaveAsset(conn1, $"{FolderPath}/Conn_VinceTimeline.asset");

            // 7. CaseSO
            CaseSO case01 = ScriptableObject.CreateInstance<CaseSO>();
            case01.levelNumber = 1;
            case01.caseId = "LEVEL_01";
            case01.caseTitle = "LEVEL 1: The Missing Necklace";
            case01.dateAndLocation = "Stormy Evening - High-Society Manor Study";
            case01.incidentDescription = "A valuable family necklace disappeared from the manor safe during a stormy evening gathering.";
            case01.objective = "Interrogate Vince Angelo Batecan, inspect table evidence, disprove his kitchen alibi, and uncover the truth.";
            case01.victimInfo = "Kirby Raymundo (Aristocrat - Proud & Demanding Owner)";
            case01.leadInvestigator = kyle;
            case01.primarySuspect = vince;
            case01.additionalSuspects.Add(jane);
            case01.evidenceItems.Add(evPhoto);
            case01.evidenceItems.Add(evTeacup);
            case01.evidenceItems.Add(evKitchen);
            case01.dialogueTrees.Add(tree);
            case01.contradictionRules.Add(rule1);
            case01.clueConnections.Add(conn1);
            case01.hasTimeLimit = true;
            case01.timeLimitSeconds = 300f;

            ConclusionQuestion q1 = new ConclusionQuestion
            {
                questionId = "Q_SUSPECT",
                questionText = "Who stole Kirby Raymundo's necklace?",
                options = new List<string> { "Vince Angelo Batecan", "Kirby Raymundo", "House Staff" },
                correctOptionIndex = 0
            };
            case01.conclusionQuestions.Add(q1);

            ConclusionQuestion q2 = new ConclusionQuestion
            {
                questionId = "Q_MOTIVE",
                questionText = "What was Vince's motive?",
                options = new List<string> { "Pay Off Debts", "Jealousy", "Accidental Spillage" },
                correctOptionIndex = 0
            };
            case01.conclusionQuestions.Add(q2);

            ConclusionQuestion q3 = new ConclusionQuestion
            {
                questionId = "Q_EVIDENCE",
                questionText = "Which evidence disproved Vince's kitchen alibi?",
                options = new List<string> { "Kitchen Pantry Log", "Broken Teacup", "Family Photograph" },
                correctOptionIndex = 0
            };
            case01.conclusionQuestions.Add(q3);

            SaveAsset(case01, $"{FolderPath}/Case01_Data.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Case01AssetGenerator] Case 001 ScriptableObject assets generated successfully in " + FolderPath);
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
            Object existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}

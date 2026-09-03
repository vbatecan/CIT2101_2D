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
            Sprite femaleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/Case001_Female.png");
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

            CharacterProfileSO janine = ScriptableObject.CreateInstance<CharacterProfileSO>();
            janine.characterId = "CHAR_CASE1_FEMALE";
            janine.fullName = "Janine Marie Sotto";
            janine.age = 24;
            janine.occupation = "Manor Guest & Key Witness";
            janine.relationshipToVictim = "Family Acquaintance";
            janine.personalityTrait = PersonalityTrait.Observant;
            janine.alibi = "Sat in the dining room talking with guests until 9:00 PM.";
            janine.possibleMotives = "None directly known; witnessed Vince running toward the garden.";
            janine.knownConflicts = "Noticed Vince arguing heatedly with Uncle Kirby before dinner.";
            janine.defaultSittingPose = femaleSprite;
            SaveAsset(janine, $"{FolderPath}/Char_JanineSotto.asset");

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
            evPhoto.baseDescription = "A photograph taken at 8:45 PM showing the study doorway.";
            evPhoto.detailedObservation = "A distinct silhouette matching Vince is visible standing near the study door.";
            evPhoto.unlockedClueText = "Vince silhouette spotted near study doorway at 8:45 PM.";
            evPhoto.startsDiscovered = true;
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
            evTeacup.baseDescription = "Found shattered inside the locked study, right near the safe.";
            evTeacup.unlockedClueText = "Teacup shattered directly in front of the safe during break-in.";
            evTeacup.startsDiscovered = true;
            SaveAsset(evTeacup, $"{FolderPath}/Evidence_BrokenTeacup.asset");

            EvidenceSO evKitchen = ScriptableObject.CreateInstance<EvidenceSO>();
            evKitchen.id = "EVD_KITCHEN_LOG";
            evKitchen.evidenceName = "Kitchen Pantry Log";
            evKitchen.category = EvidenceCategory.Document;
            evKitchen.normalSprite = kitchenSprite;
            evKitchen.zoomedSprite = kitchenZoomSprite;
            evKitchen.baseDescription = "Logbook entry noting the kitchen pantry was locked by staff from 8:30 PM to 9:15 PM.";
            evKitchen.unlockedClueText = "Kitchen pantry was locked by staff from 8:30 PM to 9:15 PM; Vince could not have been inside!";
            evKitchen.startsDiscovered = true;
            SaveAsset(evKitchen, $"{FolderPath}/Evidence_KitchenLog.asset");

            // 4. Dialogue Tree
            DialogueTreeSO tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.treeId = "TREE_VINCE_01";
            tree.characterId = vince.characterId;
            tree.startNodeId = "NODE_01";

            DialogueNode node1 = new DialogueNode
            {
                nodeId = "NODE_01",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Defensive,
                statementText = "I never went near the study! I stayed in the kitchen from 8:30 PM until everyone started shouting!",
                isChallengeable = true,
                targetContradictionRuleId = "RULE_VINCE_ALIBI_LIE"
            };
            tree.nodes.Add(node1);

            DialogueNode node2 = new DialogueNode
            {
                nodeId = "NODE_02_CONFESSION",
                speakerId = vince.characterId,
                speakerName = vince.fullName,
                expression = CharacterExpression.Nervous,
                statementText = "W-what?! The kitchen pantry log? Fine! The kitchen was locked... I needed money to clear my debts, so I took the necklace!",
                isChallengeable = false
            };
            tree.nodes.Add(node2);
            SaveAsset(tree, $"{FolderPath}/Dialogue_Vince01.asset");

            // 5. Contradiction Rule
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
            case01.additionalSuspects.Add(janine);
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

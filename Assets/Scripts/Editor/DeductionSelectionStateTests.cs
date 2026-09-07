using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    public class DeductionSelectionStateTests
    {
        [Test]
        public void Select_TracksFirstCluePairAndClearAsExplicitStateTransitions()
        {
            DeductionSelectionState state = new DeductionSelectionState();

            Assert.AreEqual(DeductionSelectionState.Transition.Ignored, state.Select(null));
            Assert.AreEqual(DeductionSelectionState.Transition.FirstClueSelected, state.Select("CLUE_A"));
            Assert.AreEqual("CLUE_A", state.FirstSelectedClueId);
            Assert.IsNull(state.SecondSelectedClueId);

            Assert.AreEqual(DeductionSelectionState.Transition.PairReady, state.Select("CLUE_B"));
            Assert.AreEqual("CLUE_A", state.FirstSelectedClueId);
            Assert.AreEqual("CLUE_B", state.SecondSelectedClueId);

            state.Clear();

            Assert.IsNull(state.FirstSelectedClueId);
            Assert.IsNull(state.SecondSelectedClueId);
        }

        [Test]
        public void Select_DeselectsFirstClueWithoutChangingLegacyPendingSecondClue()
        {
            DeductionSelectionState state = new DeductionSelectionState();
            state.Select("CLUE_A");
            state.Select("CLUE_B");

            Assert.AreEqual(DeductionSelectionState.Transition.FirstClueDeselected, state.Select("CLUE_A"));
            Assert.IsNull(state.FirstSelectedClueId);
            Assert.AreEqual("CLUE_B", state.SecondSelectedClueId);
        }

        [Test]
        public void Controller_ValidPairPublishesResultBeforeSelectionIsCleared()
        {
            GameObject root = new GameObject("Test_DeductionBoardController");
            CaseSO caseData = ScriptableObject.CreateInstance<CaseSO>();
            ClueConnectionSO connection = ScriptableObject.CreateInstance<ClueConnectionSO>();
            List<string> events = new List<string>();

            try
            {
                connection.connectionId = "CONNECTION";
                connection.clueA_Id = "CLUE_A";
                connection.clueB_Id = "CLUE_B";
                connection.resultClueId = "RESULT";
                connection.resultClueTitle = "Result";
                connection.deductionText = "The clues connect.";
                caseData.clueConnections.Add(connection);

                CaseManager caseManager = root.AddComponent<CaseManager>();
                DeductionBoardController controller = root.AddComponent<DeductionBoardController>();
                caseManager.LoadCase(caseData);

                controller.OnClueSelectedForConnection += clueId => events.Add($"selection:{clueId}");
                controller.OnConnectionResult += (success, rule) =>
                    events.Add($"result:{success}:{controller.FirstSelectedClueId}:{controller.SecondSelectedClueId}");

                controller.SelectClue("CLUE_A");
                controller.SelectClue("CLUE_B");

                CollectionAssert.AreEqual(
                    new[]
                    {
                        "selection:CLUE_A",
                        "result:True:CLUE_A:CLUE_B",
                        "selection:"
                    },
                    events);
                CollectionAssert.Contains(caseManager.UnlockedClueIds, "RESULT");
                Assert.IsNull(controller.FirstSelectedClueId);
                Assert.IsNull(controller.SecondSelectedClueId);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(connection);
                Object.DestroyImmediate(caseData);
            }
        }
    }
}

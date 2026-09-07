using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    public class CaseSessionStateTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _createdObjects.Count - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(_createdObjects[index]);
            }
            _createdObjects.Clear();
        }

        [Test]
        public void SessionTransitions_DoNotMutateAuthoredScriptableObjects()
        {
            CaseSO authoredCase = Create<CaseSO>();
            CharacterProfileSO authoredInvestigator = Create<CharacterProfileSO>();
            CharacterProfileSO selectedInvestigator = Create<CharacterProfileSO>();
            EvidenceSO evidence = Create<EvidenceSO>();
            EvidenceHotspot hotspot = new EvidenceHotspot { hotspotId = "HOTSPOT", isDiscovered = true };
            ContradictionRuleSO rule = Create<ContradictionRuleSO>();

            authoredCase.leadInvestigator = authoredInvestigator;
            evidence.id = "EVIDENCE";
            evidence.isExamined = false;
            evidence.isToggledOnTable = false;
            evidence.hotspots.Add(hotspot);
            rule.ruleId = "RULE";

            CaseSessionState session = new CaseSessionState();
            session.Begin(authoredCase, selectedInvestigator, 10f);

            Assert.IsTrue(session.TryDiscoverEvidence(evidence));
            Assert.IsTrue(session.TryMarkEvidenceExamined(evidence));
            Assert.IsTrue(session.TryDiscoverHotspot(evidence, hotspot));
            Assert.IsTrue(session.TryUnlockClue("CLUE", "Observed detail"));
            Assert.IsTrue(session.TryExposeContradiction(rule));

            Assert.AreSame(authoredInvestigator, authoredCase.leadInvestigator);
            Assert.IsFalse(evidence.isExamined);
            Assert.IsFalse(evidence.isToggledOnTable);
            Assert.IsTrue(hotspot.isDiscovered);
            Assert.AreSame(selectedInvestigator, session.EffectiveInvestigator);
        }

        [Test]
        public void DuplicateEvidenceClueAndHotspotTransitions_AreIdempotent()
        {
            CaseSessionState session = new CaseSessionState();
            EvidenceSO evidence = Create<EvidenceSO>();
            EvidenceHotspot hotspot = new EvidenceHotspot { hotspotId = "HOTSPOT" };

            evidence.id = "EVIDENCE";
            evidence.hotspots.Add(hotspot);
            session.Begin(Create<CaseSO>(), null, 0f);

            Assert.IsTrue(session.TryDiscoverEvidence(evidence));
            Assert.IsFalse(session.TryDiscoverEvidence(evidence));
            Assert.IsTrue(session.TryDiscoverHotspot(evidence, hotspot));
            Assert.IsFalse(session.TryDiscoverHotspot(evidence, hotspot));
            Assert.IsTrue(session.TryUnlockClue("CLUE", "First text"));
            Assert.IsFalse(session.TryUnlockClue("CLUE", "Changed text"));

            Assert.AreEqual(1, session.DiscoveredEvidenceCount);
            Assert.AreEqual("First text", session.UnlockedClues["CLUE"]);
        }

        [Test]
        public void BeginningAnotherCase_ClearsPreviousSessionState()
        {
            CaseSessionState session = new CaseSessionState();
            EvidenceSO evidence = Create<EvidenceSO>();
            EvidenceHotspot hotspot = new EvidenceHotspot { hotspotId = "HOTSPOT" };
            ContradictionRuleSO rule = Create<ContradictionRuleSO>();

            evidence.id = "EVIDENCE";
            rule.ruleId = "RULE";
            session.Begin(Create<CaseSO>(), null, 0f);
            session.TryDiscoverEvidence(evidence);
            session.TryMarkEvidenceExamined(evidence);
            session.TryDiscoverHotspot(evidence, hotspot);
            session.TryUnlockClue("CLUE", "Detail");
            session.TryExposeContradiction(rule);

            CaseSO nextCase = Create<CaseSO>();
            session.Begin(nextCase, null, 25f);

            Assert.AreSame(nextCase, session.ActiveCase);
            Assert.AreEqual(0, session.DiscoveredEvidenceCount);
            Assert.AreEqual(0, session.ExaminedEvidenceIds.Count);
            Assert.AreEqual(0, session.UnlockedClueIds.Count);
            Assert.AreEqual(0, session.ExposedContradictionCount);
            Assert.IsFalse(session.IsHotspotDiscovered(evidence, hotspot));
        }

        [Test]
        public void TimerLifecycle_AccumulatesOnlyWhileRunningAndExpiresOnce()
        {
            CaseSO timedCase = Create<CaseSO>();
            timedCase.hasTimeLimit = true;
            timedCase.timeLimitSeconds = 120f;

            CaseSessionState session = new CaseSessionState();
            session.Begin(timedCase, null, 10f);
            Assert.IsTrue(session.IsTimerRunning);
            Assert.AreEqual(15f, session.GetElapsedTime(25f));

            session.PauseTimer(30f);
            Assert.IsFalse(session.IsTimerRunning);
            Assert.AreEqual(20f, session.GetElapsedTime(50f));

            Assert.IsTrue(session.ResumeTimer(50f));
            Assert.AreEqual(30f, session.GetElapsedTime(60f));
            Assert.IsTrue(session.ExpireTimer());
            Assert.IsFalse(session.ExpireTimer());
            Assert.IsFalse(session.IsTimerRunning);
            Assert.IsTrue(session.HasTimeExpired);
            Assert.AreEqual(120f, session.GetElapsedTime(90f));
        }

        private T Create<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(asset);
            return asset;
        }
    }
}

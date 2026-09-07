using System;
using System.Collections.Generic;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Owns all mutable state for one loaded case. Authoring ScriptableObjects are read only;
    /// callers must use the named operations below for every case-session transition.
    /// </summary>
    public sealed class CaseSessionState
    {
        private readonly HashSet<string> _discoveredEvidenceIds = new HashSet<string>();
        private readonly HashSet<string> _examinedEvidenceIds = new HashSet<string>();
        private readonly HashSet<string> _tableEvidenceIds = new HashSet<string>();
        private readonly HashSet<string> _discoveredHotspotKeys = new HashSet<string>();
        private readonly HashSet<string> _unlockedClueIds = new HashSet<string>();
        private readonly Dictionary<string, string> _unlockedClues = new Dictionary<string, string>();
        private readonly HashSet<string> _exposedContradictionIds = new HashSet<string>();

        private float _accumulatedElapsedTime;
        private float _lastResumeTimestamp;

        /// <summary>Authored case data currently being played.</summary>
        public CaseSO ActiveCase { get; private set; }

        /// <summary>Player-selected investigator for this session, if one was selected.</summary>
        public CharacterProfileSO SelectedInvestigator { get; private set; }

        /// <summary>Investigator that should be presented to the player for this session.</summary>
        public CharacterProfileSO EffectiveInvestigator => SelectedInvestigator ?? ActiveCase?.leadInvestigator;

        public IReadOnlyCollection<string> DiscoveredEvidenceIds => _discoveredEvidenceIds;
        public IReadOnlyCollection<string> ExaminedEvidenceIds => _examinedEvidenceIds;
        public IReadOnlyCollection<string> TableEvidenceIds => _tableEvidenceIds;
        public IReadOnlyCollection<string> UnlockedClueIds => _unlockedClueIds;
        public IReadOnlyDictionary<string, string> UnlockedClues => _unlockedClues;
        public IReadOnlyCollection<string> ExposedContradictionIds => _exposedContradictionIds;

        public int DiscoveredEvidenceCount => _discoveredEvidenceIds.Count;
        public int ExposedContradictionCount => _exposedContradictionIds.Count;

        public float InvestigationStartedAt { get; private set; }
        public bool IsTimerRunning { get; private set; }
        public bool HasTimeExpired { get; private set; }

        public bool HasActiveTimeLimit =>
            ActiveCase != null && ActiveCase.hasTimeLimit && ActiveCase.timeLimitSeconds > 0f;

        public float CaseTimeLimit => ActiveCase != null ? ActiveCase.timeLimitSeconds : 0f;

        /// <summary>
        /// Starts a new isolated session. The supplied time is infrastructure input so this
        /// type remains independent of Unity's clock and can be tested without a scene.
        /// </summary>
        public void Begin(CaseSO caseData, CharacterProfileSO investigator, float currentTime)
        {
            ActiveCase = caseData;
            SelectedInvestigator = investigator;
            _discoveredEvidenceIds.Clear();
            _examinedEvidenceIds.Clear();
            _tableEvidenceIds.Clear();
            _discoveredHotspotKeys.Clear();
            _unlockedClueIds.Clear();
            _unlockedClues.Clear();
            _exposedContradictionIds.Clear();

            _accumulatedElapsedTime = 0f;
            _lastResumeTimestamp = currentTime;
            InvestigationStartedAt = currentTime;
            HasTimeExpired = false;
            IsTimerRunning = HasActiveTimeLimit;
        }

        public void SetSelectedInvestigator(CharacterProfileSO investigator)
        {
            SelectedInvestigator = investigator;
        }

        public float GetElapsedTime(float currentTime)
        {
            return _accumulatedElapsedTime + (IsTimerRunning ? currentTime - _lastResumeTimestamp : 0f);
        }

        public void PauseTimer(float currentTime)
        {
            if (!IsTimerRunning) return;

            _accumulatedElapsedTime += currentTime - _lastResumeTimestamp;
            IsTimerRunning = false;
        }

        public bool ResumeTimer(float currentTime)
        {
            if (IsTimerRunning || HasTimeExpired || !HasActiveTimeLimit) return false;

            _lastResumeTimestamp = currentTime;
            IsTimerRunning = true;
            return true;
        }

        public bool ExpireTimer()
        {
            if (HasTimeExpired) return false;

            HasTimeExpired = true;
            IsTimerRunning = false;
            _accumulatedElapsedTime = CaseTimeLimit;
            return true;
        }

        public bool TryDiscoverEvidence(EvidenceSO evidence)
        {
            if (evidence == null || evidence.id == null) return false;
            if (!_discoveredEvidenceIds.Add(evidence.id)) return false;

            _tableEvidenceIds.Add(evidence.id);
            return true;
        }

        public bool IsEvidenceDiscovered(EvidenceSO evidence)
        {
            return evidence != null && evidence.id != null && _discoveredEvidenceIds.Contains(evidence.id);
        }

        public bool IsEvidenceDiscovered(string evidenceId)
        {
            return evidenceId != null && _discoveredEvidenceIds.Contains(evidenceId);
        }

        public bool TryMarkEvidenceExamined(EvidenceSO evidence)
        {
            return evidence != null && evidence.id != null && _examinedEvidenceIds.Add(evidence.id);
        }

        public bool IsEvidenceExamined(EvidenceSO evidence)
        {
            return evidence != null && evidence.id != null && _examinedEvidenceIds.Contains(evidence.id);
        }

        public bool ToggleEvidenceTablePresence(EvidenceSO evidence)
        {
            if (evidence == null || evidence.id == null) return false;
            if (_tableEvidenceIds.Remove(evidence.id)) return false;

            _tableEvidenceIds.Add(evidence.id);
            return true;
        }

        public bool IsEvidenceOnTable(EvidenceSO evidence)
        {
            return evidence != null && evidence.id != null && _tableEvidenceIds.Contains(evidence.id);
        }

        public bool TryDiscoverHotspot(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            string key = GetHotspotKey(evidence, hotspot);
            return key != null && _discoveredHotspotKeys.Add(key);
        }

        public bool IsHotspotDiscovered(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            string key = GetHotspotKey(evidence, hotspot);
            return key != null && _discoveredHotspotKeys.Contains(key);
        }

        /// <summary>Counts discovered hotspots for an authored evidence item in this session.</summary>
        public int GetDiscoveredHotspotCount(EvidenceSO evidence)
        {
            if (evidence == null || evidence.hotspots == null) return 0;

            int count = 0;
            foreach (EvidenceHotspot hotspot in evidence.hotspots)
            {
                if (IsHotspotDiscovered(evidence, hotspot)) count++;
            }
            return count;
        }

        public bool TryUnlockClue(string clueId, string clueText)
        {
            if (string.IsNullOrEmpty(clueId) || !_unlockedClueIds.Add(clueId)) return false;

            _unlockedClues[clueId] = clueText;
            return true;
        }

        public bool IsClueUnlocked(string clueId)
        {
            return !string.IsNullOrEmpty(clueId) && _unlockedClueIds.Contains(clueId);
        }

        public bool TryExposeContradiction(ContradictionRuleSO rule)
        {
            return rule != null && !string.IsNullOrEmpty(rule.ruleId) && _exposedContradictionIds.Add(rule.ruleId);
        }

        public bool IsContradictionExposed(string ruleId)
        {
            return !string.IsNullOrEmpty(ruleId) && _exposedContradictionIds.Contains(ruleId);
        }

        private static string GetHotspotKey(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            if (evidence == null || evidence.id == null || hotspot == null || hotspot.hotspotId == null) return null;
            return evidence.id + "\u001f" + hotspot.hotspotId;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks relationship history with NPCs for contextual conversation generation.
/// Remembers past failures, successes, and patterns of interaction.
/// </summary>
public class NPCRelationshipTracker
{
    private readonly Dictionary<string, RelationshipHistory> _histories = new Dictionary<string, RelationshipHistory>();

    /// <summary>
    /// Record a successful delivery for an NPC.
    /// </summary>
    public void RecordDelivery(string npcId, bool onTime)
    {
        EnsureHistory(npcId);

        if (onTime)
        {
            _histories[npcId].SuccessfulDeliveries++;
            _histories[npcId].LastInteractionOutcome = "successful_delivery";
        }
        else
        {
            _histories[npcId].LateDeliveries++;
            _histories[npcId].LastInteractionOutcome = "late_delivery";
        }

        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Record a failed delivery (deadline missed completely).
    /// </summary>
    public void RecordFailure(string npcId)
    {
        EnsureHistory(npcId);
        _histories[npcId].FailedDeliveries++;
        _histories[npcId].LastInteractionOutcome = "failed_delivery";
        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Record that a letter was refused.
    /// </summary>
    public void RecordRefusal(string npcId)
    {
        EnsureHistory(npcId);
        _histories[npcId].RefusedLetters++;
        _histories[npcId].LastInteractionOutcome = "refused_letter";
        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Record that a promise was made to an NPC.
    /// </summary>
    public void RecordPromise(string npcId, string promiseType)
    {
        EnsureHistory(npcId);
        _histories[npcId].ActivePromises.Add(new Promise
        {
            Type = promiseType,
            MadeAt = DateTime.Now
        });
        _histories[npcId].LastInteractionOutcome = "promise_made";
        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Record that a promise was fulfilled.
    /// </summary>
    public void RecordPromiseFulfilled(string npcId, string promiseType)
    {
        EnsureHistory(npcId);
        Promise? promise = _histories[npcId].ActivePromises
            .FirstOrDefault(p => p.Type == promiseType);

        if (promise != null)
        {
            _histories[npcId].ActivePromises.Remove(promise);
            _histories[npcId].FulfilledPromises++;
            _histories[npcId].LastInteractionOutcome = "promise_fulfilled";
        }

        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Record that a promise was broken.
    /// </summary>
    public void RecordPromiseBroken(string npcId, string promiseType)
    {
        EnsureHistory(npcId);
        Promise? promise = _histories[npcId].ActivePromises
            .FirstOrDefault(p => p.Type == promiseType);

        if (promise != null)
        {
            _histories[npcId].ActivePromises.Remove(promise);
            _histories[npcId].BrokenPromises++;
            _histories[npcId].LastInteractionOutcome = "promise_broken";
        }

        _histories[npcId].LastInteractionTime = DateTime.Now;
    }

    /// <summary>
    /// Get the number of failed deliveries for an NPC.
    /// </summary>
    public int GetFailedDeliveries(string npcId)
    {
        return _histories.ContainsKey(npcId) ? _histories[npcId].FailedDeliveries : 0;
    }

    /// <summary>
    /// Get the total number of successful deliveries for an NPC.
    /// </summary>
    public int GetSuccessfulDeliveries(string npcId)
    {
        return _histories.ContainsKey(npcId) ? _histories[npcId].SuccessfulDeliveries : 0;
    }

    /// <summary>
    /// Get the last interaction outcome with an NPC.
    /// </summary>
    public string GetLastInteractionOutcome(string npcId)
    {
        return _histories.ContainsKey(npcId) ? _histories[npcId].LastInteractionOutcome : "none";
    }

    /// <summary>
    /// Check if we've broken promises to this NPC before.
    /// </summary>
    public bool HasBrokenPromisesBefore(string npcId)
    {
        return _histories.ContainsKey(npcId) && _histories[npcId].BrokenPromises > 0;
    }

    /// <summary>
    /// Get the trust pattern with an NPC (reliable, unreliable, mixed).
    /// </summary>
    public string GetTrustPattern(string npcId)
    {
        if (!_histories.ContainsKey(npcId))
            return "unknown";

        RelationshipHistory history = _histories[npcId];
        int totalDeliveries = history.SuccessfulDeliveries + history.LateDeliveries + history.FailedDeliveries;

        if (totalDeliveries < 3)
            return "new_relationship";

        float successRate = (float)history.SuccessfulDeliveries / totalDeliveries;

        if (successRate >= 0.8f)
            return "reliable";
        else if (successRate <= 0.3f)
            return "unreliable";
        else
            return "mixed";
    }

    /// <summary>
    /// Get contextual modifiers for conversation based on history.
    /// </summary>
    public ConversationModifiers GetConversationModifiers(string npcId)
    {
        if (!_histories.ContainsKey(npcId))
        {
            return new ConversationModifiers
            {
                TrustModifier = 0,
                EmotionalModifier = EmotionalModifier.Neutral,
                HistoricalContext = "first_meeting"
            };
        }

        RelationshipHistory history = _histories[npcId];
        ConversationModifiers modifiers = new ConversationModifiers();

        // Calculate trust modifier based on history
        modifiers.TrustModifier = history.SuccessfulDeliveries * 2
                                 - history.FailedDeliveries * 3
                                 - history.BrokenPromises * 5
                                 + history.FulfilledPromises * 3;

        // Determine emotional modifier based on recent interactions
        switch (history.LastInteractionOutcome)
        {
            case "failed_delivery":
            case "promise_broken":
                modifiers.EmotionalModifier = EmotionalModifier.Negative;
                break;
            case "successful_delivery":
            case "promise_fulfilled":
                modifiers.EmotionalModifier = EmotionalModifier.Positive;
                break;
            case "refused_letter":
                modifiers.EmotionalModifier = EmotionalModifier.Hurt;
                break;
            default:
                modifiers.EmotionalModifier = EmotionalModifier.Neutral;
                break;
        }

        // Set historical context for narrative generation
        if (history.BrokenPromises > 2)
            modifiers.HistoricalContext = "serial_promise_breaker";
        else if (history.FailedDeliveries > history.SuccessfulDeliveries)
            modifiers.HistoricalContext = "unreliable_carrier";
        else if (history.SuccessfulDeliveries > 10)
            modifiers.HistoricalContext = "trusted_partner";
        else if (history.RefusedLetters > 3)
            modifiers.HistoricalContext = "selective_carrier";
        else
            modifiers.HistoricalContext = "established_relationship";

        return modifiers;
    }

    private void EnsureHistory(string npcId)
    {
        if (!_histories.ContainsKey(npcId))
        {
            _histories[npcId] = new RelationshipHistory { NPCId = npcId };
        }
    }

    private class RelationshipHistory
    {
        public string NPCId { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int RefusedLetters { get; set; }
        public int FulfilledPromises { get; set; }
        public int BrokenPromises { get; set; }
        public List<Promise> ActivePromises { get; set; } = new List<Promise>();
        public string LastInteractionOutcome { get; set; } = "none";
        public DateTime LastInteractionTime { get; set; }
    }

    private class Promise
    {
        public string Type { get; set; }
        public DateTime MadeAt { get; set; }
    }
}

/// <summary>
/// Modifiers for conversation based on relationship history.
/// </summary>
public class ConversationModifiers
{
    public int TrustModifier { get; set; }
    public EmotionalModifier EmotionalModifier { get; set; }
    public string HistoricalContext { get; set; }
}

/// <summary>
/// Emotional modifier based on recent interactions.
/// </summary>
public enum EmotionalModifier
{
    Positive,
    Neutral,
    Negative,
    Hurt
}
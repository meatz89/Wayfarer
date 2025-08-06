using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the collection, verification, and trading of rumors.
/// Rumors are a key information discovery mechanic in the literary UI.
/// </summary>
public class RumorManager
{
    private readonly GameWorld _gameWorld;
    private readonly Dictionary<string, Rumor> _knownRumors;
    private readonly List<string> _tradedRumorIds;

    public RumorManager(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        _knownRumors = new Dictionary<string, Rumor>();
        _tradedRumorIds = new List<string>();
    }

    /// <summary>
    /// Learn a new rumor
    /// </summary>
    public void LearnRumor(Rumor rumor)
    {
        if (rumor == null || string.IsNullOrEmpty(rumor.Id))
            return;

        if (!_knownRumors.ContainsKey(rumor.Id))
        {
            rumor.DiscoveredDay = _gameWorld.CurrentDay;
            _knownRumors[rumor.Id] = rumor;
        }
    }

    /// <summary>
    /// Update confidence in a rumor based on corroboration
    /// </summary>
    public void UpdateConfidence(string rumorId, RumorConfidence newConfidence)
    {
        if (_knownRumors.TryGetValue(rumorId, out Rumor? rumor))
        {
            rumor.Confidence = newConfidence;

            if (newConfidence == RumorConfidence.Verified)
                rumor.IsVerified = true;
            else if (newConfidence == RumorConfidence.False)
                rumor.IsDisproven = true;
        }
    }

    /// <summary>
    /// Verify a rumor as true
    /// </summary>
    public void VerifyRumor(string rumorId)
    {
        UpdateConfidence(rumorId, RumorConfidence.Verified);
    }

    /// <summary>
    /// Disprove a rumor as false
    /// </summary>
    public void DisproveRumor(string rumorId)
    {
        UpdateConfidence(rumorId, RumorConfidence.False);
    }

    /// <summary>
    /// Get all known rumors
    /// </summary>
    public IEnumerable<Rumor> GetKnownRumors()
    {
        return _knownRumors.Values.Where(r => !IsExpired(r));
    }

    /// <summary>
    /// Get rumors by category
    /// </summary>
    public IEnumerable<Rumor> GetRumorsByCategory(RumorCategory category)
    {
        return GetKnownRumors().Where(r => r.Category == category);
    }

    /// <summary>
    /// Get tradeable rumors (not yet traded, have value)
    /// </summary>
    public IEnumerable<Rumor> GetTradeableRumors()
    {
        return GetKnownRumors()
            .Where(r => r.TradeValue > 0 && !_tradedRumorIds.Contains(r.Id));
    }

    /// <summary>
    /// Trade a rumor (mark it as traded)
    /// </summary>
    public void TradeRumor(string rumorId)
    {
        if (_knownRumors.ContainsKey(rumorId) && !_tradedRumorIds.Contains(rumorId))
        {
            _tradedRumorIds.Add(rumorId);
        }
    }

    /// <summary>
    /// Check if a rumor has expired
    /// </summary>
    private bool IsExpired(Rumor rumor)
    {
        if (!rumor.ExpiryDay.HasValue)
            return false;

        return _gameWorld.CurrentDay > rumor.ExpiryDay.Value;
    }

    /// <summary>
    /// Generate narrative description of all known rumors
    /// </summary>
    public string GetRumorsNarrative()
    {
        List<Rumor> rumors = GetKnownRumors().ToList();
        if (!rumors.Any())
            return "You haven't heard any interesting rumors lately.";

        string narrative = "Things you've heard:\n";
        foreach (Rumor? rumor in rumors.Take(5)) // Show top 5
        {
            narrative += $"\n{rumor.GetConfidenceSymbol()} \"{rumor.Text}\" - {rumor.Source}";
        }

        if (rumors.Count > 5)
            narrative += $"\n...and {rumors.Count - 5} other whispers";

        return narrative;
    }

    /// <summary>
    /// Create a rumor from conversation
    /// </summary>
    public static Rumor CreateFromConversation(string text, string npcId, string npcName, string locationId, RumorCategory category = RumorCategory.General)
    {
        return new Rumor
        {
            Id = $"rumor_{npcId}_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Text = text,
            Source = $"heard from {npcName}",
            SourceNpcId = npcId,
            SourceLocationId = locationId,
            Category = category,
            Confidence = RumorConfidence.Unknown,
            TradeValue = DetermineTradeValue(category)
        };
    }

    /// <summary>
    /// Create a rumor from observation
    /// </summary>
    public static Rumor CreateFromObservation(string text, string locationId, RumorCategory category = RumorCategory.General)
    {
        return new Rumor
        {
            Id = $"rumor_obs_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Text = text,
            Source = "personal observation",
            SourceLocationId = locationId,
            Category = category,
            Confidence = RumorConfidence.Possible, // Observations start more confident
            TradeValue = DetermineTradeValue(category)
        };
    }

    private static int DetermineTradeValue(RumorCategory category)
    {
        return category switch
        {
            RumorCategory.Trade => 3,
            RumorCategory.Political => 5,
            RumorCategory.Danger => 4,
            RumorCategory.Opportunity => 4,
            RumorCategory.Social => 2,
            RumorCategory.Location => 3,
            _ => 1
        };
    }
}
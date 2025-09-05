using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tier levels for letters and routes
/// Determines access requirements and complexity
/// </summary>
public enum TierLevel
{
    T1 = 1,  // Basic tier - always accessible
    T2 = 2,  // Intermediate tier - requires some progress
    T3 = 3   // Advanced tier - requires significant investment
}

/// <summary>
/// Location tags that enable specific observation actions during conversations
/// These are NOT atmosphere modifiers - they determine what can be observed
/// </summary>
public enum LocationTag
{
    // Environmental conditions that enable observations
    Crowded,        // Can observe crowd dynamics, eavesdrop
    Quiet,          // Can notice subtle sounds, whispers
    Public,         // Can observe social interactions
    Private,        // Can notice hidden details

    // Atmospheric features that create opportunities
    HearthWarmed,   // Can observe who sits where, social dynamics
    AleScented,     // Can notice who's drinking, loose tongues
    MusicDrifting,  // Can observe reactions to music, covered sounds
    Sunny,          // Can see clearly, notice visual details
    Shadowed,       // Can spot hidden figures, concealed items

    // Activity types that provide context
    MarketDay,      // Can observe commerce, trades, deals
    GuardPatrol,    // Can notice security, restricted areas
    Religious,      // Can observe faithful, ceremonies
    Industrial,     // Can notice work patterns, tools
}

/// <summary>
/// Defines what observations are enabled by each tag
/// </summary>
public static class LocationTagObservations
{
    private static readonly Dictionary<LocationTag, List<ObservationAction>> TagActions = new()
    {
        [LocationTag.Crowded] = new()
        {
            new ObservationAction("eavesdrop", "Listen to nearby conversations", 1),
            new ObservationAction("scan_crowd", "Look for specific individuals", 1),
            new ObservationAction("notice_pickpocket", "Watch for thieves", 1)
        },
        [LocationTag.Quiet] = new()
        {
            new ObservationAction("listen_carefully", "Hear distant sounds", 1),
            new ObservationAction("notice_breathing", "Detect hidden focus", 1)
        },
        [LocationTag.Public] = new()
        {
            new ObservationAction("observe_social", "Note who talks to whom", 1),
            new ObservationAction("spot_authority", "Identify important figures", 1)
        },
        [LocationTag.Private] = new()
        {
            new ObservationAction("search_hidden", "Look for concealed items", 1),
            new ObservationAction("check_exits", "Note escape routes", 1)
        },
        [LocationTag.HearthWarmed] = new()
        {
            new ObservationAction("observe_flow", "See who's relaxed or guarded", 1),
            new ObservationAction("notice_regulars", "Identify frequent visitors", 1)
        },
        [LocationTag.AleScented] = new()
        {
            new ObservationAction("spot_drunk", "Identify intoxicated individuals", 1),
            new ObservationAction("overhear_boasts", "Listen to loose tongues", 1)
        },
        [LocationTag.MusicDrifting] = new()
        {
            new ObservationAction("watch_reactions", "See emotional responses", 1),
            new ObservationAction("spot_distracted", "Notice who's not listening", 1)
        },
        [LocationTag.MarketDay] = new()
        {
            new ObservationAction("watch_trades", "Observe commerce patterns", 1),
            new ObservationAction("spot_deals", "Notice special transactions", 1)
        },
        [LocationTag.GuardPatrol] = new()
        {
            new ObservationAction("track_patrols", "Note guard movements", 1),
            new ObservationAction("spot_nervous", "See who avoids guards", 1)
        }
    };

    /// <summary>
    /// Get available observation actions for a set of location tags
    /// </summary>
    public static List<ObservationAction> GetObservationActions(IEnumerable<LocationTag> tags)
    {
        // Use List with explicit deduplication instead of HashSet
        List<ObservationAction> actions = new List<ObservationAction>();
        List<string> addedActionIds = new List<string>();

        foreach (LocationTag tag in tags)
        {
            if (TagActions.ContainsKey(tag))
            {
                foreach (ObservationAction action in TagActions[tag])
                {
                    // Binary availability: action is either in the list or not
                    // Deduplicate by checking if action ID already exists
                    if (!addedActionIds.Contains(action.Id))
                    {
                        actions.Add(action);
                        addedActionIds.Add(action.Id);
                    }
                }
            }
        }

        return actions;
    }
}

/// <summary>
/// Represents an observation action that costs attention
/// </summary>
public class ObservationAction : IEquatable<ObservationAction>
{
    public string Id { get; }
    public string Description { get; }
    public int AttentionCost { get; }
    public TierLevel RequiredTier { get; }

    public ObservationAction(string id, string description, int attentionCost, TierLevel requiredTier = TierLevel.T1)
    {
        Id = id;
        Description = description;
        AttentionCost = attentionCost;
        RequiredTier = requiredTier;
    }

    public bool Equals(ObservationAction other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ObservationAction);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }
}

/// <summary>
/// Strongly-typed structure for tier-based action requirements
/// </summary>
public class TierRequirement
{
    public TierLevel MinimumTier { get; set; }
    public List<string> RequiredTags { get; set; } = new List<string>();
    public string FailureReason { get; set; }

    /// <summary>
    /// Check if a player meets the tier requirement
    /// Binary check - either meets requirement or doesn't
    /// </summary>
    public bool IsMet(TierLevel playerTier, List<string> playerTags)
    {
        // Check tier level first
        if (playerTier < MinimumTier)
            return false;

        // Check required tags (all must be present)
        foreach (string requiredTag in RequiredTags)
        {
            if (!playerTags.Contains(requiredTag))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Extension to add location tags to locations
/// </summary>
public static class LocationTagExtensions
{
    // Hardcoded for now, should come from JSON later
    private static readonly Dictionary<string, List<LocationTag>> LocationTagMap = new()
    {
        ["market_square"] = new() { LocationTag.Crowded, LocationTag.Public, LocationTag.MarketDay },
        ["copper_kettle"] = new() { LocationTag.HearthWarmed, LocationTag.AleScented, LocationTag.MusicDrifting },
        ["noble_district"] = new() { LocationTag.Quiet, LocationTag.Private, LocationTag.GuardPatrol },
        ["merchant_row"] = new() { LocationTag.Crowded, LocationTag.MarketDay, LocationTag.Industrial },
        ["riverside"] = new() { LocationTag.Public, LocationTag.Industrial, LocationTag.Shadowed },
        ["city_gates"] = new() { LocationTag.Public, LocationTag.GuardPatrol, LocationTag.Crowded }
    };

    public static List<LocationTag> GetLocationTags(string locationId)
    {
        return LocationTagMap.ContainsKey(locationId?.ToLower())
            ? LocationTagMap[locationId.ToLower()]
            : new List<LocationTag>();
    }
}
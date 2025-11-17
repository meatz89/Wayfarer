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
/// Venue tags that enable specific observation actions during conversations
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
    Shadowed,       // Can location hidden figures, concealed items

    // Activity types that provide context
    MarketDay,      // Can observe diplomacy, trades, deals
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
        // ADR-007: No Id parameter - Description is natural key
        [LocationTag.Crowded] = new()
    {
        new ObservationAction("Listen to nearby conversations", 1),
        new ObservationAction("Look for specific individuals", 1),
        new ObservationAction("Watch for thieves", 1)
    },
        [LocationTag.Quiet] = new()
    {
        new ObservationAction("Hear distant sounds", 1),
        new ObservationAction("Detect hidden focus", 1)
    },
        [LocationTag.Public] = new()
    {
        new ObservationAction("Note who talks to whom", 1),
        new ObservationAction("Identify important figures", 1)
    },
        [LocationTag.Private] = new()
    {
        new ObservationAction("Look for concealed items", 1),
        new ObservationAction("Note escape routes", 1)
    },
        [LocationTag.HearthWarmed] = new()
    {
        new ObservationAction("See who's relaxed or guarded", 1),
        new ObservationAction("Identify frequent visitors", 1)
    },
        [LocationTag.AleScented] = new()
    {
        new ObservationAction("Identify intoxicated individuals", 1),
        new ObservationAction("Listen to loose tongues", 1)
    },
        [LocationTag.MusicDrifting] = new()
    {
        new ObservationAction("See emotional responses", 1),
        new ObservationAction("Notice who's not listening", 1)
    },
        [LocationTag.MarketDay] = new()
    {
        new ObservationAction("Observe diplomacy patterns", 1),
        new ObservationAction("Notice special transactions", 1)
    },
        [LocationTag.GuardPatrol] = new()
    {
        new ObservationAction("Note guard movements", 1),
        new ObservationAction("See who avoids guards", 1)
    }
    };

    /// <summary>
    /// Get available observation actions for a set of Venue tags
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
/// ADR-007: NO Id property - Description is natural key
/// </summary>
public class ObservationAction : IEquatable<ObservationAction>
{
    // HIGHLANDER: NO Id property - Description is natural key
    public string Description { get; }
    public int AttentionCost { get; }
    public TierLevel RequiredTier { get; }

    public ObservationAction(string description, int attentionCost, TierLevel requiredTier = TierLevel.T1)
    {
        Description = description;
        AttentionCost = attentionCost;
        RequiredTier = requiredTier;
    }

    public bool Equals(ObservationAction other)
    {
        if (other == null) return false;
        // ADR-007: Natural key equality (Description, not Id)
        return Description == other.Description;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ObservationAction);
    }

    public override int GetHashCode()
    {
        // ADR-007: Hash natural key (Description), not synthetic Id
        return Description?.GetHashCode() ?? 0;
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
/// Extension to add Venue tags to locations
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

    public static List<LocationTag> GetLocationTags(string venueId)
    {
        if (string.IsNullOrEmpty(venueId))
            return new List<LocationTag>();

        string lowerVenueId = venueId.ToLower();
        if (!LocationTagMap.ContainsKey(lowerVenueId))
            return new List<LocationTag>();

        return LocationTagMap[lowerVenueId];
    }
}
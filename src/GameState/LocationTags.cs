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
/// Defines what observations are enabled by each tag.
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties per LocationTag enum value.
/// </summary>
public static class LocationTagObservations
{
    // Explicit properties per LocationTag enum value
    // ADR-007: No Id parameter - Description is natural key
    public static List<ObservationAction> CrowdedActions { get; } = new()
    {
        new ObservationAction("Listen to nearby conversations", 1),
        new ObservationAction("Look for specific individuals", 1),
        new ObservationAction("Watch for thieves", 1)
    };

    public static List<ObservationAction> QuietActions { get; } = new()
    {
        new ObservationAction("Hear distant sounds", 1),
        new ObservationAction("Detect hidden focus", 1)
    };

    public static List<ObservationAction> PublicActions { get; } = new()
    {
        new ObservationAction("Note who talks to whom", 1),
        new ObservationAction("Identify important figures", 1)
    };

    public static List<ObservationAction> PrivateActions { get; } = new()
    {
        new ObservationAction("Look for concealed items", 1),
        new ObservationAction("Note escape routes", 1)
    };

    public static List<ObservationAction> HearthWarmedActions { get; } = new()
    {
        new ObservationAction("See who's relaxed or guarded", 1),
        new ObservationAction("Identify frequent visitors", 1)
    };

    public static List<ObservationAction> AleScentedActions { get; } = new()
    {
        new ObservationAction("Identify intoxicated individuals", 1),
        new ObservationAction("Listen to loose tongues", 1)
    };

    public static List<ObservationAction> MusicDriftingActions { get; } = new()
    {
        new ObservationAction("See emotional responses", 1),
        new ObservationAction("Notice who's not listening", 1)
    };

    public static List<ObservationAction> MarketDayActions { get; } = new()
    {
        new ObservationAction("Observe diplomacy patterns", 1),
        new ObservationAction("Notice special transactions", 1)
    };

    public static List<ObservationAction> GuardPatrolActions { get; } = new()
    {
        new ObservationAction("Note guard movements", 1),
        new ObservationAction("See who avoids guards", 1)
    };

    // Empty lists for tags without defined actions
    public static List<ObservationAction> SunnyActions { get; } = new();
    public static List<ObservationAction> ShadowedActions { get; } = new();
    public static List<ObservationAction> ReligiousActions { get; } = new();
    public static List<ObservationAction> IndustrialActions { get; } = new();

    /// <summary>
    /// Get observation actions for a single tag using switch expression
    /// </summary>
    public static List<ObservationAction> GetActionsForTag(LocationTag tag)
    {
        return tag switch
        {
            LocationTag.Crowded => CrowdedActions,
            LocationTag.Quiet => QuietActions,
            LocationTag.Public => PublicActions,
            LocationTag.Private => PrivateActions,
            LocationTag.HearthWarmed => HearthWarmedActions,
            LocationTag.AleScented => AleScentedActions,
            LocationTag.MusicDrifting => MusicDriftingActions,
            LocationTag.MarketDay => MarketDayActions,
            LocationTag.GuardPatrol => GuardPatrolActions,
            LocationTag.Sunny => SunnyActions,
            LocationTag.Shadowed => ShadowedActions,
            LocationTag.Religious => ReligiousActions,
            LocationTag.Industrial => IndustrialActions,
            _ => new List<ObservationAction>()
        };
    }

    /// <summary>
    /// Get available observation actions for a set of Venue tags
    /// </summary>
    public static List<ObservationAction> GetObservationActions(IEnumerable<LocationTag> tags)
    {
        // Use List with reference equality deduplication
        List<ObservationAction> actions = new List<ObservationAction>();

        foreach (LocationTag tag in tags)
        {
            foreach (ObservationAction action in GetActionsForTag(tag))
            {
                // Binary availability: action is either in the list or not
                // HIGHLANDER: Deduplicate by object reference equality
                if (!actions.Contains(action))
                {
                    actions.Add(action);
                }
            }
        }

        return actions;
    }
}

/// <summary>
/// Represents an observation action that costs attention
/// HIGHLANDER: Uses default reference equality - instances are equal only if same object
/// </summary>
public class ObservationAction
{
    // HIGHLANDER: NO Id property - object reference is identity
    public string Description { get; }
    public int AttentionCost { get; }
    public TierLevel RequiredTier { get; }

    public ObservationAction(string description, int attentionCost, TierLevel requiredTier = TierLevel.T1)
    {
        Description = description;
        AttentionCost = attentionCost;
        RequiredTier = requiredTier;
    }

    // HIGHLANDER: No IEquatable implementation - uses default reference equality
    // Two ObservationAction instances are equal only if they're the same object in memory
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


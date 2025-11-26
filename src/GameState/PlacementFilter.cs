/// <summary>
/// Filter for selecting placement entities at spawn time via CATEGORICAL PROPERTIES ONLY
/// Implements AI content generation pattern: JSON describes categories (PersonalityTypes, LocationProperties),
/// EntityResolver queries GameWorld entities and applies SelectionStrategy to choose from matches
/// Tutorial pattern: A1 and A2 use same categorical specs → naturally reuse same entities via FindOrCreate
/// HIGHLANDER: One pattern only - categorical queries, no concrete IDs, no binding mechanisms
/// </summary>
public class PlacementFilter
{
    /// <summary>
    /// Type of placement: Location, NPC, or Route
    /// Determines which filter properties apply
    /// </summary>
    public PlacementType PlacementType { get; init; }

    // ==================== SPAWN CONTEXT RELATIVE PLACEMENT ====================

    /// <summary>
    /// Spawn-context-relative placement for situations.
    /// SameLocation = use context.CurrentLocation directly (no categorical search)
    /// SameVenue = search within context.CurrentVenue only
    /// Anywhere = standard categorical search (default)
    /// EXPLICIT: Situations must specify Proximity, no implicit inheritance
    /// </summary>
    public PlacementProximity Proximity { get; init; } = PlacementProximity.Anywhere;

    // ==================== CATEGORICAL SEARCH ====================

    /// <summary>
    /// Strategy for selecting ONE entity when multiple candidates match the filter
    /// Defaults to Random (uniform distribution)
    /// </summary>
    public PlacementSelectionStrategy SelectionStrategy { get; init; } = PlacementSelectionStrategy.Random;

    // ==================== NPC FILTERS (when PlacementType == NPC) ====================

    /// <summary>
    /// Required personality types for NPC selection
    /// Example: ["Mercantile", "Commanding"] - NPC must have one of these personalities
    /// CATEGORICAL - no concrete NPC IDs
    /// </summary>
    public List<PersonalityType> PersonalityTypes { get; init; } = new List<PersonalityType>();

    /// <summary>
    /// Required professions for NPC selection
    /// Example: [Merchant, Scholar, Guard]
    /// NPC must have one of these professions to match
    /// </summary>
    public List<Professions> Professions { get; init; } = new List<Professions>();

    /// <summary>
    /// Required relationship states for NPC selection
    /// Example: [Ally, Rival, Neutral]
    /// NPC must have one of these relationship states with player
    /// </summary>
    public List<NPCRelationship> RequiredRelationships { get; init; } = new List<NPCRelationship>();

    /// <summary>
    /// Minimum NPC tier requirement
    /// null = no minimum
    /// Example: MinTier = 2 means "only select tier 2+ NPCs"
    /// </summary>
    public int? MinTier { get; init; }

    /// <summary>
    /// Maximum NPC tier requirement
    /// null = no maximum
    /// Example: MaxTier = 3 means "only select tier 1-3 NPCs"
    /// </summary>
    public int? MaxTier { get; init; }

    /// <summary>
    /// Minimum bond strength required with player
    /// Example: MinBond = 10 means "select NPCs player has bond 10+"
    /// null = no minimum requirement
    /// </summary>
    public int? MinBond { get; init; }

    /// <summary>
    /// Maximum bond strength allowed with player
    /// Example: MaxBond = 5 means "select NPCs player has bond 5 or less"
    /// null = no maximum requirement
    /// Enables "stranger" or "unfamiliar" categorical selection
    /// </summary>
    public int? MaxBond { get; init; }

    // Orthogonal Categorical Dimensions for NPC selection
    // Multiple dimensions compose to create archetypes
    // Empty list = don't filter on this dimension (any value matches)
    // Non-empty list = NPC must have ONE OF the specified values

    /// <summary>
    /// Social standing dimension for NPC selection
    /// Example: [Notable, Authority] - select NPCs who are recognized or powerful
    /// Empty = any standing (don't filter)
    /// </summary>
    public List<NPCSocialStanding> SocialStandings { get; init; } = new List<NPCSocialStanding>();

    /// <summary>
    /// Story role dimension for NPC selection
    /// Example: [Obstacle, Facilitator] - select NPCs who block or help
    /// Empty = any role (don't filter)
    /// </summary>
    public List<NPCStoryRole> StoryRoles { get; init; } = new List<NPCStoryRole>();

    /// <summary>
    /// Knowledge level dimension for NPC selection
    /// Example: [Informed, Expert] - select NPCs aware of local events
    /// Empty = any knowledge level (don't filter)
    /// </summary>
    public List<NPCKnowledgeLevel> KnowledgeLevels { get; init; } = new List<NPCKnowledgeLevel>();

    /// <summary>
    /// DEPRECATED: NPC tags used only for DEPENDENT_NPC marker system
    /// Use orthogonal categorical dimensions (SocialStanding, StoryRole, KnowledgeLevel) instead
    /// </summary>
    public List<string> NpcTags { get; init; } = new List<string>();

    // ==================== LOCATION FILTERS (when PlacementType == Location) ====================

    /// <summary>
    /// Required location types for categorical selection
    /// Example: [Inn, Tavern, Market]
    /// Location must match one of these types
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<LocationTypes> LocationTypes { get; init; } = new List<LocationTypes>();

    /// <summary>
    /// Required location capabilities for categorical selection
    /// Example: [Crossroads, Commercial, Indoor]
    /// Location must have ALL specified capabilities to match
    /// STRONGLY-TYPED Flags enum, not strings
    /// </summary>
    public LocationCapability RequiredCapabilities { get; init; } = LocationCapability.None;

    /// <summary>
    /// Player accessibility requirement
    /// true = only accessible locations (visited OR unlocked)
    /// false/null = any location regardless of accessibility
    /// </summary>
    public bool? IsPlayerAccessible { get; init; }

    // Orthogonal Categorical Dimensions for Location selection
    // Multiple dimensions compose to create archetypes
    // Empty list = don't filter on this dimension (any value matches)
    // Non-empty list = Location must have ONE OF the specified values

    /// <summary>
    /// Privacy dimension for location selection
    /// Example: [SemiPublic, Private] - select locations with moderate to high privacy
    /// Empty = any privacy level (don't filter)
    /// </summary>
    public List<LocationPrivacy> PrivacyLevels { get; init; } = new List<LocationPrivacy>();

    /// <summary>
    /// Safety dimension for location selection
    /// Example: [Safe] - select only secure locations
    /// Empty = any safety level (don't filter)
    /// </summary>
    public List<LocationSafety> SafetyLevels { get; init; } = new List<LocationSafety>();

    /// <summary>
    /// Activity dimension for location selection
    /// Example: [Quiet, Moderate] - avoid busy crowded places
    /// Empty = any activity level (don't filter)
    /// </summary>
    public List<LocationActivity> ActivityLevels { get; init; } = new List<LocationActivity>();

    /// <summary>
    /// Purpose dimension for location selection
    /// Example: [Dwelling, Commerce] - select lodging or trade locations
    /// Empty = any purpose (don't filter)
    /// </summary>
    public List<LocationPurpose> Purposes { get; init; } = new List<LocationPurpose>();

    /// <summary>
    /// District identifier filter
    /// null = any district
    /// Specified = location must be in this district
    /// NOTE: This is concrete ID, but districts are large categorical containers
    /// </summary>
    public string DistrictId { get; init; }

    /// <summary>
    /// Region identifier filter
    /// null = any region
    /// Specified = location must be in this region
    /// NOTE: This is concrete ID, but regions are large categorical containers
    /// </summary>
    public string RegionId { get; init; }

    // ==================== ROUTE FILTERS (when PlacementType == Route) ====================

    /// <summary>
    /// Terrain types for route selection
    /// Example: [Urban, Forest, Mountain]
    /// Route must match one of these terrain types
    /// </summary>
    public List<string> TerrainTypes { get; init; } = new List<string>();

    /// <summary>
    /// Route difficulty tier requirement
    /// null = any difficulty
    /// Specified = route must match this tier
    /// </summary>
    public int? RouteTier { get; init; }

    /// <summary>
    /// Minimum difficulty rating for route selection (uses DangerRating property)
    /// null = no minimum
    /// Used to filter scenes for dangerous routes
    /// Example: MinDifficulty = 30 means "only spawn on routes with danger 30+"
    /// </summary>
    public int? MinDifficulty { get; init; }

    /// <summary>
    /// Maximum difficulty rating for route selection (uses DangerRating property)
    /// null = no maximum
    /// Used to filter scenes for safer routes
    /// Example: MaxDifficulty = 50 means "only spawn on routes with danger <= 50"
    /// </summary>
    public int? MaxDifficulty { get; init; }

    /// <summary>
    /// Categorical tags for route selection
    /// Example: ["Trade", "Dangerous", "Shortcut"]
    /// Route must have ALL specified tags to match
    /// </summary>
    public List<string> RouteTags { get; init; } = new List<string>();

    /// <summary>
    /// Specific segment index for route situation placement
    /// Enables geographic specificity: situation activates at THIS segment of route
    /// Example: SegmentIndex = 0 (first segment), 1 (second segment), 2 (third segment)
    /// IMMUTABLE: Segment placement determined at parse-time from template
    /// ARCHITECTURAL: Required for route segment situations (Tutorial A3: 4 segments with obstacles)
    /// VERISIMILITUDE: "Fallen tree blocks segment 1" = geographic reality, not abstract concept
    /// </summary>
    public int SegmentIndex { get; init; }

    // ==================== VARIETY CONTROL ====================

    /// <summary>
    /// Exclude recently used entities from selection
    /// Improves variety by preventing same location/NPC/route appearing repeatedly
    /// Uses LastUsedDay property to filter out recent selections
    /// </summary>
    public bool ExcludeRecentlyUsed { get; init; }

    // ==================== PLAYER STATE FILTERS (applies to all placement types) ====================

    /// <summary>
    /// Player must have these states for Scene to spawn
    /// Example: [Injured, Exhausted, Wanted]
    /// Player must have ALL specified states
    /// STRONGLY-TYPED enum, not strings
    /// </summary>
    public List<StateType> RequiredStates { get; init; } = new List<StateType>();

    /// <summary>
    /// Player must NOT have these states for Scene to spawn
    /// Example: [Rested, Healthy]
    /// Player must have NONE of these states
    /// Enables "only when wounded" or "only when fresh" scenarios
    /// </summary>
    public List<StateType> ForbiddenStates { get; init; } = new List<StateType>();

    /// <summary>
    /// Player must have these achievements for Scene to spawn
    /// Player must have ALL specified achievements
    /// HIGHLANDER: Object references only, no string IDs
    /// </summary>
    public List<Achievement> RequiredAchievements { get; init; } = new List<Achievement>();

    /// <summary>
    /// Player scale requirements for spawning
    /// Example: Morality >= +5, Reputation >= -3
    /// All requirements must be met
    /// </summary>
    public List<ScaleRequirement> ScaleRequirements { get; init; } = new List<ScaleRequirement>();
}

/// <summary>
/// Scale requirement for player behavioral spectrum filtering
/// Example: "Morality must be at least +5" or "Reputation must be at most -3"
/// </summary>
public class ScaleRequirement
{
    /// <summary>
    /// Which scale to check
    /// </summary>
    public ScaleType ScaleType { get; init; }

    /// <summary>
    /// Minimum required value (inclusive)
    /// null = no minimum
    /// Example: MinValue = 5 means "scale must be >= 5"
    /// </summary>
    public int? MinValue { get; init; }

    /// <summary>
    /// Maximum allowed value (inclusive)
    /// null = no maximum
    /// Example: MaxValue = -3 means "scale must be <= -3"
    /// </summary>
    public int? MaxValue { get; init; }
}

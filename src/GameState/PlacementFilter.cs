/// <summary>
/// Filter for selecting placement entities at spawn time via CATEGORICAL PROPERTIES ONLY
/// Implements AI content generation pattern: JSON describes categories (PersonalityType, LocationProperties),
/// EntityResolver queries GameWorld entities and applies SelectionStrategy to choose from matches
/// Tutorial pattern: A1 and A2 use same categorical specs → naturally reuse same entities via FindOrCreate
/// HIGHLANDER: One pattern only - categorical queries, no concrete IDs, no binding mechanisms
/// SIMPLICITY: Each property is SINGULAR (nullable enum), not plural (List).
///   null = don't filter on this property (any value matches)
///   value = entity must have exactly this value
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
    /// Required personality type for NPC selection
    /// null = don't filter by personality (any personality matches)
    /// Example: PersonalityType.Mercantile - NPC must have this personality
    /// </summary>
    public PersonalityType? PersonalityType { get; init; }

    /// <summary>
    /// Required profession for NPC selection
    /// null = don't filter by profession (any profession matches)
    /// Example: Professions.Merchant - NPC must have this profession
    /// </summary>
    public Professions? Profession { get; init; }

    /// <summary>
    /// Required relationship state for NPC selection
    /// null = don't filter by relationship (any relationship matches)
    /// Example: NPCRelationship.Ally - NPC must have this relationship with player
    /// </summary>
    public NPCRelationship? RequiredRelationship { get; init; }

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

    /// <summary>
    /// Social standing for NPC selection
    /// null = don't filter by standing (any standing matches)
    /// Example: NPCSocialStanding.Notable - select NPCs who are recognized
    /// </summary>
    public NPCSocialStanding? SocialStanding { get; init; }

    /// <summary>
    /// Story role for NPC selection
    /// null = don't filter by role (any role matches)
    /// Example: NPCStoryRole.Obstacle - select NPCs who block progress
    /// </summary>
    public NPCStoryRole? StoryRole { get; init; }

    /// <summary>
    /// Knowledge level for NPC selection
    /// null = don't filter by knowledge (any knowledge level matches)
    /// Example: NPCKnowledgeLevel.Informed - select NPCs aware of local events
    /// </summary>
    public NPCKnowledgeLevel? KnowledgeLevel { get; init; }

    /// <summary>
    /// DEPRECATED: NPC tags used only for DEPENDENT_NPC marker system
    /// Use orthogonal categorical dimensions (SocialStanding, StoryRole, KnowledgeLevel) instead
    /// </summary>
    public List<string> NpcTags { get; init; } = new List<string>();

    // ==================== LOCATION FILTERS (when PlacementType == Location) ====================

    /// <summary>
    /// Required location role for categorical selection
    /// null = don't filter by role (any role matches)
    /// Example: LocationRole.Hub - location must have this functional/narrative role
    /// </summary>
    public LocationRole? LocationRole { get; init; }

    /// <summary>
    /// Player accessibility requirement
    /// true = only accessible locations (visited OR unlocked)
    /// false/null = any location regardless of accessibility
    /// </summary>
    public bool? IsPlayerAccessible { get; init; }

    /// <summary>
    /// Privacy level for location selection
    /// null = don't filter by privacy (any privacy matches)
    /// Example: LocationPrivacy.Private - select private locations
    /// </summary>
    public LocationPrivacy? Privacy { get; init; }

    /// <summary>
    /// Safety level for location selection
    /// null = don't filter by safety (any safety matches)
    /// Example: LocationSafety.Safe - select only secure locations
    /// </summary>
    public LocationSafety? Safety { get; init; }

    /// <summary>
    /// Activity level for location selection
    /// null = don't filter by activity (any activity matches)
    /// Example: LocationActivity.Quiet - avoid busy crowded places
    /// </summary>
    public LocationActivity? Activity { get; init; }

    /// <summary>
    /// Purpose for location selection
    /// null = don't filter by purpose (any purpose matches)
    /// Example: LocationPurpose.Commerce - select trade locations
    /// </summary>
    public LocationPurpose? Purpose { get; init; }

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
    /// Terrain type for route selection (orthogonal: natural geography)
    /// null = don't filter by terrain (any terrain matches)
    /// Example: TerrainType.Forest - route must have this terrain type
    /// </summary>
    public TerrainType? Terrain { get; init; }

    /// <summary>
    /// Structure type for route selection (orthogonal: built/constructed features)
    /// null = don't filter by structure (any structure matches)
    /// Example: StructureType.Bridge - route must include this structure type
    /// </summary>
    public StructureType? Structure { get; init; }

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

/// <summary>
/// Context object for procedural A-story generation
/// Tracks player's progression through infinite A-story for intelligent generation decisions
/// Used by ProceduralAStoryService to select appropriate archetypes and avoid repetition
///
/// PROGRESSION MODEL (FLEXIBLE):
/// - Authored tutorial: A1-A3 (current), A1-A10 (design goal)
/// - Procedural continuation: Starts after last authored scene (A4+ currently, A11+ at full tutorial)
/// - Architecture works with ANY number of authored scenes
///
/// TIER ESCALATION (GROUNDED, CHARACTER-DRIVEN):
/// - Sequence 1-30: Personal stakes (individual relationships, internal conflict, character growth)
/// - Sequence 31-50: Local stakes (community dynamics, village/town consequences)
/// - Sequence 51+: Regional stakes (district/province scope, infinite at maximum grounding)
///
/// ANTI-REPETITION:
/// Track recent archetypes, regions, NPC personality types to ensure variety
/// Never use same archetype twice in 5-scene window
/// Rotate through regions systematically
/// Mix investigation/confrontation/social/discovery patterns
/// </summary>
public class AStoryContext
{
    /// <summary>
    /// Current A-story sequence number (works from ANY sequence)
    /// Determines tier escalation and narrative scope
    /// Example: 4, 5, 6... (if tutorial is A1-A3) or 11, 12, 13... (if tutorial is A1-A10)
    /// </summary>
    public int CurrentSequence { get; set; }

    /// <summary>
    /// Last completed A-story sequence
    /// Used to determine next sequence and validate progression
    /// </summary>
    public int LastCompletedSequence { get; set; }

    /// <summary>
    /// Completed A-story scenes (for tracking progression history)
    /// HIGHLANDER: Store scene objects, not string IDs
    /// Used to prevent re-spawning and validate chain integrity
    /// </summary>
    public List<Scene> CompletedScenes { get; set; } = new List<Scene>();

    /// <summary>
    /// Recent archetype types used (last 5 scenes)
    /// Anti-repetition: Avoid same archetype in 5-scene window
    /// Rolling window updated after each generation
    /// HIGHLANDER: Uses SceneArchetypeType enum, not strings
    /// </summary>
    public List<SceneArchetypeType> RecentArchetypes { get; set; } = new List<SceneArchetypeType>();

    /// <summary>
    /// Recent regions visited (last 3 scenes)
    /// Anti-repetition: Rotate through different regions
    /// Systematically explore world geography
    /// HIGHLANDER: Store Region objects, not string IDs
    /// </summary>
    public List<Region> RecentRegions { get; set; } = new List<Region>();

    /// <summary>
    /// Recent NPC personality types encountered (last 3 scenes)
    /// Anti-repetition: Vary social interactions
    /// Mix Diplomatic/Aggressive/Calculating/Zealous personalities
    /// </summary>
    public List<PersonalityType> RecentPersonalityTypes { get; set; } = new List<PersonalityType>();

    /// <summary>
    /// Current tier (calculated from sequence)
    /// Tier 1: Sequence 1-30 (personal stakes - relationships, internal conflict)
    /// Tier 2: Sequence 31-50 (local stakes - community, village/town)
    /// Tier 3: Sequence 51+ (regional stakes - district/province, maximum scope)
    /// </summary>
    public int CalculatedTier
    {
        get
        {
            if (CurrentSequence <= 30) return 1; // Personal
            if (CurrentSequence <= 50) return 2; // Local
            return 3; // Regional (infinite)
        }
    }

    /// <summary>
    /// Regions player has unlocked through A-story progression
    /// A-story progressively unlocks new regions for B/C story content
    /// Tracks narrative progression across geography
    /// </summary>
    public List<string> UnlockedRegionNames { get; set; } = new List<string>();

    /// <summary>
    /// Order members player has met
    /// Tracks revealed Order members for narrative continuity
    /// Each Order member knows one piece of the mystery
    /// </summary>
    public List<string> EncounteredOrderMemberIds { get; set; } = new List<string>();

    /// <summary>
    /// Order artifacts player has collected
    /// Tangible progression tokens throughout infinite journey
    /// Physical evidence of investigation depth
    /// </summary>
    public List<string> CollectedArtifactIds { get; set; } = new List<string>();

    /// <summary>
    /// Major revelations player has uncovered
    /// Narrative breadcrumbs deepening mystery without resolution
    /// Pursuit framework: Each revelation poses new questions
    /// </summary>
    public List<string> UncoveredRevelationIds { get; set; } = new List<string>();

    /// <summary>
    /// Current pursuit goal description
    /// Dynamically evolves based on recent revelations
    /// Example: "Find the keeper of the Eastern Seal" → "Uncover the conspiracy in the capital"
    /// Never resolved, always evolving deeper
    /// </summary>
    public string CurrentPursuitGoal { get; set; } = "Discover the fate of the scattered Order";

    /// <summary>
    /// Update context after completing A-scene
    /// Advances sequence, tracks completion, updates anti-repetition windows
    /// HIGHLANDER: Accept Scene and Region objects, not string IDs
    /// </summary>
    public void RecordCompletion(Scene scene, SceneArchetypeType archetype, Region region, PersonalityType? personalityType)
    {
        LastCompletedSequence = CurrentSequence;
        CurrentSequence++;

        CompletedScenes.Add(scene);

        // Update anti-repetition rolling windows
        RecentArchetypes.Add(archetype);
        if (RecentArchetypes.Count > 5)
        {
            RecentArchetypes.RemoveAt(0); // Remove oldest
        }

        // HIGHLANDER: Store Region object, not string ID
        if (region != null && !RecentRegions.Contains(region))
        {
            RecentRegions.Add(region);
            if (RecentRegions.Count > 3)
            {
                RecentRegions.RemoveAt(0);
            }
        }

        if (personalityType.HasValue && !RecentPersonalityTypes.Contains(personalityType.Value))
        {
            RecentPersonalityTypes.Add(personalityType.Value);
            if (RecentPersonalityTypes.Count > 3)
            {
                RecentPersonalityTypes.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Check if archetype is recent (used in last 5 scenes)
    /// Anti-repetition: Don't select same archetype twice in 5-scene window
    /// HIGHLANDER: Uses SceneArchetypeType enum, not strings
    /// </summary>
    public bool IsArchetypeRecent(SceneArchetypeType archetype)
    {
        return RecentArchetypes.Contains(archetype);
    }

    /// <summary>
    /// Check if region is recent (used in last 3 scenes)
    /// Anti-repetition: Rotate through different regions
    /// HIGHLANDER: Accepts Region object, not string ID
    /// </summary>
    public bool IsRegionRecent(Region region)
    {
        return RecentRegions.Contains(region);
    }

    /// <summary>
    /// Check if personality type is recent (encountered in last 3 scenes)
    /// Anti-repetition: Vary social dynamics
    /// </summary>
    public bool IsPersonalityTypeRecent(PersonalityType personalityType)
    {
        return RecentPersonalityTypes.Contains(personalityType);
    }

    /// <summary>
    /// Create initial empty context for procedural generation
    /// Called when player has no completed A-story scenes yet
    /// Actual sequence number passed to GenerateNextATemplate(), not stored here
    /// </summary>
    public static AStoryContext InitializeForProceduralGeneration()
    {
        return new AStoryContext
        {
            CurrentSequence = 0, // Placeholder - actual sequence passed to generation method
            LastCompletedSequence = 0, // Placeholder - no completed scenes yet
            CompletedScenes = new List<Scene>(),
            RecentArchetypes = new List<SceneArchetypeType>(),
            RecentRegions = new List<Region>(),
            RecentPersonalityTypes = new List<PersonalityType>(),
            UnlockedRegionNames = new List<string>(),
            EncounteredOrderMemberIds = new List<string>(),
            CollectedArtifactIds = new List<string>(),
            UncoveredRevelationIds = new List<string>(),
            CurrentPursuitGoal = "Discover the fate of the scattered Order"
        };
    }

    /// <summary>
    /// Get narrative framing for current sequence
    /// Provides AI generation context for scene narrative tone/scope
    /// Grounded character-driven narrative (Le Guin/Rothfuss style)
    /// </summary>
    public string GetNarrativeFraming()
    {
        return CurrentSequence switch
        {
            <= 30 => "personal_investigation",  // Individual relationships, character stakes
            <= 50 => "local_conspiracy",        // Community dynamics, village/town
            _ => "regional_intrigue"        // District/province scope
        };
    }

    /// <summary>
    /// Get story stakes description for AI narrative generation
    /// Grounded personal escalation (never fantasy-tropey)
    /// </summary>
    public string GetStakesDescription()
    {
        return CurrentSequence switch
        {
            <= 30 => "The mystery is deeply personal. Relationships you trust may harbor secrets. Your understanding of yourself is at stake.",
            <= 50 => "The truth affects those around you. Your community's fabric unravels as you discover what lies beneath familiar faces.",
            _ => "What you uncover reaches across the province. The regional order you knew was built on foundations of deception."
        };
    }
}

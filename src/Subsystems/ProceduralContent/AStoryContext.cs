/// <summary>
/// Context object for procedural A-story generation
/// Tracks player's progression through infinite A-story for intelligent generation decisions
/// Used by ProceduralAStoryService to select appropriate archetypes and avoid repetition
///
/// PROGRESSION MODEL:
/// - A1-A10: Authored tutorial introducing mechanics and narrative foundation
/// - A11+: Infinite procedural continuation deepening mystery without resolution
///
/// TIER ESCALATION:
/// - A11-A20: Local stakes (village/town level)
/// - A21-A30: Regional stakes (district level)
/// - A31-A40: Continental stakes (empire level)
/// - A41-A50: Cosmic stakes (reality level)
/// - A51+: Infinite cycling at cosmic tier with archetype variation
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
/// Current A-story sequence number (11, 12, 13... infinity)
/// Determines tier escalation and narrative scope
/// </summary>
public int CurrentSequence { get; set; }

/// <summary>
/// Last completed A-story sequence
/// Used to determine next sequence and validate progression
/// </summary>
public int LastCompletedSequence { get; set; }

/// <summary>
/// Completed A-story scene IDs (for tracking progression history)
/// Used to prevent re-spawning and validate chain integrity
/// </summary>
public List<string> CompletedASceneIds { get; set; } = new List<string>();

/// <summary>
/// Recent archetype IDs used (last 5 scenes)
/// Anti-repetition: Avoid same archetype in 5-scene window
/// Rolling window updated after each generation
/// </summary>
public List<string> RecentArchetypeIds { get; set; } = new List<string>();

/// <summary>
/// Recent region IDs visited (last 3 scenes)
/// Anti-repetition: Rotate through different regions
/// Systematically explore world geography
/// </summary>
public List<string> RecentRegionIds { get; set; } = new List<string>();

/// <summary>
/// Recent NPC personality types encountered (last 3 scenes)
/// Anti-repetition: Vary social interactions
/// Mix Diplomatic/Aggressive/Calculating/Zealous personalities
/// </summary>
public List<PersonalityType> RecentPersonalityTypes { get; set; } = new List<PersonalityType>();

/// <summary>
/// Current tier (calculated from sequence)
/// Tier 1: A11-A20 (local stakes)
/// Tier 2: A21-A30 (regional stakes)
/// Tier 3: A31-A40 (continental stakes)
/// Tier 4: A41-A50 (cosmic stakes)
/// Tier 4+: A51+ (infinite cosmic)
/// </summary>
public int CalculatedTier
{
    get
    {
        if (CurrentSequence <= 20) return 1;
        if (CurrentSequence <= 30) return 2;
        if (CurrentSequence <= 40) return 3;
        return 4; // Cosmic tier continues infinitely
    }
}

/// <summary>
/// Regions player has unlocked through A-story progression
/// A-story progressively unlocks new regions for B/C story content
/// Tracks narrative progression across geography
/// </summary>
public List<string> UnlockedRegionIds { get; set; } = new List<string>();

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
/// </summary>
public void RecordCompletion(string sceneId, string archetypeId, string regionId, PersonalityType? personalityType)
{
    LastCompletedSequence = CurrentSequence;
    CurrentSequence++;

    CompletedASceneIds.Add(sceneId);

    // Update anti-repetition rolling windows
    RecentArchetypeIds.Add(archetypeId);
    if (RecentArchetypeIds.Count > 5)
    {
        RecentArchetypeIds.RemoveAt(0); // Remove oldest
    }

    if (!string.IsNullOrEmpty(regionId) && !RecentRegionIds.Contains(regionId))
    {
        RecentRegionIds.Add(regionId);
        if (RecentRegionIds.Count > 3)
        {
            RecentRegionIds.RemoveAt(0);
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
/// </summary>
public bool IsArchetypeRecent(string archetypeId)
{
    return RecentArchetypeIds.Contains(archetypeId);
}

/// <summary>
/// Check if region is recent (used in last 3 scenes)
/// Anti-repetition: Rotate through different regions
/// </summary>
public bool IsRegionRecent(string regionId)
{
    return RecentRegionIds.Contains(regionId);
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
/// Create initial context for A11 generation (after A10 tutorial completion)
/// </summary>
public static AStoryContext InitializeForProceduralGeneration()
{
    return new AStoryContext
    {
        CurrentSequence = 11,
        LastCompletedSequence = 10,
        CompletedASceneIds = new List<string>(),
        RecentArchetypeIds = new List<string>(),
        RecentRegionIds = new List<string>(),
        RecentPersonalityTypes = new List<PersonalityType>(),
        UnlockedRegionIds = new List<string>(),
        EncounteredOrderMemberIds = new List<string>(),
        CollectedArtifactIds = new List<string>(),
        UncoveredRevelationIds = new List<string>(),
        CurrentPursuitGoal = "Discover the fate of the scattered Order"
    };
}

/// <summary>
/// Get narrative framing for current sequence
/// Provides AI generation context for scene narrative tone/scope
/// </summary>
public string GetNarrativeFraming()
{
    return CurrentSequence switch
    {
        <= 20 => "local_investigation",  // Village/town scale
        <= 30 => "regional_conspiracy",  // District/regional scale
        <= 40 => "continental_intrigue", // Empire-wide scale
        _     => "cosmic_mystery"        // Reality-threatening scale
    };
}

/// <summary>
/// Get story stakes description for AI narrative generation
/// Escalates dramatically as sequence increases
/// </summary>
public string GetStakesDescription()
{
    return CurrentSequence switch
    {
        <= 20 => "The mystery deepens in your immediate surroundings. Local figures may know more than they reveal.",
        <= 30 => "The conspiracy extends beyond local borders. Regional powers are involved, and the stakes rise.",
        <= 40 => "The truth you seek threatens the stability of entire nations. Continental forces align against discovery.",
        _     => "Reality itself bends around the secrets you pursue. The very fabric of existence hangs in balance."
    };
}
}

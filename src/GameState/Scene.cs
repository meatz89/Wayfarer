using System.Collections.Generic;

/// <summary>
/// Scene - ephemeral UI construct generated per location/NPC visit
/// NOT stored in GameWorld - generated fresh each visit by SceneFacade
/// Contains all available situations for current context
/// </summary>
public class Scene
{
    /// <summary>
    /// Location ID if this is a location scene
    /// null for NPC scenes
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// NPC ID if this is an NPC interaction scene
    /// null for location scenes
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// Display name for this scene context
    /// Example: "Old Mill - Courtyard", "Conversation with Martha"
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Intro narrative for this scene visit
    /// Generated fresh each time based on current game state
    /// </summary>
    public string IntroNarrative { get; set; }

    /// <summary>
    /// All available situations in this scene
    /// Categorized by interaction type and status
    /// </summary>
    public List<Situation> AvailableSituations { get; set; } = new List<Situation>();

    /// <summary>
    /// Locked situations (visible but not accessible)
    /// Shown with requirement information for player visibility
    /// </summary>
    public List<SituationWithLockReason> LockedSituations { get; set; } = new List<SituationWithLockReason>();

    /// <summary>
    /// Current day when scene was generated
    /// </summary>
    public int CurrentDay { get; set; }

    /// <summary>
    /// Current time block when scene was generated
    /// </summary>
    public TimeBlocks CurrentTimeBlock { get; set; }

    /// <summary>
    /// Current segment when scene was generated
    /// </summary>
    public int CurrentSegment { get; set; }
}

/// <summary>
/// STRONGLY-TYPED REQUIREMENT GAP CLASSES
/// Each class documents its UI execution context and enables type-specific rendering
/// </summary>

/// <summary>
/// Unmet bond requirement - for relationship-gated situations
/// UI Context: Render NPC portrait, bond progress bar, "Talk to X" guidance
/// </summary>
public class UnmetBondRequirement
{
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet scale requirement - for behavioral reputation gates
/// UI Context: Render scale spectrum visualization, current position marker
/// </summary>
public class UnmetScaleRequirement
{
    public ScaleType ScaleType { get; set; }
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet resolve requirement - for strategic resource gates
/// UI Context: Render progress bar with current/required resolve
/// </summary>
public class UnmetResolveRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet coins requirement - for economic gates
/// UI Context: Render coin amount with "Earn X more coins" guidance
/// </summary>
public class UnmetCoinsRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet situation count requirement - for progression gates
/// UI Context: Render completion counter "Complete X more situations"
/// </summary>
public class UnmetSituationCountRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet achievement requirement - for milestone gates
/// UI Context: Render achievement badge, link to earning situation
/// </summary>
public class UnmetAchievementRequirement
{
    public string AchievementId { get; set; }
    public bool MustHave { get; set; }  // true = must have, false = must NOT have
}

/// <summary>
/// Unmet state requirement - for temporary condition gates
/// UI Context: Render state icon, show how to gain/remove state
/// </summary>
public class UnmetStateRequirement
{
    public StateType StateType { get; set; }
    public bool MustHave { get; set; }  // true = must have state, false = must NOT have state
}

/// <summary>
/// Situation with lock reason - for displaying why a situation is locked
/// Perfect information pattern: player sees what they need to unlock
/// </summary>
public class SituationWithLockReason
{
    /// <summary>
    /// The locked situation
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// Human-readable explanation of why this situation is locked
    /// Example: "Requires Bond 15+ with Martha OR Morality +8"
    /// </summary>
    public string LockReason { get; set; }

    /// <summary>
    /// CONTEXTUAL PROPERTIES - each enables different UI execution contexts
    /// UI renders type-specific components based on requirement type
    /// </summary>
    public List<UnmetBondRequirement> UnmetBonds { get; set; } = new List<UnmetBondRequirement>();
    public List<UnmetScaleRequirement> UnmetScales { get; set; } = new List<UnmetScaleRequirement>();
    public List<UnmetResolveRequirement> UnmetResolve { get; set; } = new List<UnmetResolveRequirement>();
    public List<UnmetCoinsRequirement> UnmetCoins { get; set; } = new List<UnmetCoinsRequirement>();
    public List<UnmetSituationCountRequirement> UnmetSituationCount { get; set; } = new List<UnmetSituationCountRequirement>();
    public List<UnmetAchievementRequirement> UnmetAchievements { get; set; } = new List<UnmetAchievementRequirement>();
    public List<UnmetStateRequirement> UnmetStates { get; set; } = new List<UnmetStateRequirement>();
}

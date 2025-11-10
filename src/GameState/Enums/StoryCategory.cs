
/// <summary>
/// Categorizes scene content by narrative role
/// Main story progresses sequentially (A1-A10) then procedurally (A11+)
/// Side stories provide optional content unlocked by A-story progression
/// Service stories provide repeatable transactional content (inns, merchants, healers)
/// </summary>
public enum StoryCategory
{
    /// <summary>
    /// Main narrative progression (A-stories)
    /// Sequential authored tutorial (A1-A10) followed by infinite procedural continuation (A11+)
    /// Linear progression: Each A-scene unlocks next A-scene upon completion
    /// Guaranteed forward progression: Every situation has zero-requirement success path
    /// </summary>
    MainStory,

    /// <summary>
    /// Optional side content (B-stories)
    /// Unlocked by A-story progression (regions/NPCs revealed by A-scenes)
    /// Non-linear: Can engage in any order once unlocked
    /// No progression requirements: Can skip without blocking A-story
    /// </summary>
    SideStory,

    /// <summary>
    /// Repeatable service content (C-stories)
    /// Transactional scenes: Inns, merchants, healers, bathhouses
    /// Repeatable: Can engage multiple times
    /// Resource-gated: Require coins/items but provide services
    /// </summary>
    Service
}

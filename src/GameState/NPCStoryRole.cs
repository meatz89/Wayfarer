/// <summary>
/// Narrative function of NPC in player's journey
/// Orthogonal categorical dimension for entity resolution
/// Determines whether NPC helps, hinders, or remains neutral to player progress
/// </summary>
public enum NPCStoryRole
{
    /// <summary>
    /// Blocks, challenges, opposes player progress
    /// Gatekeepers requiring persuasion, antagonists, obstacles
    /// Example: Guard blocking passage, rival, suspicious merchant
    /// </summary>
    Obstacle,

    /// <summary>
    /// Background presence, transactional relationship, indifferent to player goals
    /// Standard service providers, neutral NPCs, uninvolved parties
    /// Example: Shop keeper, courier, generic townsperson
    /// </summary>
    Neutral,

    /// <summary>
    /// Helps, enables, supports player progress
    /// Guides, allies, mentors, willing collaborators
    /// Example: Quest giver, helpful informant, friendly guide
    /// </summary>
    Facilitator
}

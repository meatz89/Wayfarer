/// <summary>
/// Information access and awareness of NPC
/// Orthogonal categorical dimension for entity resolution
/// Determines what NPC knows about local events, secrets, and world state
/// </summary>
public enum NPCKnowledgeLevel
{
    /// <summary>
    /// Knows little beyond immediate surroundings
    /// Generic knowledge, unaware of larger events, focused on daily life
    /// Example: Common laborer, isolated hermit, newcomer
    /// </summary>
    Ignorant,

    /// <summary>
    /// Aware of local events and connected to community
    /// Hears gossip, knows local politics, understands regional dynamics
    /// Example: Innkeeper, merchant, priest, regular patron
    /// </summary>
    Informed,

    /// <summary>
    /// Deep specialized knowledge or insider information
    /// Expert in domain, privy to secrets, scholarly understanding
    /// Example: Master scholar, spy, noble courtier, guild master
    /// </summary>
    Expert
}

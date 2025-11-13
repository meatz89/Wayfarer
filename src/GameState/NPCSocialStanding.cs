/// <summary>
/// Social power and community standing of NPC
/// Orthogonal categorical dimension for entity resolution
/// Combines with StoryRole and KnowledgeLevel to create composite archetypes
/// </summary>
public enum NPCSocialStanding
{
    /// <summary>
    /// General population - workers, peasants, common folk
    /// No special authority or recognition
    /// Example: Generic merchant, laborer, servant
    /// </summary>
    Commoner,

    /// <summary>
    /// Respected community figures - known and trusted
    /// Local influence, recognized expertise, established reputation
    /// Example: Master craftsman, innkeeper, guild member, local merchant
    /// </summary>
    Notable,

    /// <summary>
    /// Power to make binding decisions and control access
    /// Official position, command authority, enforcement capability
    /// Example: Guard captain, noble, magistrate, temple high priest
    /// </summary>
    Authority
}

/// <summary>
/// Defines what effect a goal has when completed
/// </summary>
public enum GoalEffectType
{
    /// <summary>
    /// No obstacle interaction - ambient goals like "Rest", "Chat", "Browse goods"
    /// These are repeatable actions loaded at game initialization
    /// </summary>
    None,

    /// <summary>
    /// Preparation goal - reduces parent obstacle properties
    /// Makes better resolution options become available
    /// Example: "Scout Enemy" reduces PhysicalDanger, unlocking "Tactical Strike"
    /// </summary>
    ReduceProperties,

    /// <summary>
    /// Resolution goal - removes parent obstacle entirely
    /// Complete solution to the challenge
    /// Example: "Force Through", "Negotiate Passage", "Find Hidden Path"
    /// </summary>
    RemoveObstacle
}

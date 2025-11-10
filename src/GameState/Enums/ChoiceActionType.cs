
/// <summary>
/// Classification of what happens when player selects a Choice
/// Determines resolution flow
/// </summary>
public enum ChoiceActionType
{
/// <summary>
/// Apply cost and reward immediately, advance situation
/// No challenge, instant resolution
/// </summary>
Instant,

/// <summary>
/// Enter tactical challenge (Social, Mental, or Physical)
/// Challenge must be completed to apply rewards
/// </summary>
StartChallenge,

/// <summary>
/// Move player to new location, NPC, or route
/// Navigation action with optional auto-trigger
/// </summary>
Navigate
}

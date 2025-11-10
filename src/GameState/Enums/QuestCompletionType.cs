/// <summary>
/// Types of quest/story completions that can clear player states
/// Used by StateClearingBehavior for quest-based state clearing
/// </summary>
public enum QuestCompletionType
{
/// <summary>
/// Complete a quest to clear your name from accusations
/// </summary>
ClearName,

/// <summary>
/// Complete a quest to restore your damaged reputation
/// </summary>
RestoreReputation,

/// <summary>
/// Complete a quest to restore lost honor
/// </summary>
RestoreHonor,

/// <summary>
/// Achieve a goal related to an obsession state
/// </summary>
AchieveGoal
}

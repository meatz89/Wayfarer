using System.Collections.Generic;

/// <summary>
/// Represents a one-time request from an NPC with multiple cards at different rapport thresholds
/// </summary>
public class GoalCard
{
    /// <summary>
    /// Unique identifier for the request
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the request
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Narrative description of what the NPC is asking
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The text displayed when the NPC presents this request (shown on LISTEN action)
    /// </summary>
    public string NpcRequestText { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this request uses (default Social for NPCs)
    /// </summary>
    public TacticalSystemType SystemType { get; set; } = TacticalSystemType.Social;

    /// <summary>
    /// The engagement type ID this request uses (ID within SystemType collection)
    /// Replaces ConversationTypeId - now supports all three systems
    /// </summary>
    public string ChallengeTypeId { get; set; }

    /// <summary>
    /// Category that must match the conversation type's category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this request
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the request
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.Available;

    /// <summary>
    /// Momentum thresholds for tiered rewards (used by stranger conversations)
    /// </summary>
    public List<int> MomentumThresholds { get; set; } = new List<int>();

    /// <summary>
    /// Rewards for reaching momentum thresholds (used by stranger conversations)
    /// </summary>
    public List<GoalReward> Rewards { get; set; } = new List<GoalReward>();

    /// <summary>
    /// Tiered goals with different momentum thresholds and weights (new system)
    /// </summary>
    public List<NPCRequestGoal> Goals { get; set; } = new List<NPCRequestGoal>();
    public string NpcId { get; internal set; }
    public string Location { get; internal set; }

    /// <summary>
    /// Check if this request is available to attempt
    /// </summary>
    public bool IsAvailable()
    {
        return Status == GoalStatus.Available;
    }

    /// <summary>
    /// Mark this request as completed
    /// </summary>
    public void Complete()
    {
        Status = GoalStatus.Completed;
    }
}

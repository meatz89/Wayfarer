/// <summary>
/// DTO for ConversationTree - simple dialogue tree system for non-tactical NPC conversations.
/// Can escalate to tactical Social challenges when tension rises.
/// Uses categorical properties to match participant NPCs (DDR-006)
/// </summary>
public class ConversationTreeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public PlacementFilterDTO ParticipantFilter { get; set; }

    /// <summary>
    /// Minimum relationship level required to access this conversation
    /// </summary>
    public int MinimumRelationship { get; set; }

    /// <summary>
    /// Knowledge tokens required to unlock this conversation
    /// </summary>
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Time blocks when this conversation is available
    /// Values: "Morning", "Afternoon", "Evening", "Night"
    /// </summary>
    public List<string> AvailableTimeBlocks { get; set; } = new List<string>();

    /// <summary>
    /// Dialogue nodes that make up the conversation tree
    /// </summary>
    public List<DialogueNodeDTO> Nodes { get; set; } = new List<DialogueNodeDTO>();

    /// <summary>
    /// ID of the starting node for this conversation
    /// </summary>
    public string StartingNodeId { get; set; }

    /// <summary>
    /// Whether this conversation can be repeated after completion
    /// </summary>
    public bool IsRepeatable { get; set; }

    /// <summary>
    /// Whether this conversation has been completed
    /// </summary>
    public bool IsCompleted { get; set; }
}

/// <summary>
/// A single node in a conversation tree representing NPC dialogue
/// and available player responses.
/// </summary>
public class DialogueNodeDTO
{
    public string Id { get; set; }

    /// <summary>
    /// What the NPC says at this node
    /// </summary>
    public string NpcDialogue { get; set; }

    /// <summary>
    /// Available player responses at this node
    /// </summary>
    public List<DialogueResponseDTO> Responses { get; set; } = new List<DialogueResponseDTO>();
}

/// <summary>
/// A player response option within a dialogue node.
/// Costs resources, may have requirements, grants outcomes.
/// </summary>
public class DialogueResponseDTO
{
    public string Id { get; set; }

    /// <summary>
    /// What the player says when choosing this response
    /// </summary>
    public string ResponseText { get; set; }

    /// <summary>
    /// Focus cost to choose this response
    /// </summary>
    public int FocusCost { get; set; }

    /// <summary>
    /// Time cost in segments to choose this response
    /// </summary>
    public int TimeCost { get; set; }

    /// <summary>
    /// Required player stat to choose this response
    /// Values: "Charm", "Reason", "Intimidation", "Deception", "Insight", "Rapport"
    /// </summary>
    public string RequiredStat { get; set; }

    /// <summary>
    /// Required stat level to choose this response
    /// </summary>
    public int? RequiredStatLevel { get; set; }

    /// <summary>
    /// Next node to transition to (null = ends conversation)
    /// </summary>
    public string NextNodeId { get; set; }

    /// <summary>
    /// Change in NPC relationship from choosing this response
    /// </summary>
    public int RelationshipDelta { get; set; }

    /// <summary>
    /// Knowledge tokens granted by this response
    /// </summary>
    public List<string> GrantedKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Situation IDs spawned by this response
    /// </summary>
    public List<string> SpawnedSituationIds { get; set; } = new List<string>();

    /// <summary>
    /// Whether this response escalates to a tactical Social challenge
    /// </summary>
    public bool EscalatesToSocialChallenge { get; set; }

    /// <summary>
    /// Situation ID for the Social challenge if this response escalates
    /// </summary>
    public string SocialChallengeSituationId { get; set; }
}

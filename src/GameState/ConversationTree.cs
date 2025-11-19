/// <summary>
/// Simple dialogue tree system for non-tactical NPC conversations.
/// Can escalate to tactical Social challenges when tension rises.
/// </summary>
public class ConversationTree
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object reference ONLY, no NpcId
    public NPC Npc { get; set; }

    // Availability conditions
    public int MinimumRelationship { get; set; }
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();

    // Tree structure
    public List<DialogueNode> Nodes { get; set; } = new List<DialogueNode>();
    // HIGHLANDER: Object reference ONLY, no StartingNodeId
    public DialogueNode StartingNode { get; set; }

    // Lifecycle
    public bool IsRepeatable { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>
/// A single node in a conversation tree representing NPC dialogue
/// and available player responses.
/// </summary>
public class DialogueNode
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string NpcDialogue { get; set; }
    public List<DialogueResponse> Responses { get; set; } = new List<DialogueResponse>();
}

/// <summary>
/// A player response option within a dialogue node.
/// Costs resources, may have requirements, grants outcomes.
/// </summary>
public class DialogueResponse
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string ResponseText { get; set; }

    // Costs
    public int FocusCost { get; set; }
    public int TimeCost { get; set; }  // Segments

    // Requirements
    public PlayerStatType? RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }

    // Outcomes
    // HIGHLANDER: Object reference ONLY, no NextNodeId
    public DialogueNode NextNode { get; set; }  // null = ends conversation
    public int RelationshipDelta { get; set; }
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    // HIGHLANDER: Object references ONLY, no SpawnedSituationIds
    public List<Situation> SpawnedSituations { get; set; } = new List<Situation>();

    // Escalation to tactical Social challenge
    public bool EscalatesToSocialChallenge { get; set; }
    // HIGHLANDER: Object reference ONLY, no SocialChallengeSituationId
    public Situation SocialChallengeSituation { get; set; }  // If escalates
}

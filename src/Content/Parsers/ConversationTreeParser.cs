/// <summary>
/// Parser for converting ConversationTreeDTO to ConversationTree domain model
/// </summary>
public static class ConversationTreeParser
{
/// <summary>
/// Convert ConversationTreeDTO to ConversationTree entity
/// </summary>
public static ConversationTree Parse(ConversationTreeDTO dto, GameWorld gameWorld)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    if (string.IsNullOrEmpty(dto.Id))
        throw new InvalidOperationException("ConversationTree missing required 'Id' field");

    if (string.IsNullOrEmpty(dto.Name))
        throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'Name' field");

    if (string.IsNullOrEmpty(dto.NpcId))
        throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'NpcId' field");

    if (dto.Nodes == null || dto.Nodes.Count == 0)
        throw new InvalidOperationException($"ConversationTree '{dto.Id}' must have at least one node");

    if (string.IsNullOrEmpty(dto.StartingNodeId))
        throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'StartingNodeId' field");

    // Verify NPC exists
    NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.NpcId);
    if (npc == null)
    {
        throw new InvalidOperationException(
            $"ConversationTree '{dto.Id}' references unknown NPC '{dto.NpcId}'");
    }

    // Parse time blocks from string to enum
    List<TimeBlocks> timeBlocks = new List<TimeBlocks>();
    if (dto.AvailableTimeBlocks != null)
    {
        foreach (string timeBlockString in dto.AvailableTimeBlocks)
        {
            if (Enum.TryParse<TimeBlocks>(timeBlockString, ignoreCase: true, out TimeBlocks timeBlock))
            {
                timeBlocks.Add(timeBlock);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid time block '{timeBlockString}' in ConversationTree '{dto.Id}'. " +
                    $"Must be one of: {string.Join(", ", Enum.GetNames<TimeBlocks>())}");
            }
        }
    }

    ConversationTree tree = new ConversationTree
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description ?? "",
        Npc = npc,  // Resolve object reference during parsing (HIGHLANDER: ID is parsing artifact)
        MinimumRelationship = dto.MinimumRelationship,
        RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
        AvailableTimeBlocks = timeBlocks,
        IsRepeatable = dto.IsRepeatable,
        IsCompleted = dto.IsCompleted,
        StartingNodeId = dto.StartingNodeId
    };

    // Parse dialogue nodes
    foreach (DialogueNodeDTO nodeDto in dto.Nodes)
    {
        DialogueNode node = ParseNode(nodeDto, dto.Id);
        tree.Nodes.Add(node);
    }

    // Validate starting node exists
    if (!tree.Nodes.Any(n => n.Id == tree.StartingNodeId))
    {
        throw new InvalidOperationException(
            $"ConversationTree '{dto.Id}' has invalid StartingNodeId '{tree.StartingNodeId}'. " +
            $"No node with that ID exists.");
    }

    return tree;
}

/// <summary>
/// Parse a dialogue node from DTO
/// </summary>
private static DialogueNode ParseNode(DialogueNodeDTO dto, string treeId)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    if (string.IsNullOrEmpty(dto.Id))
        throw new InvalidOperationException($"DialogueNode in tree '{treeId}' missing required 'Id' field");

    if (string.IsNullOrEmpty(dto.NpcDialogue))
        throw new InvalidOperationException($"DialogueNode '{dto.Id}' in tree '{treeId}' missing required 'NpcDialogue' field");

    DialogueNode node = new DialogueNode
    {
        Id = dto.Id,
        NpcDialogue = dto.NpcDialogue
    };

    // Parse responses
    if (dto.Responses != null)
    {
        foreach (DialogueResponseDTO responseDto in dto.Responses)
        {
            DialogueResponse response = ParseResponse(responseDto, treeId, dto.Id);
            node.Responses.Add(response);
        }
    }

    return node;
}

/// <summary>
/// Parse a dialogue response from DTO
/// </summary>
private static DialogueResponse ParseResponse(DialogueResponseDTO dto, string treeId, string nodeId)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    if (string.IsNullOrEmpty(dto.Id))
        throw new InvalidOperationException($"DialogueResponse in node '{nodeId}' (tree '{treeId}') missing required 'Id' field");

    if (string.IsNullOrEmpty(dto.ResponseText))
        throw new InvalidOperationException($"DialogueResponse '{dto.Id}' in node '{nodeId}' (tree '{treeId}') missing required 'ResponseText' field");

    // Parse required stat if present
    PlayerStatType? requiredStat = null;
    if (!string.IsNullOrEmpty(dto.RequiredStat))
    {
        if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, ignoreCase: true, out PlayerStatType parsedStat))
        {
            requiredStat = parsedStat;
        }
        else
        {
            throw new InvalidOperationException(
                $"DialogueResponse '{dto.Id}' has invalid RequiredStat '{dto.RequiredStat}'. " +
                $"Must be one of: {string.Join(", ", Enum.GetNames<PlayerStatType>())}");
        }
    }

    // Use defaults from catalogue if costs not specified
    int focusCost = dto.FocusCost;
    if (focusCost == 0 && dto.FocusCost == 0)
    {
        // Only use default if explicitly set to 0 in JSON (not missing)
        // Most responses should explicitly specify cost
    }

    int timeCost = dto.TimeCost;
    if (timeCost == 0 && dto.TimeCost == 0)
    {
        // Same as focus cost
    }

    DialogueResponse response = new DialogueResponse
    {
        Id = dto.Id,
        ResponseText = dto.ResponseText,
        FocusCost = focusCost,
        TimeCost = timeCost,
        RequiredStat = requiredStat,
        RequiredStatLevel = dto.RequiredStatLevel,
        NextNodeId = dto.NextNodeId,
        RelationshipDelta = dto.RelationshipDelta,
        GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
        SpawnedSituationIds = dto.SpawnedSituationIds ?? new List<string>(),
        EscalatesToSocialChallenge = dto.EscalatesToSocialChallenge,
        SocialChallengeSituationId = dto.SocialChallengeSituationId
    };

    return response;
}
}

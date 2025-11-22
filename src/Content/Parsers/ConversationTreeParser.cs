/// <summary>
/// Parser for converting ConversationTreeDTO to ConversationTree domain model
/// </summary>
public static class ConversationTreeParser
{
    /// <summary>
    /// Convert ConversationTreeDTO to ConversationTree entity
    /// Uses EntityResolver.FindOrCreate for categorical NPC resolution (DDR-006)
    /// </summary>
    public static ConversationTree Parse(ConversationTreeDTO dto, EntityResolver entityResolver)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("ConversationTree missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'Name' field");

        if (dto.ParticipantFilter == null)
            throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'participantFilter' field");

        if (dto.Nodes == null || dto.Nodes.Count == 0)
            throw new InvalidOperationException($"ConversationTree '{dto.Id}' must have at least one node");

        if (string.IsNullOrEmpty(dto.StartingNodeId))
            throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'StartingNodeId' field");

        // EntityResolver.FindOrCreate pattern - categorical entity resolution
        PlacementFilter participantFilter = SceneTemplateParser.ParsePlacementFilter(dto.ParticipantFilter, $"ConversationTree:{dto.Id}");
        NPC npc = entityResolver.FindOrCreateNPC(participantFilter);

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
            // HIGHLANDER: Object reference only (no ID string)
            Npc = npc,  // Categorical resolution via EntityResolver.FindOrCreate
            MinimumRelationship = dto.MinimumRelationship,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            AvailableTimeBlocks = timeBlocks,
            IsRepeatable = dto.IsRepeatable,
            IsCompleted = dto.IsCompleted
        };

        // Parse dialogue nodes
        foreach (DialogueNodeDTO nodeDto in dto.Nodes)
        {
            DialogueNode node = ParseNode(nodeDto, dto.Id);
            tree.Nodes.Add(node);
        }

        // Resolve starting node from ID to object reference
        if (string.IsNullOrEmpty(dto.StartingNodeId))
        {
            throw new InvalidOperationException($"ConversationTree '{dto.Id}' missing required 'StartingNodeId' field");
        }

        tree.StartingNode = tree.Nodes.FirstOrDefault(n => n.Id == dto.StartingNodeId);
        if (tree.StartingNode == null)
        {
            throw new InvalidOperationException(
                $"ConversationTree '{dto.Id}' has invalid StartingNodeId '{dto.StartingNodeId}'. " +
                $"No node with that ID exists.");
        }

        // SECOND PASS: Resolve NextNode object references for all responses
        // After all nodes created, link responses to target nodes
        for (int nodeIdx = 0; nodeIdx < dto.Nodes.Count; nodeIdx++)
        {
            DialogueNodeDTO nodeDto = dto.Nodes[nodeIdx];
            DialogueNode node = tree.Nodes[nodeIdx];

            if (nodeDto.Responses != null)
            {
                for (int respIdx = 0; respIdx < nodeDto.Responses.Count; respIdx++)
                {
                    DialogueResponseDTO responseDto = nodeDto.Responses[respIdx];
                    DialogueResponse response = node.Responses[respIdx];

                    // Resolve NextNodeId → NextNode object reference
                    if (!string.IsNullOrEmpty(responseDto.NextNodeId))
                    {
                        response.NextNode = tree.Nodes.FirstOrDefault(n => n.Id == responseDto.NextNodeId);
                        if (response.NextNode == null)
                        {
                            throw new InvalidOperationException(
                                $"DialogueResponse '{responseDto.Id}' in node '{nodeDto.Id}' has invalid NextNodeId '{responseDto.NextNodeId}'. " +
                                $"No node with that ID exists in tree '{dto.Id}'.");
                        }
                    }
                    // If NextNodeId is null/empty, NextNode remains null (conversation ends)

                    // SpawnedSituations and SocialChallengeSituation resolution requires GameWorld context
                    // These remain unresolved at parse-time, resolved at spawn-time via EntityResolver
                    // or post-loading by PackageLoader with full GameWorld access
                }
            }
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
            RelationshipDelta = dto.RelationshipDelta,
            GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
            EscalatesToSocialChallenge = dto.EscalatesToSocialChallenge
        };

        return response;
    }
}

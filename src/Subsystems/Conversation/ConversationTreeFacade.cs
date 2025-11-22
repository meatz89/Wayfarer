/// <summary>
/// Public facade for conversation tree operations.
/// Handles simple dialogue tree navigation and escalation to tactical Social challenges.
/// This is the public interface for the ConversationTree subsystem.
/// </summary>
public class ConversationTreeFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TokenFacade _tokenFacade;

    public ConversationTreeFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TokenFacade tokenFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
    }

    /// <summary>
    /// Create context for a conversation tree screen
    /// PHASE 4: Accept ConversationTree object instead of ID
    /// </summary>
    public ConversationTreeContext CreateContext(ConversationTree tree)
    {
        if (tree == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = "Conversation tree is null"
            };
        }

        NPC npc = tree.Npc;
        if (npc == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = "NPC not found for conversation tree"
            };
        }

        Player player = _gameWorld.GetPlayer();

        // Check availability conditions
        // HIGHLANDER: Pass NPC object directly, not npc.ID
        int relationship = _tokenFacade.GetTokenCount(npc, ConnectionType.Trust);
        if (relationship < tree.MinimumRelationship)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"Not enough relationship with {npc.Name} (need {tree.MinimumRelationship}, have {relationship})"
            };
        }

        // Check time blocks
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        if (tree.AvailableTimeBlocks.Count > 0 && !tree.AvailableTimeBlocks.Contains(currentTime))
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"{npc.Name} is not available for this conversation right now"
            };
        }

        // Check required knowledge
        foreach (string knowledge in tree.RequiredKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                return new ConversationTreeContext
                {
                    IsValid = false,
                    ErrorMessage = "You don't have the required knowledge for this conversation"
                };
            }
        }

        // Get starting node (HIGHLANDER: Use object reference, not ID lookup)
        DialogueNode startingNode = tree.StartingNode;
        if (startingNode == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = "Conversation tree has invalid starting node"
            };
        }

        return new ConversationTreeContext
        {
            IsValid = true,
            Tree = tree,
            CurrentNode = startingNode,
            Npc = npc,
            CurrentFocus = player.Focus,
            MaxFocus = player.MaxFocus,
            CurrentRelationship = relationship,
            PlayerStats = BuildPlayerStats(player),
            PlayerKnowledge = new List<string>(player.Knowledge),
            LocationName = GetLocationName(),
            TimeDisplay = _timeFacade.GetTimeString()
        };
    }

    /// <summary>
    /// Select a dialogue response and apply outcomes
    /// PHASE 4: Accept object references instead of IDs
    /// </summary>
    public ConversationTreeResult SelectResponse(ConversationTree tree, DialogueNode currentNode, DialogueResponse response)
    {
        if (tree == null)
            return ConversationTreeResult.Failed("Conversation tree is null");

        if (currentNode == null)
            return ConversationTreeResult.Failed("Dialogue node is null");

        if (response == null)
            return ConversationTreeResult.Failed("Response is null");

        Player player = _gameWorld.GetPlayer();

        // Validate resources
        if (player.Focus < response.FocusCost)
            return ConversationTreeResult.Failed($"Not enough Focus (need {response.FocusCost}, have {player.Focus})");

        // Validate stat requirements
        if (response.RequiredStat.HasValue && response.RequiredStatLevel.HasValue)
        {
            int statLevel = response.RequiredStat.Value switch
            {
                PlayerStatType.Insight => player.Insight,
                PlayerStatType.Rapport => player.Rapport,
                PlayerStatType.Authority => player.Authority,
                PlayerStatType.Diplomacy => player.Diplomacy,
                PlayerStatType.Cunning => player.Cunning,
                _ => 0
            };
            if (statLevel < response.RequiredStatLevel.Value)
            {
                return ConversationTreeResult.Failed(
                    $"Requires {response.RequiredStat} level {response.RequiredStatLevel} (you have {statLevel})");
            }
        }

        // Apply costs
        if (response.FocusCost > 0)
        {
            player.Focus -= response.FocusCost;
        }
        if (response.TimeCost > 0)
        {
            _timeFacade.AdvanceSegments(response.TimeCost);
        }

        // Apply relationship changes
        if (response.RelationshipDelta != 0)
        {
            NPC npc = tree.Npc;
            if (npc != null)
            {
                if (response.RelationshipDelta > 0)
                {
                    _tokenFacade.AddTokensToNPC(ConnectionType.Trust, response.RelationshipDelta, npc);
                }
                else
                {
                    _tokenFacade.RemoveTokensFromNPC(ConnectionType.Trust, -response.RelationshipDelta, npc);
                }

                string deltaText = response.RelationshipDelta > 0
                    ? $"+{response.RelationshipDelta}"
                    : response.RelationshipDelta.ToString();
                _messageSystem.AddSystemMessage(
                    $"Relationship with {npc.Name}: {deltaText}",
                    SystemMessageTypes.Info);
            }
        }

        // Grant knowledge
        foreach (string knowledge in response.GrantedKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                player.Knowledge.Add(knowledge);
                _messageSystem.AddSystemMessage($"Learned: {knowledge}", SystemMessageTypes.Info);
            }
        }

        // Spawn situations (HIGHLANDER: Use object references)
        if (response.SpawnedSituations != null)
        {
            foreach (Situation situation in response.SpawnedSituations)
            {
                // TODO: Implement situation spawning logic when situation system is in place
                _messageSystem.AddSystemMessage($"New situation available: {situation.Name}", SystemMessageTypes.Info);
            }
        }

        // Check for escalation to Social challenge (HIGHLANDER: Use object reference)
        if (response.EscalatesToSocialChallenge && response.SocialChallengeSituation != null)
        {
            return ConversationTreeResult.EscalateToChallenge(response.SocialChallengeSituation);
        }

        // Navigate to next node (HIGHLANDER: Use object reference, not ID lookup)
        if (response.NextNode == null)
        {
            // Conversation ends
            if (!tree.IsRepeatable)
            {
                tree.IsCompleted = true;
            }

            return ConversationTreeResult.Completed();
        }
        else
        {
            return ConversationTreeResult.Continue(response.NextNode);
        }
    }

    private Dictionary<PlayerStatType, int> BuildPlayerStats(Player player)
    {
        Dictionary<PlayerStatType, int> stats = new Dictionary<PlayerStatType, int>
        {
            { PlayerStatType.Insight, player.Insight },
            { PlayerStatType.Rapport, player.Rapport },
            { PlayerStatType.Authority, player.Authority },
            { PlayerStatType.Diplomacy, player.Diplomacy },
            { PlayerStatType.Cunning, player.Cunning }
        };

        return stats;
    }

    private string GetLocationName()
    {
        Player player = _gameWorld.GetPlayer();
        return _gameWorld.GetPlayerCurrentLocation()?.Name ?? "Unknown";
    }

    /// <summary>
    /// Get all conversation trees available at a specific location
    /// Checks NPC location, completion status, relationship requirements, knowledge requirements, and time blocks
    /// </summary>
    public List<ConversationTree> GetAvailableTreesAtLocation(Location location)
    {
        Player player = _gameWorld.GetPlayer();
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();

        return _gameWorld.ConversationTrees
            .Where(t =>
            {
                // Check if tree is available (not completed, or repeatable)
                if (t.IsCompleted && !t.IsRepeatable) return false;

                // Find NPC for this tree
                NPC npc = t.Npc;
                if (npc == null) return false;

                // Check if NPC is at this location
                if (npc.Location != location) return false;

                // Check relationship requirement
                // HIGHLANDER: Pass NPC object directly, not npc.ID
                int relationship = _tokenFacade.GetTokenCount(npc, ConnectionType.Trust);
                if (relationship < t.MinimumRelationship) return false;

                // Check time blocks (empty list = always available)
                if (t.AvailableTimeBlocks.Count > 0 && !t.AvailableTimeBlocks.Contains(currentTime))
                    return false;

                // Check knowledge requirements
                if (!t.RequiredKnowledge.All(k => player.Knowledge.Contains(k)))
                    return false;

                return true;
            })
            .ToList();
    }
}

/// <summary>
/// Result of a conversation tree response selection
/// PHASE 4: ID properties replaced with object references
/// </summary>
public class ConversationTreeResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public bool IsComplete { get; set; }
    public bool EscalatesToChallenge { get; set; }
    public Situation ChallengeSituation { get; set; }  // PHASE 4: Object reference instead of ID
    public DialogueNode NextNode { get; set; }

    public static ConversationTreeResult Failed(string message)
    {
        return new ConversationTreeResult { Success = false, Message = message };
    }

    public static ConversationTreeResult Completed()
    {
        return new ConversationTreeResult { Success = true, IsComplete = true };
    }

    public static ConversationTreeResult Continue(DialogueNode nextNode)
    {
        return new ConversationTreeResult { Success = true, NextNode = nextNode };
    }

    public static ConversationTreeResult EscalateToChallenge(Situation situation)
    {
        return new ConversationTreeResult
        {
            Success = true,
            EscalatesToChallenge = true,
            ChallengeSituation = situation
        };
    }
}

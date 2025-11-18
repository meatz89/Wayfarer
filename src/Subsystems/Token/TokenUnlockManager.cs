/// <summary>
/// Manages what gets unlocked with tokens - conversation options, letter categories, special actions.
/// Uses strongly-typed UnlockDefinition objects instead of string-based dictionaries.
/// </summary>
public class TokenUnlockManager
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    private readonly List<UnlockDefinition> _trustUnlocks = new List<UnlockDefinition>
{
    new UnlockDefinition
    {
        Threshold = 3,
        Name = "Personal letters",
        Description = "Unlocked through Trust tokens: Personal letters. Can now carry intimate correspondence.",
        Guidance = "You can now accept personal letters from this NPC, which offer higher rewards."
    },
    new UnlockDefinition
    {
        Threshold = 5,
        Name = "Intimate conversations",
        Description = "Unlocked through Trust tokens: Intimate conversations. Access to deeper personal dialogue.",
        Guidance = "You can now engage in intimate conversations, revealing personal struggles and bonds."
    },
    new UnlockDefinition
    {
        Threshold = 8,
        Name = "Secret sharing",
        Description = "Unlocked through Trust tokens: Secret sharing. Share and receive confidential information.",
        Guidance = "This NPC now trusts you with their secrets and will accept yours in return."
    },
    new UnlockDefinition
    {
        Threshold = 12,
        Name = "Life-changing favors",
        Description = "Unlocked through Trust tokens: Life-changing favors. Major personal commitments available.",
        Guidance = "You can now ask for or offer life-changing favors that reshape destinies."
    }
};

    private readonly List<UnlockDefinition> _diplomacyUnlocks = new List<UnlockDefinition>
{
    new UnlockDefinition
    {
        Threshold = 2,
        Name = "Trade discounts",
        Description = "Unlocked through Diplomacy tokens: Trade discounts. Receive better prices in markets.",
        Guidance = "Your diplomatic relationship grants you favorable pricing."
    },
    new UnlockDefinition
    {
        Threshold = 4,
        Name = "Business letters",
        Description = "Unlocked through Diplomacy tokens: Business letters. Formal commercial correspondence.",
        Guidance = "Business correspondence is now available, providing diplomacy tokens and coin bonuses."
    },
    new UnlockDefinition
    {
        Threshold = 6,
        Name = "Investment opportunities",
        Description = "Unlocked through Diplomacy tokens: Investment opportunities. Access to profitable ventures.",
        Guidance = "This NPC will now offer you investment opportunities in their business ventures."
    },
    new UnlockDefinition
    {
        Threshold = 10,
        Name = "Exclusive contracts",
        Description = "Unlocked through Diplomacy tokens: Exclusive contracts. Major business partnerships.",
        Guidance = "You can now negotiate exclusive contracts with significant financial implications."
    }
};

    private readonly List<UnlockDefinition> _statusUnlocks = new List<UnlockDefinition>
{
    new UnlockDefinition
    {
        Threshold = 2,
        Name = "Formal introductions",
        Description = "Unlocked through Status tokens: Formal introductions. Access to their social circle.",
        Guidance = "This NPC will now introduce you to members of their social circle."
    },
    new UnlockDefinition
    {
        Threshold = 5,
        Name = "Social invitations",
        Description = "Unlocked through Status tokens: Social invitations. Access to exclusive social events.",
        Guidance = "You will now receive invitations to exclusive gatherings and social functions."
    },
    new UnlockDefinition
    {
        Threshold = 8,
        Name = "Noble correspondence",
        Description = "Unlocked through Status tokens: Noble correspondence. Formal noble communication.",
        Guidance = "Noble letters grant status tokens and may unlock new locations."
    },
    new UnlockDefinition
    {
        Threshold = 12,
        Name = "Court influence",
        Description = "Unlocked through Status tokens: Court influence. Sway in political circles.",
        Guidance = "Your standing grants you influence in court politics and noble affairs."
    }
};

    private readonly List<UnlockDefinition> _shadowUnlocks = new List<UnlockDefinition>
{
    new UnlockDefinition
    {
        Threshold = 2,
        Name = "Rumor trading",
        Description = "Unlocked through Shadow tokens: Rumor trading. Exchange of sensitive information.",
        Guidance = "You can now trade rumors and gather intelligence through this NPC."
    },
    new UnlockDefinition
    {
        Threshold = 4,
        Name = "Secret messages",
        Description = "Unlocked through Shadow tokens: Secret messages. Can transport clandestine information.",
        Guidance = "Secret letters are risky but highly rewarding. Handle with care."
    },
    new UnlockDefinition
    {
        Threshold = 7,
        Name = "Blackmail letters",
        Description = "Unlocked through Shadow tokens: Blackmail letters. Leverage sensitive information.",
        Guidance = "You can now engage in blackmail operations through this contact."
    },
    new UnlockDefinition
    {
        Threshold = 10,
        Name = "Criminal contacts",
        Description = "Unlocked through Shadow tokens: Criminal contacts. Access to underground network.",
        Guidance = "This NPC will connect you with criminal elements and illicit opportunities."
    }
};

    private readonly List<RelationshipMilestone> _relationshipMilestones = new List<RelationshipMilestone>
{
    new RelationshipMilestone { Threshold = 5, Name = "Trusted associate" },
    new RelationshipMilestone { Threshold = 10, Name = "Close ally" },
    new RelationshipMilestone { Threshold = 15, Name = "Inner circle" },
    new RelationshipMilestone { Threshold = 20, Name = "Lifelong bond" }
};

    public TokenUnlockManager(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check and process unlocks when tokens are gained
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public void CheckAndProcessUnlocks(NPC npc, ConnectionType tokenType, int newTokenCount)
    {
        if (npc == null || newTokenCount <= 0) return;

        List<UnlockDefinition> unlocks = GetUnlockDefinitions(tokenType);

        foreach (UnlockDefinition unlock in unlocks)
        {
            if (newTokenCount >= unlock.Threshold && (newTokenCount - 1) < unlock.Threshold)
            {
                ProcessUnlock(npc, tokenType, unlock);
            }
        }

        int totalTokens = GetTotalTokensWithNPC(npc);
        CheckRelationshipUnlocks(npc, totalTokens);
    }

    /// <summary>
    /// Get all available unlocks for an NPC based on current tokens
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public List<TokenUnlock> GetAvailableUnlocks(NPC npc)
    {
        List<TokenUnlock> availableUnlocks = new List<TokenUnlock>();
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);

        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type == ConnectionType.None) continue;

            int tokenCount = tokens.GetValueOrDefault(type, 0);
            if (tokenCount <= 0) continue;

            List<UnlockDefinition> unlocks = GetUnlockDefinitions(type);

            foreach (UnlockDefinition unlock in unlocks)
            {
                if (tokenCount >= unlock.Threshold)
                {
                    availableUnlocks.Add(new TokenUnlock
                    {
                        UnlockId = $"unlock_{type}_{unlock.Threshold}",
                        Name = unlock.Name,
                        Description = unlock.Description,
                        Requirement = new TokenRequirement
                        {
                            Npc = npc,
                            TokenType = type,
                            MinimumCount = unlock.Threshold
                        },
                        IsAvailable = true
                    });
                }
            }
        }

        return availableUnlocks;
    }

    /// <summary>
    /// Check if a specific unlock is available
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public bool IsUnlockAvailable(NPC npc, string unlockId)
    {
        List<TokenUnlock> availableUnlocks = GetAvailableUnlocks(npc);
        return availableUnlocks.Any(u => u.UnlockId == unlockId);
    }

    /// <summary>
    /// Get unlock requirements for an NPC
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public Dictionary<string, TokenRequirement> GetUnlockRequirements(NPC npc)
    {
        Dictionary<string, TokenRequirement> requirements = new Dictionary<string, TokenRequirement>();

        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type == ConnectionType.None) continue;

            List<UnlockDefinition> unlocks = GetUnlockDefinitions(type);

            foreach (UnlockDefinition unlock in unlocks)
            {
                string unlockId = $"unlock_{type}_{unlock.Threshold}";
                requirements[unlockId] = new TokenRequirement
                {
                    Npc = npc,
                    TokenType = type,
                    MinimumCount = unlock.Threshold
                };
            }
        }

        return requirements;
    }

    /// <summary>
    /// HIGHLANDER: Accept typed NPC object, extract name for display only
    /// </summary>
    private void ProcessUnlock(NPC npc, ConnectionType tokenType, UnlockDefinition unlock)
    {
        _messageSystem.AddSystemMessage(
            $"New {tokenType} unlock with {npc.Name}: {unlock.Name}",
            SystemMessageTypes.Success,
            MessageCategory.Achievement
        );

        if (!string.IsNullOrEmpty(unlock.Guidance))
        {
            _messageSystem.AddSystemMessage(unlock.Guidance, SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// HIGHLANDER: Accept typed NPC object, extract name for display only
    /// </summary>
    private void CheckRelationshipUnlocks(NPC npc, int totalTokens)
    {
        foreach (RelationshipMilestone milestone in _relationshipMilestones)
        {
            if (totalTokens == milestone.Threshold)
            {
                _messageSystem.AddSystemMessage(
                    $"Relationship milestone with {npc.Name}: {milestone.Name}",
                    SystemMessageTypes.Success,
                    MessageCategory.Achievement
                );
            }
        }
    }

    private List<UnlockDefinition> GetUnlockDefinitions(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => _trustUnlocks,
            ConnectionType.Diplomacy => _diplomacyUnlocks,
            ConnectionType.Status => _statusUnlocks,
            ConnectionType.Shadow => _shadowUnlocks,
            _ => throw new InvalidOperationException($"No unlock definitions for ConnectionType: {type}")
        };
    }

    /// <summary>
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    private int GetTotalTokensWithNPC(NPC npc)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
        return tokens.Values.Where(v => v > 0).Sum();
    }
}

/// <summary>
/// Strongly-typed unlock definition with all properties explicit
/// </summary>
public class UnlockDefinition
{
    public int Threshold { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Guidance { get; init; }
}

/// <summary>
/// Relationship milestone definition
/// </summary>
public class RelationshipMilestone
{
    public int Threshold { get; init; }
    public string Name { get; init; }
}

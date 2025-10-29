/// <summary>
/// Manages what gets unlocked with tokens - conversation options, letter categories, special actions.
/// </summary>
public class TokenUnlockManager
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    // Token thresholds for unlocking content
    private readonly Dictionary<int, string> _trustUnlocks = new Dictionary<int, string>
    {
        { 3, "Personal letters" },
        { 5, "Intimate conversations" },
        { 8, "Secret sharing" },
        { 12, "Life-changing favors" }
    };

    private readonly Dictionary<int, string> _diplomacyUnlocks = new Dictionary<int, string>
    {
        { 2, "Trade discounts" },
        { 4, "Business letters" },
        { 6, "Investment opportunities" },
        { 10, "Exclusive contracts" }
    };

    private readonly Dictionary<int, string> _statusUnlocks = new Dictionary<int, string>
    {
        { 2, "Formal introductions" },
        { 5, "Social invitations" },
        { 8, "Noble correspondence" },
        { 12, "Court influence" }
    };

    private readonly Dictionary<int, string> _shadowUnlocks = new Dictionary<int, string>
    {
        { 2, "Rumor trading" },
        { 4, "Secret messages" },
        { 7, "Blackmail letters" },
        { 10, "Criminal contacts" }
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
    /// </summary>
    public void CheckAndProcessUnlocks(string npcId, ConnectionType tokenType, int newTokenCount)
    {
        if (string.IsNullOrEmpty(npcId) || newTokenCount <= 0) return;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return;

        // Get unlock thresholds for this token type
        Dictionary<int, string> unlockThresholds = GetUnlockThresholds(tokenType);

        // Check each threshold
        foreach (KeyValuePair<int, string> unlock in unlockThresholds)
        {
            int threshold = unlock.Key;
            string unlockName = unlock.Value;

            // Check if we just crossed this threshold
            if (newTokenCount >= threshold && (newTokenCount - 1) < threshold)
            {
                ProcessUnlock(npcId, npc.Name, tokenType, unlockName, threshold);
            }
        }

        // Check total token unlocks (relationship milestones)
        int totalTokens = GetTotalTokensWithNPC(npcId);
        CheckRelationshipUnlocks(npcId, npc.Name, totalTokens);
    }

    /// <summary>
    /// Get all available unlocks for an NPC based on current tokens
    /// </summary>
    public List<TokenUnlock> GetAvailableUnlocks(string npcId)
    {
        List<TokenUnlock> availableUnlocks = new List<TokenUnlock>();
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);

        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type == ConnectionType.None) continue;

            int tokenCount = tokens.GetValueOrDefault(type, 0);
            if (tokenCount <= 0) continue;

            Dictionary<int, string> unlockThresholds = GetUnlockThresholds(type);

            foreach (KeyValuePair<int, string> unlock in unlockThresholds)
            {
                if (tokenCount >= unlock.Key)
                {
                    availableUnlocks.Add(new TokenUnlock
                    {
                        UnlockId = $"{npcId}_{type}_{unlock.Key}",
                        Name = unlock.Value,
                        Description = GetUnlockDescription(type, unlock.Value),
                        Requirement = new TokenRequirement
                        {
                            NPCId = npcId,
                            TokenType = type,
                            MinimumCount = unlock.Key
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
    /// </summary>
    public bool IsUnlockAvailable(string npcId, string unlockId)
    {
        List<TokenUnlock> availableUnlocks = GetAvailableUnlocks(npcId);
        return availableUnlocks.Any(u => u.UnlockId == unlockId);
    }

    /// <summary>
    /// Get unlock requirements for an NPC
    /// </summary>
    public Dictionary<string, TokenRequirement> GetUnlockRequirements(string npcId)
    {
        Dictionary<string, TokenRequirement> requirements = new Dictionary<string, TokenRequirement>();

        foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
        {
            if (type == ConnectionType.None) continue;

            Dictionary<int, string> unlockThresholds = GetUnlockThresholds(type);

            foreach (KeyValuePair<int, string> unlock in unlockThresholds)
            {
                string unlockId = $"{npcId}_{type}_{unlock.Key}";
                requirements[unlockId] = new TokenRequirement
                {
                    NPCId = npcId,
                    TokenType = type,
                    MinimumCount = unlock.Key
                };
            }
        }

        return requirements;
    }

    /// <summary>
    /// Check if player meets requirements for a conversation type
    /// </summary>
    public bool MeetsConversationRequirements(string npcId, string conversationType)
    {
        // Map conversation types to token requirements
        switch (conversationType.ToLower())
        {
            case "intimate":
            case "personal":
                return _tokenManager.GetTokenCount(npcId, ConnectionType.Trust) >= 5;

            case "business":
            case "trade":
                return _tokenManager.GetTokenCount(npcId, ConnectionType.Diplomacy) >= 2;

            case "formal":
            case "noble":
                return _tokenManager.GetTokenCount(npcId, ConnectionType.Status) >= 3;

            case "secret":
            case "clandestine":
                return _tokenManager.GetTokenCount(npcId, ConnectionType.Shadow) >= 2;

            case "standard":
            case "friendlychat":
            case "friendly":
                return true; // Standard/friendly conversations have no requirements

            default:
                throw new InvalidOperationException($"Unknown conversation type: '{conversationType}'. Valid types are: intimate, personal, business, trade, formal, noble, secret, clandestine, standard, friendly");
        }
    }

    /// <summary>
    /// Get available letter categories based on tokens
    /// </summary>
    public List<string> GetAvailableLetterCategories(string npcId)
    {
        List<string> categories = new List<string> { "standard" }; // Always available

        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);

        // Trust unlocks personal letters
        if (tokens.GetValueOrDefault(ConnectionType.Trust, 0) >= 3)
        {
            categories.Add("personal");
        }
        if (tokens.GetValueOrDefault(ConnectionType.Trust, 0) >= 8)
        {
            categories.Add("secret");
        }

        // Diplomacy unlocks business letters
        if (tokens.GetValueOrDefault(ConnectionType.Diplomacy, 0) >= 4)
        {
            categories.Add("business");
            categories.Add("contract");
        }

        // Status unlocks noble letters
        if (tokens.GetValueOrDefault(ConnectionType.Status, 0) >= 5)
        {
            categories.Add("noble");
            categories.Add("invitation");
        }

        // Shadow unlocks clandestine letters
        if (tokens.GetValueOrDefault(ConnectionType.Shadow, 0) >= 4)
        {
            categories.Add("rumor");
            categories.Add("blackmail");
        }

        return categories;
    }

    // ========== PRIVATE HELPER METHODS ==========

    private void ProcessUnlock(string npcId, string npcName, ConnectionType tokenType, string unlockName, int threshold)
    {
        // Store unlock in game world (if we had an unlock tracking system)
        // For now, just announce it

        _messageSystem.AddSystemMessage(
            $"🔓 New {tokenType} unlock with {npcName}: {unlockName}",
            SystemMessageTypes.Success
        );

        // Provide specific guidance based on unlock type
        string guidance = GetUnlockGuidance(tokenType, unlockName);
        if (!string.IsNullOrEmpty(guidance))
        {
            _messageSystem.AddSystemMessage(guidance, SystemMessageTypes.Info);
        }
    }

    private void CheckRelationshipUnlocks(string npcId, string npcName, int totalTokens)
    {
        // Relationship tier unlocks (across all token types)
        Dictionary<int, string> relationshipMilestones = new Dictionary<int, string>
        {
            { 5, "Trusted associate" },
            { 10, "Close ally" },
            { 15, "Inner circle" },
            { 20, "Lifelong bond" }
        };

        foreach (KeyValuePair<int, string> milestone in relationshipMilestones)
        {
            if (totalTokens == milestone.Key)
            {
                _messageSystem.AddSystemMessage(
                    $"💫 Relationship milestone with {npcName}: {milestone.Value}",
                    SystemMessageTypes.Success
                );
            }
        }
    }

    private Dictionary<int, string> GetUnlockThresholds(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.Trust:
                return _trustUnlocks;
            case ConnectionType.Diplomacy:
                return _diplomacyUnlocks;
            case ConnectionType.Status:
                return _statusUnlocks;
            case ConnectionType.Shadow:
                return _shadowUnlocks;
            default:
                throw new InvalidOperationException($"No unlock thresholds defined for ConnectionType: {type}");
        }
    }

    private string GetUnlockDescription(ConnectionType type, string unlockName)
    {
        // Provide detailed descriptions for each unlock
        string baseDescription = $"Unlocked through {type} tokens: {unlockName}";

        // Add specific benefits
        switch (unlockName.ToLower())
        {
            case "personal letters":
                return $"{baseDescription}. Can now carry intimate correspondence.";
            case "trade discounts":
                return $"{baseDescription}. Receive better prices in markets.";
            case "social invitations":
                return $"{baseDescription}. Access to exclusive social events.";
            case "secret messages":
                return $"{baseDescription}. Can transport clandestine information.";
            default:
                throw new InvalidOperationException($"Unknown unlock name: '{unlockName}' for ConnectionType: {type}");
        }
    }

    private string GetUnlockGuidance(ConnectionType type, string unlockName)
    {
        switch (unlockName.ToLower())
        {
            case "personal letters":
                return "You can now accept personal letters from this NPC, which offer higher rewards.";
            case "business letters":
                return "Business correspondence is now available, providing diplomacy tokens and coin bonuses.";
            case "noble correspondence":
                return "Noble letters grant status tokens and may unlock new locations.";
            case "secret messages":
                return "Secret letters are risky but highly rewarding. Handle with care.";
            default:
                throw new InvalidOperationException($"Unknown unlock name for guidance: '{unlockName}' for ConnectionType: {type}");
        }
    }

    private int GetTotalTokensWithNPC(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        return tokens.Values.Where(v => v > 0).Sum();
    }
}

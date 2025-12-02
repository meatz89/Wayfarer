
/// <summary>
/// Manages core token operations - adding, spending, and tracking tokens with NPCs.
/// Tokens are always relational (tied to specific NPCs) and can go negative (debt/leverage).
/// </summary>
public class ConnectionTokenManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly TokenMechanicsManager _tokenManager;

    public ConnectionTokenManager(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        TokenMechanicsManager tokenManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Get all tokens with a specific NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// DOMAIN COLLECTION: Returns List<TokenCount>, queried with LINQ (never Dictionary)
    /// </summary>
    public List<TokenCount> GetTokensWithNPC(NPC npc)
    {
        // HIGHLANDER: Tokens stored directly on NPC, not in Player collection
        return new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = npc.Trust },
            new TokenCount { Type = ConnectionType.Diplomacy, Count = npc.Diplomacy },
            new TokenCount { Type = ConnectionType.Status, Count = npc.Status },
            new TokenCount { Type = ConnectionType.Shadow, Count = npc.Shadow }
        };
    }

    /// <summary>
    /// Get specific token count with an NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public int GetTokenCount(NPC npc, ConnectionType type)
    {
        // HIGHLANDER: Direct property access on NPC
        return npc.GetTokenCount(type);
    }

    /// <summary>
    /// Add tokens to specific NPC relationship
    /// HIGHLANDER: Delegates to TokenMechanicsManager (single source of truth)
    /// </summary>
    public void AddTokensToNPC(ConnectionType type, int count, NPC npc)
    {
        // Delegate to TokenMechanicsManager (HIGHLANDER pattern)
        _tokenManager.AddTokensToNPC(type, count, npc);
    }

    /// <summary>
    /// Spend tokens with specific NPC (can go negative)
    /// HIGHLANDER: Delegates to TokenMechanicsManager (single source of truth)
    /// </summary>
    public bool SpendTokensWithNPC(ConnectionType type, int count, NPC npc)
    {
        // Delegate to TokenMechanicsManager (HIGHLANDER pattern)
        return _tokenManager.SpendTokensWithNPC(type, count, npc);
    }

    /// <summary>
    /// Remove tokens from NPC relationship (for expired letters/damage)
    /// HIGHLANDER: Delegates to TokenMechanicsManager (single source of truth)
    /// </summary>
    public void RemoveTokensFromNPC(ConnectionType type, int count, NPC npc)
    {
        // Delegate to TokenMechanicsManager (HIGHLANDER pattern)
        _tokenManager.RemoveTokensFromNPC(type, count, npc);
    }

    /// <summary>
    /// Check if player has enough tokens (aggregates across all NPCs)
    /// </summary>
    public bool HasTokens(ConnectionType type, int count)
    {
        return GetTotalTokensOfType(type) >= count;
    }

    /// <summary>
    /// Get total tokens of a type (aggregates positive values across all NPCs)
    /// </summary>
    public int GetTotalTokensOfType(ConnectionType type)
    {
        int totalOfType = 0;

        // HIGHLANDER: Iterate NPCs directly from GameWorld
        foreach (NPC npc in _gameWorld.NPCs)
        {
            int tokensWithNpc = npc.GetTokenCount(type);
            if (tokensWithNpc > 0)
            {
                totalOfType += tokensWithNpc;
            }
        }

        return totalOfType;
    }

    /// <summary>
    /// Spend tokens of a specific type from any NPCs that have that type
    /// </summary>
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;

        // Calculate total available across all NPCs
        int totalAvailable = GetTotalTokensOfType(type);
        if (totalAvailable < amount)
        {
            _messageSystem.AddSystemMessage(
                $"Not enough {type} tokens. Need {amount}, have {totalAvailable}.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Deduct from NPCs proportionally (favor NPCs with more tokens)
        int remaining = amount;

        // Sort NPCs by token count (descending) to spend from those with most tokens first
        List<NPC> npcsByTokenCount = _gameWorld.NPCs
            .Where(npc => npc.GetTokenCount(type) > 0)
            .OrderByDescending(npc => npc.GetTokenCount(type))
            .ToList();

        // Spend tokens
        foreach (NPC npc in npcsByTokenCount)
        {
            if (remaining <= 0) break;

            int tokensWithNpc = npc.GetTokenCount(type);
            int toSpend = Math.Min(tokensWithNpc, remaining);

            npc.SetTokenCount(type, tokensWithNpc - toSpend);
            remaining -= toSpend;

            // Add narrative feedback for each NPC
            if (toSpend > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"Called in {toSpend} {type} favor{(toSpend > 1 ? "s" : "")} with {npc.Name}",
                    SystemMessageTypes.Info
                );
            }
        }

        return true;
    }

    /// <summary>
    /// Get leverage an NPC has over the player (negative tokens)
    /// HIGHLANDER: Accept typed NPC object
    /// DOMAIN COLLECTION: Query List with LINQ
    /// </summary>
    public int GetLeverage(NPC npc, ConnectionType type)
    {
        if (npc == null) return 0;

        List<TokenCount> tokens = GetTokensWithNPC(npc);
        int tokenCount = tokens.FirstOrDefault(t => t.Type == type)?.Count ?? 0;

        // Return absolute value if negative, 0 otherwise
        return tokenCount < 0 ? Math.Abs(tokenCount) : 0;
    }

    /// <summary>
    /// Get all NPCs with whom the player has tokens (positive or negative)
    /// HIGHLANDER: Return List of NPC objects, not string IDs
    /// </summary>
    public List<NPC> GetNPCsWithTokens()
    {
        List<NPC> npcsWithTokens = new List<NPC>();

        // HIGHLANDER: Iterate NPCs directly from GameWorld
        foreach (NPC npc in _gameWorld.NPCs)
        {
            // Check if NPC has any non-zero tokens
            if (npc.Trust != 0 || npc.Diplomacy != 0 || npc.Status != 0 || npc.Shadow != 0)
            {
                npcsWithTokens.Add(npc);
            }
        }

        return npcsWithTokens;
    }
}

/// <summary>
/// Represents a token count for a specific connection type
/// </summary>
public class TokenCount
{
    public ConnectionType Type { get; set; }
    public int Count { get; set; }
}
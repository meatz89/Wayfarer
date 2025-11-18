
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
    /// </summary>
    public List<TokenCount> GetTokensWithNPC(NPC npc)
    {
        Player player = _gameWorld.GetPlayer();
        List<NPCTokenEntry> npcTokens = player.NPCTokens;

        // HIGHLANDER: Compare NPC objects directly
        NPCTokenEntry entry = npcTokens.FirstOrDefault(x => x.Npc == npc);
        if (entry != null)
        {
            // Build list from properties
            return new List<TokenCount>
            {
                new TokenCount { Type = ConnectionType.Trust, Count = entry.Trust },
                new TokenCount { Type = ConnectionType.Diplomacy, Count = entry.Diplomacy },
                new TokenCount { Type = ConnectionType.Status, Count = entry.Status },
                new TokenCount { Type = ConnectionType.Shadow, Count = entry.Shadow }
            };
        }

        // Return empty list if no tokens with this NPC
        List<TokenCount> emptyTokens = new List<TokenCount>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            emptyTokens.Add(new TokenCount { Type = tokenType, Count = 0 });
        }
        return emptyTokens;
    }

    /// <summary>
    /// Get specific token count with an NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public int GetTokenCount(NPC npc, ConnectionType type)
    {
        List<TokenCount> tokens = GetTokensWithNPC(npc);
        TokenCount token = tokens.FirstOrDefault(t => t.Type == type);
        return token?.Count ?? 0;
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
        Player player = _gameWorld.GetPlayer();
        int totalOfType = 0;

        foreach (NPCTokenEntry entry in player.NPCTokens)
        {
            int tokensWithNpc = entry.GetTokenCount(type);
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

        Player player = _gameWorld.GetPlayer();

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
        List<NPCTokenEntry> npcTokens = player.NPCTokens;
        int remaining = amount;

        // Sort NPCs by token count (descending) to spend from those with most tokens first
        List<NPCTokenEntry> npcsByTokenCount = npcTokens
            .Where(entry => entry.GetTokenCount(type) > 0)
            .OrderByDescending(entry => entry.GetTokenCount(type))
            .ToList();

        // Spend tokens
        foreach (NPCTokenEntry npcEntry in npcsByTokenCount)
        {
            if (remaining <= 0) break;

            int tokensWithNpc = npcEntry.GetTokenCount(type);
            int toSpend = Math.Min(tokensWithNpc, remaining);

            npcEntry.SetTokenCount(type, tokensWithNpc - toSpend);
            remaining -= toSpend;

            // Add narrative feedback for each NPC
            NPC npc = _npcRepository.GetById(npcEntry.NpcId);
            if (npc != null && toSpend > 0)
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
    /// </summary>
    public int GetLeverage(string npcId, ConnectionType type)
    {
        if (string.IsNullOrEmpty(npcId)) return 0;

        List<TokenCount> tokens = GetTokensWithNPC(npcId);
        TokenCount token = tokens.FirstOrDefault(t => t.Type == type);
        int tokenCount = token?.Count ?? 0;

        // Return absolute value if negative, 0 otherwise
        return tokenCount < 0 ? Math.Abs(tokenCount) : 0;
    }

    /// <summary>
    /// Get all NPCs with whom the player has tokens (positive or negative)
    /// </summary>
    public List<string> GetNPCsWithTokens()
    {
        Player player = _gameWorld.GetPlayer();
        List<string> npcsWithTokens = new List<string>();

        foreach (NPCTokenEntry entry in player.NPCTokens)
        {
            // Check if NPC has any non-zero tokens
            if (entry.Trust != 0 || entry.Diplomacy != 0 || entry.Status != 0 || entry.Shadow != 0)
            {
                npcsWithTokens.Add(entry.NpcId);
            }
        }

        return npcsWithTokens;
    }

    /// <summary>
    /// Ensure NPC token tracking is initialized
    /// </summary>
    private void EnsureNPCTokensInitialized(string npcId)
    {
        Player player = _gameWorld.GetPlayer();

        if (!player.NPCTokens.Any(t => t.NpcId == npcId))
        {
            // The GetNPCTokenEntry method will create a new entry if needed
            player.GetNPCTokenEntry(npcId);
        }
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
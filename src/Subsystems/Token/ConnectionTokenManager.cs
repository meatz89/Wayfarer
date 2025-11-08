
/// <summary>
/// Manages core token operations - adding, spending, and tracking tokens with NPCs.
/// Tokens are always relational (tied to specific NPCs) and can go negative (debt/leverage).
/// </summary>
public class ConnectionTokenManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;

    public ConnectionTokenManager(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        NPCRepository npcRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
    }

    /// <summary>
    /// Get all tokens with a specific NPC
    /// </summary>
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        Player player = _gameWorld.GetPlayer();
        List<NPCTokenEntry> npcTokens = player.NPCTokens;

        NPCTokenEntry? entry = npcTokens.FirstOrDefault(x => x.NpcId == npcId);
        if (entry != null)
        {
            // Build dictionary from properties
            Dictionary<ConnectionType, int> tokens = new Dictionary<ConnectionType, int>
            {
                [ConnectionType.Trust] = entry.Trust,
                [ConnectionType.Diplomacy] = entry.Diplomacy,
                [ConnectionType.Status] = entry.Status
            };
            return tokens;
        }

        // Return empty dictionary if no tokens with this NPC
        Dictionary<ConnectionType, int> emptyTokens = new Dictionary<ConnectionType, int>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            emptyTokens[tokenType] = 0;
        }
        return emptyTokens;
    }

    /// <summary>
    /// Get specific token count with an NPC
    /// </summary>
    public int GetTokenCount(string npcId, ConnectionType type)
    {
        Dictionary<ConnectionType, int> tokens = GetTokensWithNPC(npcId);
        return tokens.GetValueOrDefault(type, 0);
    }

    /// <summary>
    /// Add tokens to specific NPC relationship
    /// </summary>
    public void AddTokensToNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        List<NPCTokenEntry> npcTokens = player.NPCTokens;

        // Initialize NPC token tracking if needed
        EnsureNPCTokensInitialized(npcId);

        // Get or create NPC token entry
        NPCTokenEntry npcEntry = player.GetNPCTokenEntry(npcId);

        // Track old token count for messaging
        int oldTokenCount = npcEntry.GetTokenCount(type);

        // Update NPC-specific tokens
        npcEntry.SetTokenCount(type, oldTokenCount + count);
        int newTokenCount = npcEntry.GetTokenCount(type);

        // Get NPC for narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"🤝 +{count} {type} token{(count > 1 ? "s" : "")} with {npc.Name} (Total: {newTokenCount})",
                SystemMessageTypes.Success
            );

            // Special message if this cleared a debt
            if (oldTokenCount < 0 && newTokenCount >= 0)
            {
                _messageSystem.AddSystemMessage(
                    $"You've cleared your debt with {npc.Name}.",
                    SystemMessageTypes.Success
                );
            }
        }
    }

    /// <summary>
    /// Spend tokens with specific NPC (can go negative)
    /// </summary>
    public bool SpendTokensWithNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0) return true;

        Player player = _gameWorld.GetPlayer();
        EnsureNPCTokensInitialized(npcId);

        // Get current token count
        int currentCount = player.GetNPCTokenCount(npcId, type);

        // Reduce from NPC relationship (can go negative)
        player.SetNPCTokenCount(npcId, type, currentCount - count);
        int newCount = player.GetNPCTokenCount(npcId, type);

        // Add narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"You call in {count} {type} favor{(count > 1 ? "s" : "")} with {npc.Name}.",
                SystemMessageTypes.Info
            );

            if (newCount < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"You now owe {npc.Name} for this favor.",
                    SystemMessageTypes.Warning
                );
            }
        }

        return true;
    }

    /// <summary>
    /// Remove tokens from NPC relationship (for expired letters/damage)
    /// </summary>
    public void RemoveTokensFromNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        EnsureNPCTokensInitialized(npcId);

        // Track old count for messaging
        int oldCount = player.GetNPCTokenCount(npcId, type);

        // Remove tokens from NPC relationship (can go negative)
        player.SetNPCTokenCount(npcId, type, oldCount - count);
        int newCount = player.GetNPCTokenCount(npcId, type);

        // Add narrative feedback for relationship damage
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"Your relationship with {npc.Name} has been damaged. (-{count} {type} token{(count > 1 ? "s" : "")})",
                SystemMessageTypes.Warning
            );

            if (oldCount >= 0 && newCount < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"{npc.Name} feels you owe them for past failures.",
                    SystemMessageTypes.Danger
                );
            }
        }
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

        Dictionary<ConnectionType, int> tokens = GetTokensWithNPC(npcId);
        int tokenCount = tokens.GetValueOrDefault(type, 0);

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
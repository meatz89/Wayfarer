public class TokenMechanicsManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;

    public TokenMechanicsManager(GameWorld gameWorld, MessageSystem messageSystem, NPCRepository npcRepository, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Get starting connection state based on token count
    /// Replaces the old threshold unlocking system
    /// HIGHLANDER: Accepts NPC object, not string npcId
    /// </summary>
    public ConnectionState GetStartingConnectionState(NPC npc, ConnectionType tokenType)
    {
        int tokens = GetTokenCount(tokenType, npc);
        return tokens switch
        {
            0 => ConnectionState.DISCONNECTED,
            <= 2 => ConnectionState.GUARDED,
            <= 5 => ConnectionState.NEUTRAL,
            <= 9 => ConnectionState.RECEPTIVE,
            _ => ConnectionState.TRUSTING
        };
    }

    /// <summary>
    /// Get tokens with specific NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// DOMAIN COLLECTION: Returns List<TokenCount>, queried with LINQ (never Dictionary)
    /// </summary>
    public List<TokenCount> GetTokensWithNPC(NPC npc)
    {
        // HIGHLANDER: Tokens stored directly on NPC
        return new List<TokenCount>
        {
            new TokenCount { Type = ConnectionType.Trust, Count = npc.Trust },
            new TokenCount { Type = ConnectionType.Diplomacy, Count = npc.Diplomacy },
            new TokenCount { Type = ConnectionType.Status, Count = npc.Status },
            new TokenCount { Type = ConnectionType.Shadow, Count = npc.Shadow }
        };
    }

    // Add tokens to specific NPC relationship
    // HIGHLANDER: Object reference ONLY, no ID string parameter
    public void AddTokensToNPC(ConnectionType type, int count, NPC npc)
    {
        if (count <= 0 || npc == null) return;

        Player player = _gameWorld.GetPlayer();

        // Apply equipment bonuses (flat integer additions)
        int equipmentBonus = GetEquipmentTokenBonus(type);
        int modifiedCount = count + equipmentBonus;

        // Track old token count for category unlock checking
        int oldTokenCount = npc.GetTokenCount(type);

        // Update NPC tokens with modified amount
        npc.SetTokenCount(type, oldTokenCount + modifiedCount);
        int newTokenCount = npc.GetTokenCount(type);

        // Narrative feedback using NPC object directly (no GetById needed)
        string bonusText = modifiedCount > count ? $" (+{modifiedCount - count} from equipment)" : "";
        _messageSystem.AddSystemMessage(
            $"🤝 +{modifiedCount} {type} token{(modifiedCount > 1 ? "s" : "")} with {npc.Name}{bonusText} (Total: {newTokenCount})",
            SystemMessageTypes.Success
        );

        // Check relationship milestones
        int totalWithNPC = npc.GetTotalTokens();
        CheckRelationshipMilestone(npc, totalWithNPC);

        // Category service removed - letters created through conversation choices only

        // Token change notifications are handled by GameOrchestrator orchestration
    }

    // Spend tokens with specific NPC context (for queue manipulation)
    // HIGHLANDER: Object reference ONLY, no ID string parameter
    public bool SpendTokensWithNPC(ConnectionType type, int count, NPC npc)
    {
        if (count <= 0) return true;
        if (npc == null) return false;

        // Get current token count
        int currentCount = npc.GetTokenCount(type);

        // reduce from NPC relationship (can go negative)
        npc.SetTokenCount(type, currentCount - count);

        // Narrative feedback using NPC object directly (no GetById needed)
        _messageSystem.AddSystemMessage(
            $"You call in {count} {type} favor{(count > 1 ? "s" : "")} with {npc.Name}.",
            SystemMessageTypes.Info
        );

        if (npc.GetTokenCount(type) < 0)
        {
            _messageSystem.AddSystemMessage(
                $"You now owe {npc.Name} for this favor.",
                SystemMessageTypes.Warning
            );
        }

        return true;
    }

    // Check if player has enough tokens (aggregates across all NPCs)
    public bool HasTokens(ConnectionType type, int count)
    {
        int totalOfType = 0;
        foreach (NPC npc in _gameWorld.NPCs)
        {
            int tokensWithNpc = npc.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType >= count;
    }

    // Get total tokens of a type (aggregates across all NPCs)
    public int GetTokenCount(ConnectionType type)
    {
        int totalOfType = 0;
        foreach (NPC npc in _gameWorld.NPCs)
        {
            int tokensWithNpc = npc.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType;
    }

    /// <summary>
    /// Get tokens of a specific type with a specific NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// DOMAIN COLLECTION: Query List with LINQ
    /// </summary>
    public int GetTokenCount(ConnectionType type, NPC npc)
    {
        List<TokenCount> tokens = GetTokensWithNPC(npc);
        return tokens.FirstOrDefault(t => t.Type == type)?.Count ?? 0;
    }

    /// <summary>
    /// Leverage Mechanic: Negative tokens represent leverage the NPC has over the player
    /// HIGHLANDER: Accepts NPC object, not string ID
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

    // Remove tokens from NPC relationship (for expired letters)
    // HIGHLANDER: Object reference ONLY, no ID string parameter
    public void RemoveTokensFromNPC(ConnectionType type, int count, NPC npc)
    {
        if (count <= 0 || npc == null) return;

        // Track old count for obligation checking
        int oldCount = npc.GetTokenCount(type);

        // Remove tokens from NPC relationship (can go negative)
        npc.SetTokenCount(type, oldCount - count);

        // Narrative feedback using NPC object directly (no GetById needed)
        _messageSystem.AddSystemMessage(
            $"Your relationship with {npc.Name} has been damaged. (-{count} {type} token{(count > 1 ? "s" : "")})",
            SystemMessageTypes.Warning
        );

        if (npc.GetTokenCount(type) < 0)
        {
            _messageSystem.AddSystemMessage(
                $"{npc.Name} feels you owe them for past failures.",
                SystemMessageTypes.Danger
            );
        }

        // Token change notifications are handled by GameOrchestrator orchestration
    }

    // Get total tokens of a specific type across all NPCs
    public int GetTotalTokensOfType(ConnectionType type)
    {
        int totalOfType = 0;
        foreach (NPC npc in _gameWorld.NPCs)
        {
            int tokensWithNpc = npc.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType;
    }

    // Spend tokens of a specific type from any NPCs that have that type
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;

        // Calculate total available across all NPCs
        int totalAvailable = GetTotalTokensOfType(type);
        if (totalAvailable < amount)
            return false;

        // Deduct from NPCs proportionally
        int remaining = amount;

        foreach (NPC npc in _gameWorld.NPCs)
        {
            if (remaining <= 0) break;

            int tokensWithNpc = npc.GetTokenCount(type);

            if (tokensWithNpc > 0)
            {
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
        }

        return true;
    }

    // Spend tokens from a specific NPC
    // HIGHLANDER: Accepts NPC object, not string ID
    public bool SpendTokens(ConnectionType type, int amount, NPC npc)
    {
        if (amount <= 0) return true;
        if (npc == null)
        {
            _messageSystem.AddSystemMessage(
                "ERROR: Cannot spend tokens without specifying NPC. Tokens are relational.",
                SystemMessageTypes.Danger
            );
            return false;
        }

        int tokensOfType = npc.GetTokenCount(type);

        if (tokensOfType < amount)
            return false;

        // Deduct tokens directly from NPC
        npc.SetTokenCount(type, tokensOfType - amount);

        // Add narrative feedback
        _messageSystem.AddSystemMessage(
            $"Spent {amount} {type} token{(amount > 1 ? "s" : "")} with {npc.Name}",
            SystemMessageTypes.Info
        );

        return true;
    }

    // Helper method to check and announce relationship milestones
    // DOMAIN COLLECTION PRINCIPLE: Use switch expression instead of Dictionary lookup
    private void CheckRelationshipMilestone(NPC npc, int totalTokens)
    {
        string message = totalTokens switch
        {
            3 => $"{npc.Name} now trusts you enough to share private correspondence.",
            5 => $"Your bond with {npc.Name} has deepened. They'll offer more valuable letters.",
            8 => $"{npc.Name} considers you among their most trusted associates. Premium letters are now available.",
            12 => $"Few people enjoy the level of trust {npc.Name} has in you.",
            _ => null
        };

        if (message != null)
        {
            _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Get the token generation bonus from equipped items (flat integer additions)
    /// </summary>
    private int GetEquipmentTokenBonus(ConnectionType tokenType)
    {
        if (_itemRepository == null) return 0;

        Player player = _gameWorld.GetPlayer();
        int totalBonus = 0;

        // Check all items in inventory for token bonuses
        // DOMAIN COLLECTION PRINCIPLE: Use explicit properties on Item
        foreach (Item item in player.Inventory.GetAllItems())
        {
            if (item == null) continue;
            // Add bonuses together (e.g., +1 from item A + +2 from item B = +3 total)
            totalBonus = totalBonus + item.GetTokenGenerationBonus(tokenType);
        }

        return totalBonus;
    }

}
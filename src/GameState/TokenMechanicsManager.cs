using System;
using System.Collections.Generic;
using System.Linq;
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
    /// </summary>
    public ConnectionState GetStartingConnectionState(string npcId, ConnectionType tokenType)
    {
        int tokens = GetTokenCount(tokenType, npcId);
        return tokens switch
        {
            0 => ConnectionState.DISCONNECTED,
            <= 2 => ConnectionState.GUARDED,
            <= 5 => ConnectionState.NEUTRAL,
            <= 9 => ConnectionState.RECEPTIVE,
            _ => ConnectionState.TRUSTING
        };
    }

    // Get tokens with specific NPC - returns a new dictionary for compatibility
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        List<NPCTokenEntry> npcTokens = _gameWorld.GetPlayer().NPCTokens;
        NPCTokenEntry tokenEntry = npcTokens.FirstOrDefault(nt => nt.NpcId == npcId);

        if (tokenEntry != null)
        {
            return new Dictionary<ConnectionType, int>
            {
                [ConnectionType.Trust] = tokenEntry.Trust,
                [ConnectionType.Diplomacy] = tokenEntry.Diplomacy,
                [ConnectionType.Status] = tokenEntry.Status,
                [ConnectionType.Shadow] = tokenEntry.Shadow
            };
        }

        // Return empty dictionary if no tokens with this NPC
        Dictionary<ConnectionType, int> emptyTokens = new Dictionary<ConnectionType, int>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            emptyTokens[tokenType] = 0;
        }
        return emptyTokens;
    }

    // Add tokens to specific NPC relationship
    public void AddTokensToNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        List<NPCTokenEntry> npcTokens = player.NPCTokens;

        // Apply equipment modifiers
        float modifier = GetEquipmentTokenModifier(type);
        int modifiedCount = (int)Math.Ceiling(count * modifier);

        // Initialize NPC token tracking if needed
        NPCTokenEntry npcEntry = npcTokens.GetNPCTokenEntry(npcId);

        // Track old token count for category unlock checking
        int oldTokenCount = npcEntry.GetTokenCount(type);

        // Update NPC-specific tokens with modified amount
        npcEntry.SetTokenCount(type, oldTokenCount + modifiedCount);
        int newTokenCount = npcEntry.GetTokenCount(type);

        // Get NPC for narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            string bonusText = modifiedCount > count ? $" (+{modifiedCount - count} from equipment)" : "";
            _messageSystem.AddSystemMessage(
                $"🤝 +{modifiedCount} {type} token{(modifiedCount > 1 ? "s" : "")} with {npc.Name}{bonusText} (Total: {newTokenCount})",
                SystemMessageTypes.Success
            );

            // Check relationship milestones
            int totalWithNPC = npcEntry.Trust + npcEntry.Diplomacy + npcEntry.Status + npcEntry.Shadow;
            CheckRelationshipMilestone(npc, totalWithNPC);

            // Category service removed - letters created through conversation choices only
        }

        // Token change notifications are handled by GameFacade orchestration
    }


    // Spend tokens with specific NPC context (for queue manipulation)
    public bool SpendTokensWithNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0) return true;

        Player player = _gameWorld.GetPlayer();

        // Get or create NPC token entry
        NPCTokenEntry npcEntry = player.NPCTokens.GetNPCTokenEntry(npcId);
        int currentCount = npcEntry.GetTokenCount(type);

        // reduce from NPC relationship (can go negative)
        npcEntry.SetTokenCount(type, currentCount - count);

        // Add narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"You call in {count} {type} favor{(count > 1 ? "s" : "")} with {npc.Name}.",
                SystemMessageTypes.Info
            );

            if (npcEntry.GetTokenCount(type) < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"You now owe {npc.Name} for this favor.",
                    SystemMessageTypes.Warning
                );
            }
        }

        return true;
    }

    // Check if player has enough tokens (aggregates across all NPCs)
    public bool HasTokens(ConnectionType type, int count)
    {
        Player player = _gameWorld.GetPlayer();
        int totalOfType = 0;
        foreach (NPCTokenEntry entry in player.NPCTokens)
        {
            int tokensWithNpc = entry.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType >= count;
    }

    // Get total tokens of a type (aggregates across all NPCs)
    public int GetTokenCount(ConnectionType type)
    {
        Player player = _gameWorld.GetPlayer();
        int totalOfType = 0;
        foreach (NPCTokenEntry entry in player.NPCTokens)
        {
            int tokensWithNpc = entry.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType;
    }

    // Get tokens of a specific type with a specific NPC
    public int GetTokenCount(ConnectionType type, string npcId)
    {
        Dictionary<ConnectionType, int> tokensWithNPC = GetTokensWithNPC(npcId);
        return tokensWithNPC.GetValueOrDefault(type, 0);
    }


    /// <summary>
    /// Leverage Mechanic: Negative tokens represent leverage the NPC has over the player
    /// </summary>
    public int GetLeverage(string npcId, ConnectionType type)
    {
        if (string.IsNullOrEmpty(npcId)) return 0;

        Dictionary<ConnectionType, int> tokens = GetTokensWithNPC(npcId);
        int tokenCount = tokens.GetValueOrDefault(type, 0);

        // Return absolute value if negative, 0 otherwise
        return tokenCount < 0 ? Math.Abs(tokenCount) : 0;
    }

    // Remove tokens from NPC relationship (for expired letters)
    public void RemoveTokensFromNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        List<NPCTokenEntry> npcTokens = player.NPCTokens;

        // Get or create NPC token entry
        NPCTokenEntry npcEntry = npcTokens.GetNPCTokenEntry(npcId);

        // Track old count for obligation checking
        int oldCount = npcEntry.GetTokenCount(type);

        // Remove tokens from NPC relationship (can go negative)
        npcEntry.SetTokenCount(type, oldCount - count);

        // Add narrative feedback for relationship damage
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"Your relationship with {npc.Name} has been damaged. (-{count} {type} token{(count > 1 ? "s" : "")})",
                SystemMessageTypes.Warning
            );

            if (npcEntry.GetTokenCount(type) < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"{npc.Name} feels you owe them for past failures.",
                    SystemMessageTypes.Danger
                );
            }
        }

        // Token change notifications are handled by GameFacade orchestration
    }

    // Get total tokens of a specific type across all NPCs
    public int GetTotalTokensOfType(ConnectionType type)
    {
        Player player = _gameWorld.GetPlayer();
        int totalOfType = 0;
        foreach (NPCTokenEntry entry in player.NPCTokens)
        {
            int tokensWithNpc = entry.GetTokenCount(type);
            if (tokensWithNpc > 0) totalOfType += tokensWithNpc;
        }
        return totalOfType;
    }

    // Spend tokens of a specific type from any NPCs that have that type
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;

        Player player = _gameWorld.GetPlayer();

        // Calculate total available across all NPCs
        int totalAvailable = GetTotalTokensOfType(type);
        if (totalAvailable < amount)
            return false;

        // Deduct from NPCs proportionally
        List<NPCTokenEntry> npcTokens = player.NPCTokens;
        int remaining = amount;

        foreach (NPCTokenEntry npcEntry in npcTokens)
        {
            if (remaining <= 0) break;

            int tokensWithNpc = npcEntry.GetTokenCount(type);

            if (tokensWithNpc > 0)
            {
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
        }

        return true;
    }

    // Spend tokens from a specific NPC
    public bool SpendTokens(ConnectionType type, int amount, string npcId)
    {
        if (amount <= 0) return true;
        if (string.IsNullOrEmpty(npcId))
        {
            _messageSystem.AddSystemMessage(
                "ERROR: Cannot spend tokens without specifying NPC. Tokens are relational.",
                SystemMessageTypes.Danger
            );
            return false;
        }

        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> npcTokens = GetTokensWithNPC(npcId);
        int tokensOfType = npcTokens.GetValueOrDefault(type, 0);

        if (tokensOfType < amount)
            return false;

        // Get or create NPC token entry and deduct tokens
        NPCTokenEntry npcEntry = player.NPCTokens.GetNPCTokenEntry(npcId);
        npcEntry.SetTokenCount(type, tokensOfType - amount);

        // Add narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"Spent {amount} {type} token{(amount > 1 ? "s" : "")} with {npc.Name}",
                SystemMessageTypes.Info
            );
        }

        return true;
    }

    // Helper method to check and announce relationship milestones
    private void CheckRelationshipMilestone(NPC npc, int totalTokens)
    {
        Dictionary<int, string> milestones = new Dictionary<int, string>
        {
            { 3, $"{npc.Name} now trusts you enough to share private correspondence." },
            { 5, $"Your bond with {npc.Name} has deepened. They'll offer more valuable letters." },
            { 8, $"{npc.Name} considers you among their most trusted associates. Premium letters are now available." },
            { 12, $"Few people enjoy the level of trust {npc.Name} has in you." }
        };

        if (milestones.TryGetValue(totalTokens, out string? message))
        {
            _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Get the token generation modifier from equipped items
    /// </summary>
    private float GetEquipmentTokenModifier(ConnectionType tokenType)
    {
        if (_itemRepository == null) return 1.0f;

        Player player = _gameWorld.GetPlayer();
        float totalModifier = 1.0f;

        // Check all items in inventory for token modifiers
        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (string.IsNullOrEmpty(itemId)) continue;

            Item item = _itemRepository.GetItemById(itemId);
            if (item != null && item.TokenGenerationModifiers != null &&
                item.TokenGenerationModifiers.TryGetValue(tokenType, out float modifier))
            {
                // Multiply modifiers (e.g., 1.5 * 1.2 = 1.8 for +50% and +20%)
                totalModifier *= modifier;
            }
        }

        return totalModifier;
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
public class ConnectionTokenManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    private LetterCategoryService _categoryService;
    private StandingObligationManager _obligationManager;

    public ConnectionTokenManager(GameWorld gameWorld, MessageSystem messageSystem, NPCRepository npcRepository, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
    }

    public void SetCategoryService(LetterCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public void SetObligationManager(StandingObligationManager obligationManager)
    {
        _obligationManager = obligationManager;
    }

    // Get player's total tokens by type
    public Dictionary<ConnectionType, int> GetPlayerTokens()
    {
        return _gameWorld.GetPlayer().ConnectionTokens;
    }

    // Get tokens with specific NPC
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        Dictionary<string, Dictionary<ConnectionType, int>> npcTokens = _gameWorld.GetPlayer().NPCTokens;
        if (npcTokens.ContainsKey(npcId))
            return npcTokens[npcId];

        // Return empty dictionary if no tokens with this NPC
        Dictionary<ConnectionType, int> emptyTokens = new Dictionary<ConnectionType, int>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            emptyTokens[tokenType] = 0;
        }
        return emptyTokens;
    }
    
    // Get tokens with specific NPC as strongly typed object
    public NPCTokenBalance GetNPCTokenBalance(string npcId)
    {
        var tokenDict = GetTokensWithNPC(npcId);
        var balance = new NPCTokenBalance();
        
        foreach (var kvp in tokenDict)
        {
            balance.Balances.Add(new TokenBalance 
            { 
                TokenType = kvp.Key, 
                Amount = kvp.Value 
            });
        }
        
        return balance;
    }

    // Add tokens through Socialize (1 hour → 1 token) or Delivery (1 stamina → 1 token)
    // This method is kept for backward compatibility but should not be used for new commands
    public void AddTokensFromSocialize(string npcId)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return;

        // Socialize gives 1 token of NPC's primary type
        ConnectionType tokenType = npc.LetterTokenTypes.FirstOrDefault();
        if (tokenType == default) return;

        AddTokensToNPC(tokenType, 1, npcId);
    }
    
    // Add tokens with specific type (used by context-aware commands)
    public void AddTokensFromAction(string npcId, ConnectionType tokenType, int count = 1)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return;

        AddTokensToNPC(tokenType, count, npcId);
    }

    // Add tokens to specific NPC relationship
    public void AddTokensToNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> playerTokens = player.ConnectionTokens;
        Dictionary<string, Dictionary<ConnectionType, int>> npcTokens = player.NPCTokens;
        
        // Apply equipment modifiers
        float modifier = GetEquipmentTokenModifier(type);
        int modifiedCount = (int)Math.Ceiling(count * modifier);

        // Update global token count with modified amount
        playerTokens[type] = playerTokens.GetValueOrDefault(type) + modifiedCount;

        // Initialize NPC token tracking if needed
        if (!npcTokens.ContainsKey(npcId))
        {
            npcTokens[npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                npcTokens[npcId][tokenType] = 0;
            }
        }

        // Track old token count for category unlock checking
        int oldTokenCount = npcTokens[npcId][type];

        // Update NPC-specific tokens with modified amount
        npcTokens[npcId][type] += modifiedCount;
        int newTokenCount = npcTokens[npcId][type];

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
            int totalWithNPC = npcTokens[npcId].Values.Sum();
            CheckRelationshipMilestone(npc, totalWithNPC);

            // Check category unlocks if we have the service
            if (_categoryService != null && npc.LetterTokenTypes.Contains(type))
            {
                _categoryService.CheckCategoryUnlock(npcId, type, oldTokenCount, newTokenCount);
            }
        }

        // Check threshold-based obligations if we have the manager
        if (_obligationManager != null)
        {
            _obligationManager.OnTokensChanged(npcId, type, oldTokenCount, newTokenCount);
        }
    }

    // Spend tokens (for queue actions)
    public bool SpendTokens(ConnectionType type, int count)
    {
        if (count <= 0) return true;

        Dictionary<ConnectionType, int> playerTokens = _gameWorld.GetPlayer().ConnectionTokens;
        if (playerTokens.GetValueOrDefault(type) >= count)
        {
            playerTokens[type] -= count;

            _messageSystem.AddSystemMessage(
                $"💰 Spent {count} {type} token{(count > 1 ? "s" : "")} (Remaining: {playerTokens[type]})",
                SystemMessageTypes.Info
            );

            return true;
        }

        _messageSystem.AddSystemMessage(
            $"❌ Insufficient {type} tokens! Need {count}, have {playerTokens.GetValueOrDefault(type)}",
            SystemMessageTypes.Danger
        );

        return false;
    }

    // Spend tokens with specific NPC context (for queue manipulation)
    public bool SpendTokensWithNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0) return true;
        if (string.IsNullOrEmpty(npcId)) return SpendTokens(type, count); // Fallback to general spending

        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> playerTokens = player.ConnectionTokens;

        // Check if player has enough total tokens
        if (playerTokens.GetValueOrDefault(type) < count)
        {
            return false;
        }

        // Ensure NPC token tracking exists
        if (!player.NPCTokens.ContainsKey(npcId))
        {
            player.NPCTokens[npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                player.NPCTokens[npcId][tokenType] = 0;
            }
        }

        // Spend from player total
        playerTokens[type] -= count;

        // Also reduce from NPC relationship (can go negative)
        player.NPCTokens[npcId][type] -= count;

        // Add narrative feedback
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"You call in {count} {type} favor{(count > 1 ? "s" : "")} with {npc.Name}.",
                SystemMessageTypes.Info
            );

            if (player.NPCTokens[npcId][type] < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"You now owe {npc.Name} for this favor.",
                    SystemMessageTypes.Warning
                );
            }
        }

        // Check threshold-based obligations if we have the manager
        if (_obligationManager != null)
        {
            int oldCount = player.NPCTokens[npcId][type] + count; // Calculate what it was before spending
            _obligationManager.OnTokensChanged(npcId, type, oldCount, player.NPCTokens[npcId][type]);
        }

        return true;
    }

    // Check if player has enough tokens
    public bool HasTokens(ConnectionType type, int count)
    {
        Dictionary<ConnectionType, int> playerTokens = _gameWorld.GetPlayer().ConnectionTokens;
        return playerTokens.GetValueOrDefault(type) >= count;
    }

    // Get total tokens of a type
    public int GetTokenCount(ConnectionType type)
    {
        return _gameWorld.GetPlayer().ConnectionTokens.GetValueOrDefault(type);
    }

    // Add tokens without NPC context (for undo operations)
    public void AddTokens(ConnectionType type, int count)
    {
        if (count <= 0) return;

        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> playerTokens = player.ConnectionTokens;

        // Update global token count
        playerTokens[type] = playerTokens.GetValueOrDefault(type) + count;

        _messageSystem.AddSystemMessage(
            $"🤝 +{count} {type} token{(count > 1 ? "s" : "")} (Total: {playerTokens[type]})",
            SystemMessageTypes.Success
        );
    }

    // Calculate connection gravity for NPC (for future use)
    public int GetConnectionGravity(string npcId)
    {
        Dictionary<ConnectionType, int> npcTokens = GetTokensWithNPC(npcId);
        int totalTokens = npcTokens.Values.Sum();

        if (totalTokens >= GameRules.TOKENS_QUALITY_THRESHOLD) return 6;  // Strong connection
        if (totalTokens >= GameRules.TOKENS_BASIC_THRESHOLD) return 7;  // Moderate connection
        return 8;                        // Default position
    }

    // Remove tokens from NPC relationship (for expired letters)
    public void RemoveTokensFromNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;

        Player player = _gameWorld.GetPlayer();
        Dictionary<string, Dictionary<ConnectionType, int>> npcTokens = player.NPCTokens;

        // Ensure NPC token dictionary exists
        if (!npcTokens.ContainsKey(npcId))
        {
            npcTokens[npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                npcTokens[npcId][tokenType] = 0;
            }
        }

        // Track old count for obligation checking
        int oldCount = npcTokens[npcId][type];

        // Remove tokens from NPC relationship (can go negative)
        npcTokens[npcId][type] -= count;

        // Also remove from player's total tokens (but don't go below 0)
        Dictionary<ConnectionType, int> playerTokens = player.ConnectionTokens;
        playerTokens[type] = Math.Max(0, playerTokens.GetValueOrDefault(type) - count);

        // Add narrative feedback for relationship damage
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"Your relationship with {npc.Name} has been damaged. (-{count} {type} token{(count > 1 ? "s" : "")})",
                SystemMessageTypes.Warning
            );

            if (npcTokens[npcId][type] < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"{npc.Name} feels you owe them for past failures.",
                    SystemMessageTypes.Danger
                );
            }
        }

        // Check threshold-based obligations if we have the manager
        if (_obligationManager != null)
        {
            _obligationManager.OnTokensChanged(npcId, type, oldCount, npcTokens[npcId][type]);
        }
    }

    // Check if player has 3+ tokens with a specific NPC
    public bool HasEnoughTokensForDirectOffer(string npcId)
    {
        Dictionary<ConnectionType, int> npcTokens = GetTokensWithNPC(npcId);
        int totalTokens = npcTokens.Values.Sum();
        return totalTokens >= GameRules.TOKENS_BASIC_THRESHOLD;
    }

    // Get total tokens of a specific type across all NPCs
    public int GetTotalTokensOfType(ConnectionType type)
    {
        Player player = _gameWorld.GetPlayer();
        return player.ConnectionTokens.GetValueOrDefault(type, 0);
    }

    // Spend tokens of a specific type from any NPCs that have that type
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;

        Player player = _gameWorld.GetPlayer();
        int totalAvailable = player.ConnectionTokens.GetValueOrDefault(type, 0);

        if (totalAvailable < amount)
            return false;

        // Deduct from total
        player.ConnectionTokens[type] = totalAvailable - amount;

        // Deduct from NPCs proportionally
        Dictionary<string, Dictionary<ConnectionType, int>> npcTokens = player.NPCTokens;
        int remaining = amount;

        foreach (string? npcId in npcTokens.Keys.ToList())
        {
            if (remaining <= 0) break;

            Dictionary<ConnectionType, int> npcDict = npcTokens[npcId];
            int tokensWithNpc = npcDict.GetValueOrDefault(type, 0);

            if (tokensWithNpc > 0)
            {
                int toSpend = Math.Min(tokensWithNpc, remaining);
                npcDict[type] = tokensWithNpc - toSpend;
                remaining -= toSpend;
            }
        }

        return true;
    }

    // Spend tokens from a specific NPC
    public bool SpendTokens(ConnectionType type, int amount, string npcId)
    {
        if (amount <= 0) return true;

        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> npcTokens = GetTokensWithNPC(npcId);
        int tokensOfType = npcTokens.GetValueOrDefault(type, 0);

        if (tokensOfType < amount)
            return false;

        // Deduct from NPC relationship
        if (!player.NPCTokens.ContainsKey(npcId))
        {
            player.NPCTokens[npcId] = new Dictionary<ConnectionType, int>();
        }
        player.NPCTokens[npcId][type] = tokensOfType - amount;

        // Also deduct from total
        int totalOfType = player.ConnectionTokens.GetValueOrDefault(type, 0);
        player.ConnectionTokens[type] = Math.Max(0, totalOfType - amount);

        return true;
    }

    // Helper method to generate NPC reactions to token gains
    private string GetTokenGainReaction(NPC npc, ConnectionType type, int count)
    {
        string[] reactions = type switch
        {
            ConnectionType.Trust => new[]
            {
                $"{npc.Name} smiles warmly. \"I knew I could count on you.\"",
                $"{npc.Name} clasps your hand. \"Thank you, my friend.\"",
                $"\"You've proven yourself trustworthy,\" {npc.Name} says with appreciation."
            },
            ConnectionType.Commerce => new[]
            {
                $"{npc.Name} nods approvingly. \"Good business, as always.\"",
                $"\"Reliable couriers are worth their weight in gold,\" says {npc.Name}.",
                $"{npc.Name} makes a note. \"I'll remember this efficiency.\""
            },
            ConnectionType.Status => new[]
            {
                $"{npc.Name} inclines their head graciously. \"Your service honors us both.\"",
                $"\"The nobility appreciates discretion,\" {npc.Name} says formally.",
                $"{npc.Name} acknowledges your service with courtly grace."
            },
            ConnectionType.Shadow => new[]
            {
                $"{npc.Name} gives a subtle nod. \"Your discretion is... noted.\"",
                $"\"We remember those who keep our secrets,\" {npc.Name} murmurs.",
                $"A meaningful look passes between you and {npc.Name}."
            },
            _ => new[] { $"{npc.Name} acknowledges your service." }
        };

        Random random = new Random();
        return reactions[random.Next(reactions.Length)];
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
    /// Check if player has at least the specified number of tokens with an NPC.
    /// </summary>
    public bool HasTokensWithNPC(string npcId, int minTokens)
    {
        Dictionary<ConnectionType, int> npcTokens = GetTokensWithNPC(npcId);
        return npcTokens.Values.Sum() >= minTokens;
    }

    // Set tokens for scenario initialization
    public void SetTokensForScenario(Dictionary<string, Dictionary<ConnectionType, int>> tokenData)
    {
        Player player = _gameWorld.GetPlayer();

        // Clear existing tokens
        player.ConnectionTokens.Clear();
        player.NPCTokens.Clear();

        // Set tokens from scenario data
        foreach (KeyValuePair<string, Dictionary<ConnectionType, int>> entry in tokenData)
        {
            if (entry.Key == "global")
            {
                // Set global token counts
                foreach (KeyValuePair<ConnectionType, int> tokenEntry in entry.Value)
                {
                    player.ConnectionTokens[tokenEntry.Key] = tokenEntry.Value;
                }
            }
            else
            {
                // Set NPC-specific tokens
                if (!player.NPCTokens.ContainsKey(entry.Key))
                {
                    player.NPCTokens[entry.Key] = new Dictionary<ConnectionType, int>();
                }

                foreach (KeyValuePair<ConnectionType, int> tokenEntry in entry.Value)
                {
                    player.NPCTokens[entry.Key][tokenEntry.Key] = tokenEntry.Value;
                }
            }
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

    /// <summary>
    /// Check if player has equipment that enables generation of a specific token type
    /// </summary>
    public bool CanGenerateTokenType(ConnectionType tokenType, string npcId)
    {
        // First check if NPC naturally offers this token type
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null && npc.LetterTokenTypes.Contains(tokenType))
        {
            return true;
        }
        
        // Then check equipment
        if (_itemRepository == null) return false;
        
        Player player = _gameWorld.GetPlayer();
        
        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            Item item = _itemRepository.GetItemById(itemId);
            if (item != null && item.EnablesTokenGeneration != null && 
                item.EnablesTokenGeneration.Contains(tokenType))
            {
                return true;
            }
        }
        
        return false;
    }
}
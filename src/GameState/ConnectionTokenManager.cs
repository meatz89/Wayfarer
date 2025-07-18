using System;
using System.Collections.Generic;
using System.Linq;
public class ConnectionTokenManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    
    public ConnectionTokenManager(GameWorld gameWorld, MessageSystem messageSystem, NPCRepository npcRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
    }
    
    // Get player's total tokens by type
    public Dictionary<ConnectionType, int> GetPlayerTokens()
    {
        return _gameWorld.GetPlayer().ConnectionTokens;
    }
    
    // Get tokens with specific NPC
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
    {
        var npcTokens = _gameWorld.GetPlayer().NPCTokens;
        if (npcTokens.ContainsKey(npcId))
            return npcTokens[npcId];
        
        // Return empty dictionary if no tokens with this NPC
        var emptyTokens = new Dictionary<ConnectionType, int>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            emptyTokens[tokenType] = 0;
        }
        return emptyTokens;
    }
    
    // Add tokens (from delivery or other sources)
    public void AddTokens(ConnectionType type, int count, string npcId = null)
    {
        if (count <= 0) return;
        
        var playerTokens = _gameWorld.GetPlayer().ConnectionTokens;
        playerTokens[type] = playerTokens.GetValueOrDefault(type) + count;
        
        // Add narrative feedback for token gain
        if (!string.IsNullOrEmpty(npcId))
        {
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc != null)
            {
                // NPC-specific reaction based on token type
                var reaction = GetTokenGainReaction(npc, type, count);
                _messageSystem.AddSystemMessage(reaction, SystemMessageTypes.Success);
            }
        }
        
        _messageSystem.AddSystemMessage(
            $"Gained {count} {type} token{(count > 1 ? "s" : "")}",
            SystemMessageTypes.Info
        );
        
        // Track by NPC if provided
        if (!string.IsNullOrEmpty(npcId))
        {
            var npcTokens = _gameWorld.GetPlayer().NPCTokens;
            if (!npcTokens.ContainsKey(npcId))
            {
                npcTokens[npcId] = new Dictionary<ConnectionType, int>();
                foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
                {
                    npcTokens[npcId][tokenType] = 0;
                }
            }
            npcTokens[npcId][type] += count;
            
            // Check for relationship milestones
            var totalWithNPC = npcTokens[npcId].Values.Sum();
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc != null)
            {
                CheckRelationshipMilestone(npc, totalWithNPC);
            }
        }
    }
    
    // Spend tokens (for queue actions)
    public bool SpendTokens(ConnectionType type, int count)
    {
        if (count <= 0) return true;
        
        var playerTokens = _gameWorld.GetPlayer().ConnectionTokens;
        if (playerTokens.GetValueOrDefault(type) >= count)
        {
            playerTokens[type] -= count;
            return true;
        }
        return false;
    }
    
    // Spend tokens with specific NPC context (for queue manipulation)
    public bool SpendTokensWithNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0) return true;
        if (string.IsNullOrEmpty(npcId)) return SpendTokens(type, count); // Fallback to general spending
        
        var player = _gameWorld.GetPlayer();
        var playerTokens = player.ConnectionTokens;
        
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
        var npc = _npcRepository.GetNPCById(npcId);
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
        
        return true;
    }
    
    // Check if player has enough tokens
    public bool HasTokens(ConnectionType type, int count)
    {
        var playerTokens = _gameWorld.GetPlayer().ConnectionTokens;
        return playerTokens.GetValueOrDefault(type) >= count;
    }
    
    // Get total tokens of a type
    public int GetTokenCount(ConnectionType type)
    {
        return _gameWorld.GetPlayer().ConnectionTokens.GetValueOrDefault(type);
    }
    
    // Calculate connection gravity for NPC (for future use)
    public int GetConnectionGravity(string npcId)
    {
        var npcTokens = GetTokensWithNPC(npcId);
        var totalTokens = npcTokens.Values.Sum();
        
        if (totalTokens >= 5) return 6;  // Strong connection
        if (totalTokens >= 3) return 7;  // Moderate connection
        return 8;                        // Default position
    }
    
    // Remove tokens from NPC relationship (for expired letters)
    public void RemoveTokensFromNPC(ConnectionType type, int count, string npcId)
    {
        if (count <= 0 || string.IsNullOrEmpty(npcId)) return;
        
        var player = _gameWorld.GetPlayer();
        var npcTokens = player.NPCTokens;
        
        // Ensure NPC token dictionary exists
        if (!npcTokens.ContainsKey(npcId))
        {
            npcTokens[npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                npcTokens[npcId][tokenType] = 0;
            }
        }
        
        // Remove tokens from NPC relationship (can go negative)
        npcTokens[npcId][type] -= count;
        
        // Also remove from player's total tokens (but don't go below 0)
        var playerTokens = player.ConnectionTokens;
        playerTokens[type] = Math.Max(0, playerTokens.GetValueOrDefault(type) - count);
        
        // Add narrative feedback for relationship damage
        var npc = _npcRepository.GetNPCById(npcId);
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
    }
    
    // Check if player has 3+ tokens with a specific NPC
    public bool HasEnoughTokensForDirectOffer(string npcId)
    {
        var npcTokens = GetTokensWithNPC(npcId);
        var totalTokens = npcTokens.Values.Sum();
        return totalTokens >= 3;
    }
    
    // Get total tokens of a specific type across all NPCs
    public int GetTotalTokensOfType(ConnectionType type)
    {
        var player = _gameWorld.GetPlayer();
        return player.ConnectionTokens.GetValueOrDefault(type, 0);
    }
    
    // Spend tokens of a specific type from any NPCs that have that type
    public bool SpendTokensOfType(ConnectionType type, int amount)
    {
        if (amount <= 0) return true;
        
        var player = _gameWorld.GetPlayer();
        var totalAvailable = player.ConnectionTokens.GetValueOrDefault(type, 0);
        
        if (totalAvailable < amount)
            return false;
            
        // Deduct from total
        player.ConnectionTokens[type] = totalAvailable - amount;
        
        // Deduct from NPCs proportionally
        var npcTokens = player.NPCTokens;
        int remaining = amount;
        
        foreach (var npcId in npcTokens.Keys.ToList())
        {
            if (remaining <= 0) break;
            
            var npcDict = npcTokens[npcId];
            var tokensWithNpc = npcDict.GetValueOrDefault(type, 0);
            
            if (tokensWithNpc > 0)
            {
                var toSpend = Math.Min(tokensWithNpc, remaining);
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
        
        var player = _gameWorld.GetPlayer();
        var npcTokens = GetTokensWithNPC(npcId);
        var tokensOfType = npcTokens.GetValueOrDefault(type, 0);
        
        if (tokensOfType < amount)
            return false;
            
        // Deduct from NPC relationship
        if (!player.NPCTokens.ContainsKey(npcId))
        {
            player.NPCTokens[npcId] = new Dictionary<ConnectionType, int>();
        }
        player.NPCTokens[npcId][type] = tokensOfType - amount;
        
        // Also deduct from total
        var totalOfType = player.ConnectionTokens.GetValueOrDefault(type, 0);
        player.ConnectionTokens[type] = Math.Max(0, totalOfType - amount);
        
        return true;
    }
    
    // Helper method to generate NPC reactions to token gains
    private string GetTokenGainReaction(NPC npc, ConnectionType type, int count)
    {
        var reactions = type switch
        {
            ConnectionType.Trust => new[]
            {
                $"{npc.Name} smiles warmly. \"I knew I could count on you.\"",
                $"{npc.Name} clasps your hand. \"Thank you, my friend.\"",
                $"\"You've proven yourself trustworthy,\" {npc.Name} says with appreciation."
            },
            ConnectionType.Trade => new[]
            {
                $"{npc.Name} nods approvingly. \"Good business, as always.\"",
                $"\"Reliable couriers are worth their weight in gold,\" says {npc.Name}.",
                $"{npc.Name} makes a note. \"I'll remember this efficiency.\""
            },
            ConnectionType.Noble => new[]
            {
                $"{npc.Name} inclines their head graciously. \"Your service honors us both.\"",
                $"\"The nobility appreciates discretion,\" {npc.Name} says formally.",
                $"{npc.Name} acknowledges your service with courtly grace."
            },
            ConnectionType.Common => new[]
            {
                $"{npc.Name} grins. \"You're good people, you are!\"",
                $"\"Folk like you keep our communities connected,\" says {npc.Name}.",
                $"{npc.Name} slaps your shoulder friendly. \"Always knew you were one of us.\""
            },
            ConnectionType.Shadow => new[]
            {
                $"{npc.Name} gives a subtle nod. \"Your discretion is... noted.\"",
                $"\"We remember those who keep our secrets,\" {npc.Name} murmurs.",
                $"A meaningful look passes between you and {npc.Name}."
            },
            _ => new[] { $"{npc.Name} acknowledges your service." }
        };
        
        var random = new Random();
        return reactions[random.Next(reactions.Length)];
    }
    
    // Helper method to check and announce relationship milestones
    private void CheckRelationshipMilestone(NPC npc, int totalTokens)
    {
        var milestones = new Dictionary<int, string>
        {
            { 3, $"{npc.Name} now trusts you enough to share private correspondence." },
            { 5, $"Your bond with {npc.Name} has deepened. They'll offer more valuable letters." },
            { 8, $"{npc.Name} considers you among their most trusted associates. Premium letters are now available." },
            { 12, $"Few people enjoy the level of trust {npc.Name} has in you." }
        };
        
        if (milestones.TryGetValue(totalTokens, out var message))
        {
            _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
        }
    }
    
    /// <summary>
    /// Check if player has at least the specified number of tokens with an NPC.
    /// </summary>
    public bool HasTokensWithNPC(string npcId, int minTokens)
    {
        var npcTokens = GetTokensWithNPC(npcId);
        return npcTokens.Values.Sum() >= minTokens;
    }
}
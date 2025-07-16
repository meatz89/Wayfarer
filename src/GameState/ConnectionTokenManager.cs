using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    public class ConnectionTokenManager
    {
        private readonly GameWorld _gameWorld;
        
        public ConnectionTokenManager(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
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
        }
    }
}
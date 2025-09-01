using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.TokenSubsystem
{
    /// <summary>
    /// Calculates and applies token effects from equipment, relationships, and game state.
    /// </summary>
    public class TokenEffectProcessor
    {
        private readonly GameWorld _gameWorld;
        private readonly ItemRepository _itemRepository;
        private readonly ConnectionTokenManager _tokenManager;
        
        // Base success bonus per token (configurable via GameRules)
        private const int BASE_TRUST_BONUS = 5;      // +5% per Trust token
        private const int BASE_COMMERCE_BONUS = 5;   // +5% per Commerce token
        private const int BASE_STATUS_BONUS = 10;    // +10% per Status token
        private const int BASE_SHADOW_BONUS = 8;     // +8% per Shadow token
        
        public TokenEffectProcessor(
            GameWorld gameWorld,
            ItemRepository itemRepository,
            ConnectionTokenManager tokenManager)
        {
            _gameWorld = gameWorld;
            _itemRepository = itemRepository;
            _tokenManager = tokenManager;
        }
        
        /// <summary>
        /// Apply equipment modifiers to token generation
        /// </summary>
        public int ApplyGenerationModifiers(ConnectionType tokenType, int baseAmount)
        {
            if (baseAmount <= 0) return baseAmount;
            
            float totalModifier = GetEquipmentTokenModifier(tokenType);
            int modifiedAmount = (int)Math.Ceiling(baseAmount * totalModifier);
            
            return modifiedAmount;
        }
        
        /// <summary>
        /// Calculate success bonus from tokens for a specific action
        /// </summary>
        public int CalculateSuccessBonus(ConnectionType type, int baseChance)
        {
            int tokenCount = _tokenManager.GetTotalTokensOfType(type);
            if (tokenCount <= 0) return 0;
            
            int bonusPerToken = GetBonusPerToken(type);
            int totalBonus = tokenCount * bonusPerToken;
            
            // Cap bonus at 50% to prevent trivializing challenges
            int maxBonus = baseChance / 2;
            return Math.Min(totalBonus, maxBonus);
        }
        
        /// <summary>
        /// Calculate relationship effect on action success
        /// </summary>
        public int CalculateRelationshipBonus(string npcId, ConnectionType actionType)
        {
            if (string.IsNullOrEmpty(npcId)) return 0;
            
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            int relevantTokens = tokens.GetValueOrDefault(actionType, 0);
            
            // Negative tokens (debt) apply penalties
            if (relevantTokens < 0)
            {
                int penalty = Math.Abs(relevantTokens) * GetBonusPerToken(actionType);
                return -penalty; // Return negative for penalty
            }
            
            // Positive tokens apply bonuses
            int bonus = relevantTokens * GetBonusPerToken(actionType);
            
            // Additional synergy bonus if multiple token types are high
            int synergyBonus = CalculateSynergyBonus(tokens);
            
            return bonus + synergyBonus;
        }
        
        /// <summary>
        /// Get all active token modifiers from equipment
        /// </summary>
        public Dictionary<ConnectionType, float> GetActiveModifiers()
        {
            Player player = _gameWorld.GetPlayer();
            Dictionary<ConnectionType, float> activeModifiers = new Dictionary<ConnectionType, float>();
            
            // Initialize all types to 1.0 (no modifier)
            foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
            {
                if (type != ConnectionType.None)
                {
                    activeModifiers[type] = 1.0f;
                }
            }
            
            // Apply equipment modifiers
            foreach (string itemId in player.Inventory.GetAllItems())
            {
                if (string.IsNullOrEmpty(itemId)) continue;
                
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.TokenGenerationModifiers != null)
                {
                    foreach (KeyValuePair<ConnectionType, float> modifier in item.TokenGenerationModifiers)
                    {
                        // Multiply modifiers (e.g., 1.5 * 1.2 = 1.8 for +50% and +20%)
                        activeModifiers[modifier.Key] *= modifier.Value;
                    }
                }
            }
            
            return activeModifiers;
        }
        
        /// <summary>
        /// Check if a token type is enabled for generation (via equipment)
        /// </summary>
        public bool IsTokenTypeEnabled(ConnectionType type)
        {
            // Base token types are always enabled
            if (type == ConnectionType.Trust || type == ConnectionType.Commerce)
            {
                return true;
            }
            
            // Status and Shadow might require special equipment
            Player player = _gameWorld.GetPlayer();
            
            foreach (string itemId in player.Inventory.GetAllItems())
            {
                if (string.IsNullOrEmpty(itemId)) continue;
                
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.EnablesTokenGeneration != null)
                {
                    if (item.EnablesTokenGeneration.Contains(type))
                    {
                        return true;
                    }
                }
            }
            
            // Check if player has any tokens of this type already (grandfathered in)
            return _tokenManager.GetTotalTokensOfType(type) > 0;
        }
        
        /// <summary>
        /// Calculate token decay over time (for relationship degradation)
        /// </summary>
        public int CalculateTokenDecay(ConnectionType type, int currentTokens, int daysSinceInteraction)
        {
            if (currentTokens <= 0 || daysSinceInteraction < 7) return 0;
            
            // Different token types decay at different rates
            float decayRate = GetDecayRate(type);
            
            // Decay accelerates with time
            int weeksWithoutContact = daysSinceInteraction / 7;
            float decayMultiplier = 1.0f + (weeksWithoutContact * 0.1f);
            
            int decay = (int)Math.Ceiling(currentTokens * decayRate * decayMultiplier);
            
            // Never decay more than half of current tokens in one go
            return Math.Min(decay, currentTokens / 2);
        }
        
        /// <summary>
        /// Get equipment-based token generation modifier
        /// </summary>
        private float GetEquipmentTokenModifier(ConnectionType tokenType)
        {
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
                    // Multiply modifiers
                    totalModifier *= modifier;
                }
            }
            
            return totalModifier;
        }
        
        /// <summary>
        /// Get base bonus per token based on type
        /// </summary>
        private int GetBonusPerToken(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.Trust:
                    return BASE_TRUST_BONUS;
                case ConnectionType.Commerce:
                    return BASE_COMMERCE_BONUS;
                case ConnectionType.Status:
                    return BASE_STATUS_BONUS;
                case ConnectionType.Shadow:
                    return BASE_SHADOW_BONUS;
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// Calculate synergy bonus from having multiple token types
        /// </summary>
        private int CalculateSynergyBonus(Dictionary<ConnectionType, int> tokens)
        {
            int typesWithTokens = 0;
            int totalTokens = 0;
            
            foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
            {
                if (type == ConnectionType.None) continue;
                
                int count = tokens.GetValueOrDefault(type, 0);
                if (count > 0)
                {
                    typesWithTokens++;
                    totalTokens += count;
                }
            }
            
            // No synergy with only one type
            if (typesWithTokens <= 1) return 0;
            
            // Synergy bonus: 2% per additional type, multiplied by average tokens
            int averageTokens = totalTokens / typesWithTokens;
            int synergyBonus = (typesWithTokens - 1) * 2 * averageTokens;
            
            return Math.Min(synergyBonus, 20); // Cap at 20% bonus
        }
        
        /// <summary>
        /// Get decay rate for token type
        /// </summary>
        private float GetDecayRate(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.Trust:
                    return 0.05f; // Trust decays slowly (5% per week)
                case ConnectionType.Commerce:
                    return 0.03f; // Commerce is most stable (3% per week)
                case ConnectionType.Status:
                    return 0.08f; // Status decays faster (8% per week)
                case ConnectionType.Shadow:
                    return 0.10f; // Shadow decays fastest (10% per week)
                default:
                    return 0.05f;
            }
        }
    }
}
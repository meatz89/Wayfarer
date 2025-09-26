using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;

namespace Wayfarer.Subsystems.ExchangeSubsystem
{
    /// <summary>
    /// Validates exchange availability and requirements.
    /// Internal to the Exchange subsystem - not exposed publicly.
    /// </summary>
    public class ExchangeValidator
    {
        private readonly GameWorld _gameWorld;
        private readonly TimeManager _timeManager;

        public ExchangeValidator(
            GameWorld gameWorld,
            TimeManager timeManager)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        }

        /// <summary>
        /// Validate if an exchange can be performed
        /// </summary>
        public ExchangeValidationResult ValidateExchange(
            ExchangeData exchange,
            NPC npc,
            PlayerResourceState playerResources,
            Dictionary<ConnectionType, int> npcTokens,
            RelationshipTier relationshipTier,
            List<string> currentSpotDomains)
        {
            ExchangeValidationResult result = new ExchangeValidationResult
            {
                IsValid = true,
                IsVisible = true,
                CanAfford = true
            };

            // Check visibility requirements first
            if (!CheckVisibilityRequirements(exchange, npc, relationshipTier))
            {
                result.IsVisible = false;
                result.IsValid = false;
                result.ValidationMessage = "Exchange not available";
                return result;
            }

            // Check domain requirements
            if (!CheckDomainRequirements(exchange, currentSpotDomains))
            {
                result.IsVisible = false;
                result.IsValid = false;
                result.ValidationMessage = "Wrong location for this exchange";
                return result;
            }

            // Check time restrictions
            if (!CheckTimeRequirements(exchange))
            {
                result.IsVisible = true; // Show but disabled
                result.IsValid = false;
                result.ValidationMessage = GetTimeRestrictionMessage(exchange);
                return result;
            }

            // Check token requirements
            if (!CheckTokenRequirements(exchange, npc, npcTokens))
            {
                result.IsVisible = true; // Show but disabled
                result.IsValid = false;
                result.ValidationMessage = GetTokenRequirementMessage(exchange);
                return result;
            }

            // Check item requirements
            if (!CheckItemRequirements(exchange, playerResources))
            {
                result.IsVisible = true; // Show but disabled
                result.IsValid = false;
                result.ValidationMessage = GetItemRequirementMessage(exchange);
                return result;
            }

            // Check affordability
            if (!CanAffordExchange(exchange, playerResources, npcTokens))
            {
                result.CanAfford = false;
                result.IsValid = false;
                result.ValidationMessage = "Cannot afford this exchange";
                return result;
            }

            // Check NPC state requirements
            if (!CheckNPCStateRequirements(exchange, npc, npcTokens))
            {
                result.IsVisible = true;
                result.IsValid = false;
                result.ValidationMessage = "NPC is not in the right state for this exchange";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Check if player can afford the exchange costs
        /// </summary>
        public bool CanAffordExchange(ExchangeData exchange, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens)
        {

            foreach (ResourceAmount cost in exchange.Costs)
            {
                if (!CanAffordResource(cost, playerResources, npcTokens))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check visibility requirements (minimum relationship to even see the exchange)
        /// </summary>
        private bool CheckVisibilityRequirements(ExchangeData exchange, NPC npc, RelationshipTier relationshipTier)
        {
            // Check if exchange requires minimum relationship
            if (exchange.MinimumRelationshipTier > 0)
            {
                if ((int)relationshipTier < exchange.MinimumRelationshipTier)
                {
                    return false;
                }
            }

            // Check if exchange is still available (not exhausted)
            if (exchange.IsUnique && exchange.TimesUsed > 0)
            {
                return false;
            }

            if (exchange.MaxUses > 0 && exchange.TimesUsed >= exchange.MaxUses)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if exchange is available at current location
        /// </summary>
        private bool CheckDomainRequirements(ExchangeData exchange, List<string> currentSpotDomains)
        {
            if (exchange.RequiredDomains == null || !exchange.RequiredDomains.Any())
            {
                return true; // No domain requirements
            }

            // Check if any required domain matches current spot domains
            return exchange.RequiredDomains.Any(required =>
                currentSpotDomains.Contains(required, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check if exchange is available at current time
        /// </summary>
        private bool CheckTimeRequirements(ExchangeData exchange)
        {
            if (exchange.TimeRestrictions == null || !exchange.TimeRestrictions.Any())
            {
                return true; // No time restrictions
            }

            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            return exchange.TimeRestrictions.Contains(currentTime);
        }

        /// <summary>
        /// Check if player has required tokens with NPC
        /// </summary>
        private bool CheckTokenRequirements(ExchangeData exchange, NPC npc, Dictionary<ConnectionType, int> npcTokens)
        {
            if (!exchange.RequiredTokenType.HasValue || exchange.MinimumTokensRequired <= 0)
            {
                return true; // No token requirements
            }

            int currentTokens = npcTokens.GetValueOrDefault(exchange.RequiredTokenType.Value, 0);
            return currentTokens >= exchange.MinimumTokensRequired;
        }

        /// <summary>
        /// Check if player has required items
        /// </summary>
        private bool CheckItemRequirements(ExchangeData exchange, PlayerResourceState playerResources)
        {
            if (exchange.RequiredItems == null || !exchange.RequiredItems.Any())
            {
                return true; // No item requirements
            }

            foreach (string itemId in exchange.RequiredItems)
            {
                Player player = _gameWorld.GetPlayer();
                if (!player.Inventory.HasItem(itemId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check NPC-specific state requirements
        /// </summary>
        private bool CheckNPCStateRequirements(ExchangeData exchange, NPC npc, Dictionary<ConnectionType, int> npcTokens)
        {
            // Patience system removed - all NPCs always have patience

            // Check connection state requirements
            if (exchange.RequiredConnectionState.HasValue)
            {
                ConnectionState currentState = DetermineNPCConnectionState(npcTokens);
                if (currentState != exchange.RequiredConnectionState.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if player can afford a specific resource cost
        /// </summary>
        private bool CanAffordResource(ResourceAmount cost, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens)
        {
            return cost.Type switch
            {
                ResourceType.Coins => playerResources.Coins >= cost.Amount,
                ResourceType.Health => playerResources.Health >= cost.Amount,
                ResourceType.Hunger => true, // Hunger is usually a reward, not a cost
                ResourceType.TrustToken => npcTokens.GetValueOrDefault(ConnectionType.Trust, 0) >= cost.Amount,
                ResourceType.CommerceToken => npcTokens.GetValueOrDefault(ConnectionType.Commerce, 0) >= cost.Amount,
                ResourceType.StatusToken => npcTokens.GetValueOrDefault(ConnectionType.Status, 0) >= cost.Amount,
                ResourceType.ShadowToken => npcTokens.GetValueOrDefault(ConnectionType.Shadow, 0) >= cost.Amount,
                _ => true
            };
        }

        /// <summary>
        /// Determine NPC's current connection state
        /// </summary>
        private ConnectionState DetermineNPCConnectionState(Dictionary<ConnectionType, int> npcTokens)
        {
            // Get total tokens with this NPC
            int totalTokens = npcTokens.Values.Sum();

            // Map token count to connection state
            if (totalTokens <= 0) return ConnectionState.DISCONNECTED;
            if (totalTokens <= 2) return ConnectionState.GUARDED;
            if (totalTokens <= 5) return ConnectionState.NEUTRAL;
            if (totalTokens <= 8) return ConnectionState.RECEPTIVE;
            return ConnectionState.TRUSTING;
        }

        // Message generation helpers

        private string GetTimeRestrictionMessage(ExchangeData exchange)
        {
            if (exchange.TimeRestrictions == null || !exchange.TimeRestrictions.Any())
            {
                return "Not available at this time";
            }

            string availableTimes = string.Join(", ", exchange.TimeRestrictions.Select(t => t.ToString()));
            return $"Only available during: {availableTimes}";
        }

        private string GetTokenRequirementMessage(ExchangeData exchange)
        {
            if (!exchange.RequiredTokenType.HasValue)
            {
                return "Insufficient relationship";
            }

            return $"Requires {exchange.MinimumTokensRequired} {exchange.RequiredTokenType} tokens";
        }

        private string GetItemRequirementMessage(ExchangeData exchange)
        {
            if (exchange.RequiredItems == null || !exchange.RequiredItems.Any())
            {
                return "Missing required items";
            }

            return $"Requires: {string.Join(", ", exchange.RequiredItems)}";
        }
    }

    /// <summary>
    /// Result of exchange validation
    /// </summary>
    public class ExchangeValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsVisible { get; set; }
        public bool CanAfford { get; set; }
        public string ValidationMessage { get; set; }
        public List<string> MissingRequirements { get; set; } = new List<string>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

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
        ExchangeCard exchange,
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
            result.ValidationMessage = "Wrong Venue for this exchange";
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

        // Item requirements removed - PRINCIPLE 4: Items are resource costs (ConsumedItemIds), not boolean gates
        // Affordability check below handles ConsumedItemIds as part of resource costs

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
    /// Check if player can afford the exchange costs (resources only, NOT items)
    /// Item affordability is checked at runtime by ExchangeContext.CanAfford()
    /// </summary>
    public bool CanAffordExchange(ExchangeCard exchange, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens)
    {

        foreach (ResourceAmount cost in exchange.GetCostAsList())
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
    private bool CheckVisibilityRequirements(ExchangeCard exchange, NPC npc, RelationshipTier relationshipTier)
    {
        // Check if exchange requires minimum relationship
        if (exchange.MinimumRelationshipTier > 0)
        {
            if ((int)relationshipTier < exchange.MinimumRelationshipTier)
            {
                return false;
            }
        }

        // Check if exchange is exhausted
        if (exchange.IsExhausted())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if exchange is available at current location
    /// </summary>
    private bool CheckDomainRequirements(ExchangeCard exchange, List<string> currentSpotDomains)
    {
        if (exchange.RequiredDomains == null || !exchange.RequiredDomains.Any())
        {
            return true; // No domain requirements
        }

        // Check if any required domain matches current location domains
        return exchange.RequiredDomains.Any(required =>
            currentSpotDomains.Contains(required, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if exchange is available at current time
    /// </summary>
    private bool CheckTimeRequirements(ExchangeCard exchange)
    {
        if (exchange.AvailableTimeBlocks == null || !exchange.AvailableTimeBlocks.Any())
        {
            return true; // No time restrictions
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return exchange.AvailableTimeBlocks.Contains(currentTime);
    }

    /// <summary>
    /// Check if player has required tokens with NPC
    /// </summary>
    private bool CheckTokenRequirements(ExchangeCard exchange, NPC npc, Dictionary<ConnectionType, int> npcTokens)
    {
        if (exchange.Cost?.TokenRequirements == null || exchange.Cost.TokenRequirements.Count == 0)
        {
            return true; // No token requirements
        }

        foreach (KeyValuePair<ConnectionType, int> requirement in exchange.Cost.TokenRequirements)
        {
            int currentTokens = npcTokens.GetValueOrDefault(requirement.Key, 0);
            if (currentTokens < requirement.Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check NPC-specific state requirements
    /// </summary>
    private bool CheckNPCStateRequirements(ExchangeCard exchange, NPC npc, Dictionary<ConnectionType, int> npcTokens)
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
            ResourceType.DiplomacyToken => npcTokens.GetValueOrDefault(ConnectionType.Diplomacy, 0) >= cost.Amount,
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

    private string GetTimeRestrictionMessage(ExchangeCard exchange)
    {
        if (exchange.AvailableTimeBlocks == null || !exchange.AvailableTimeBlocks.Any())
        {
            return "Not available at this time";
        }

        string availableTimes = string.Join(", ", exchange.AvailableTimeBlocks.Select(t => t.ToString()));
        return $"Only available during: {availableTimes}";
    }

    private string GetTokenRequirementMessage(ExchangeCard exchange)
    {
        if (exchange.Cost?.TokenRequirements == null || exchange.Cost.TokenRequirements.Count == 0)
        {
            return "Insufficient relationship";
        }

        KeyValuePair<ConnectionType, int> firstRequirement = exchange.Cost.TokenRequirements.First();
        return $"Requires {firstRequirement.Value} {firstRequirement.Key} tokens";
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

    /// <summary>
    /// Backward compatibility alias for MissingRequirements.
    /// </summary>
    public string RequirementDetails
    {
        get
        {
            return string.Join(", ", MissingRequirements);
        }

        set { }
    }
}

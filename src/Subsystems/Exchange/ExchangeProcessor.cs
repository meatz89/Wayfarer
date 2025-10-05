using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Processes exchange execution, applying costs and rewards.
/// Internal to the Exchange subsystem - not exposed publicly.
/// </summary>
public class ExchangeProcessor
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeProcessor(
        GameWorld gameWorld,
        TimeManager timeManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Prepare exchange operation data for GameFacade to execute
    /// </summary>
    public ExchangeOperationData PrepareExchangeOperation(ExchangeData exchange, NPC npc, PlayerResourceState playerResources)
    {
        return new ExchangeOperationData
        {
            Costs = exchange.Costs,
            Rewards = exchange.Rewards,
            ItemRewards = exchange.ItemRewards,
            AdvancesTime = ShouldAdvanceTime(exchange),
            TimeAdvancementHours = CalculateTimeAdvancement(exchange),
            AffectsRelationship = exchange.AffectsRelationship,
            FlowModifier = exchange.FlowModifier,
            ConsumesPatience = exchange.ConsumesPatience,
            PatienceCost = exchange.PatienceCost,
            UnlocksExchangeId = exchange.UnlocksExchangeId,
            TriggerEvent = exchange.TriggerEvent,
            NPCId = npc.ID,
            ExchangeId = exchange.Id,
            IsUnique = exchange.IsUnique,
            ConnectionStateChange = exchange.ConnectionStateChange
        };
    }


    /// <summary>
    /// Check if exchange should advance time
    /// </summary>
    private bool ShouldAdvanceTime(ExchangeData exchange)
    {
        // Exchanges advance time based on their configuration
        return exchange.AdvancesTime;
    }

    /// <summary>
    /// Calculate how much time to advance
    /// </summary>
    private int CalculateTimeAdvancement(ExchangeData exchange)
    {
        if (exchange.TimeAdvancementHours > 0)
        {
            return exchange.TimeAdvancementHours;
        }

        // Default: 1 segment for most exchanges
        return 1;
    }

}

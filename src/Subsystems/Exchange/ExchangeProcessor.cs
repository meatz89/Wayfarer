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
    public ExchangeOperationData PrepareExchangeOperation(ExchangeCard exchange, NPC npc, PlayerResourceState playerResources)
    {
        return new ExchangeOperationData
        {
            Costs = exchange.GetCostAsList(),
            Rewards = exchange.GetRewardAsList(),
            // HIGHLANDER: GetItemRewards returns List<Item>, convert to names for display
            ItemRewards = exchange.GetItemRewards().Select(item => item.Name).ToList(),
            AdvancesTime = ShouldAdvanceTime(exchange),
            TimeAdvancementHours = CalculateTimeAdvancement(exchange),
            AffectsRelationship = exchange.AffectsRelationship,
            FlowModifier = exchange.FlowModifier,
            ConsumesPatience = exchange.ConsumesPatience,
            PatienceCost = exchange.PatienceCost,
            // HIGHLANDER: ExchangeCard has UnlocksExchange object, not UnlocksExchangeId string
            UnlocksExchangeId = exchange.UnlocksExchange?.Name,
            TriggerEvent = exchange.TriggerEvent,
            // HIGHLANDER: Store NPC object, not npc.ID
            Npc = npc,
            // HIGHLANDER: ExchangeCard has NO Id property, use Name as natural key
            ExchangeId = exchange.Name,
            IsUnique = exchange.SingleUse,
            ConnectionStateChange = exchange.ConnectionStateChange
        };
    }

    /// <summary>
    /// Check if exchange should advance time
    /// </summary>
    private bool ShouldAdvanceTime(ExchangeCard exchange)
    {
        // Exchanges advance time based on their configuration
        return exchange.AdvancesTime;
    }

    /// <summary>
    /// Calculate how much time to advance
    /// </summary>
    private int CalculateTimeAdvancement(ExchangeCard exchange)
    {
        if (exchange.TimeAdvancementHours > 0)
        {
            return exchange.TimeAdvancementHours;
        }

        // Default: 1 segment for most exchanges
        return 1;
    }

}

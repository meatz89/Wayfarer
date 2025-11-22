/// <summary>
/// Context object for exchange UI rendering.
/// Contains all data needed to display and interact with exchanges.
/// This is passed to UI components to render the exchange interface.
/// </summary>
public class ExchangeContext
{
    /// <summary>
    /// The active exchange session.
    /// </summary>
    public ExchangeSession Session { get; set; }

    /// <summary>
    /// Current player resource state.
    /// </summary>
    public PlayerResourceState PlayerResources { get; set; }

    /// <summary>
    /// Player's current token counts.
    /// </summary>
    public Dictionary<ConnectionType, int> PlayerTokens { get; set; } = new Dictionary<ConnectionType, int>();

    /// <summary>
    /// The player object - contains inventory via Player.Inventory
    /// HIGHLANDER: Access inventory through Player.Inventory, not redundant dictionary
    /// </summary>
    public Player Player { get; set; }

    // ADR-007: NpcInfo DELETED - use NPC object reference directly
    /// <summary>
    /// The NPC offering exchanges. Null for location-based exchanges.
    /// </summary>
    public NPC Npc { get; set; }

    // ADR-007: LocationInfo DELETED - use Location object reference directly
    /// <summary>
    /// Current location where exchange is happening.
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    /// Current time block.
    /// </summary>
    public TimeBlocks CurrentTimeBlock { get; set; }

    /// <summary>
    /// Gets exchanges that are currently available.
    /// Filters out completed single-use exchanges and time/location restricted ones.
    /// </summary>
    public List<ExchangeCard> GetAvailableExchanges()
    {
        if (Session == null)
            return new List<ExchangeCard>();

        // HIGHLANDER: Pass Venue object directly
        Venue venue = Location?.Venue;
        return Session.AvailableExchanges
            .Where(e => e.ExchangeCard != null && e.ExchangeCard.IsAvailable(venue, CurrentTimeBlock))
            .Select(e => e.ExchangeCard)
            .ToList();
    }

    /// <summary>
    /// Gets exchanges the player can actually afford.
    /// Checks both resource costs and token requirements.
    /// </summary>
    public List<ExchangeCard> GetAffordableExchanges()
    {
        return GetAvailableExchanges()
            .Where(e => CanAfford(e))
            .ToList();
    }

    /// <summary>
    /// Checks if the player can afford a specific exchange.
    /// </summary>
    public bool CanAfford(ExchangeCard exchange)
    {
        if (exchange == null)
            return false;

        // Check resource costs
        if (!exchange.Cost.CanAfford(PlayerResources))
            return false;

        // Check token requirements
        // Convert Dictionary<ConnectionType, int> to List<TokenCount>
        List<TokenCount> tokenList = PlayerTokens.Select(kvp => new TokenCount { Type = kvp.Key, Count = kvp.Value }).ToList();
        if (!exchange.Cost.MeetsTokenRequirements(tokenList))
            return false;

        // Check consumed item requirements (resource costs)
        // HIGHLANDER: Use Player.Inventory directly, check Item objects
        foreach (Item item in exchange.Cost.ConsumedItems)
        {
            if (Player?.Inventory == null || !Player.Inventory.Contains(item))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a categorized view of available exchanges.
    /// Groups by exchange type for better UI organization.
    /// </summary>
    public Dictionary<ExchangeType, List<ExchangeCard>> GetExchangesByType()
    {
        Dictionary<ExchangeType, List<ExchangeCard>> result = new Dictionary<ExchangeType, List<ExchangeCard>>();
        List<ExchangeCard> available = GetAvailableExchanges();

        foreach (ExchangeCard exchange in available)
        {
            if (!result.ContainsKey(exchange.ExchangeType))
            {
                result[exchange.ExchangeType] = new List<ExchangeCard>();
            }
            result[exchange.ExchangeType].Add(exchange);
        }

        return result;
    }

    /// <summary>
    /// Gets display information for resource changes from an exchange.
    /// Shows what will be gained and lost.
    /// </summary>
    public ExchangePreview GetExchangePreview(string exchangeId)
    {
        if (Session == null)
            throw new InvalidOperationException("Cannot get exchange preview: No active exchange session");

        ExchangeOption option = Session.AvailableExchanges.Find(e => e.ExchangeId == exchangeId);
        if (option == null)
            throw new InvalidOperationException($"Exchange option not found: {exchangeId}");

        ExchangeCard exchange = option.ExchangeCard;
        if (exchange == null)
            throw new InvalidOperationException($"Exchange card not found for option: {exchangeId}");

        return new ExchangePreview
        {
            // HIGHLANDER: ExchangeCard has NO Id property, use Name as natural key
            ExchangeId = exchange.Name,
            ExchangeName = exchange.Name,
            CanAfford = CanAfford(exchange),
            CostDescription = exchange.Cost.GetDescription(),
            RewardDescription = exchange.Reward.GetDescription(),
            SuccessRate = exchange.SuccessRate,
            IsRisky = exchange.IsRisky(),
            NewResourceState = CalculateNewResourceState(exchange)
        };
    }

    /// <summary>
    /// Calculates what the resource state would be after an exchange.
    /// Assumes success - used for preview only.
    /// </summary>
    private ResourcePreview CalculateNewResourceState(ExchangeCard exchange)
    {
        if (exchange == null)
            return null;

        ResourcePreview preview = new ResourcePreview
        {
            Coins = PlayerResources.Coins,
            Health = PlayerResources.Health,
            Stamina = PlayerResources.Stamina,
        };

        // Apply costs
        foreach (ResourceAmount cost in exchange.Cost.Resources)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    preview.Coins -= cost.Amount;
                    break;
                case ResourceType.Health:
                    preview.Health -= cost.Amount;
                    break;
                case ResourceType.Hunger:
                    preview.Stamina -= cost.Amount;
                    break;
            }
        }

        // Apply rewards
        foreach (ResourceAmount reward in exchange.Reward.Resources)
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    preview.Coins += reward.Amount;
                    break;
                case ResourceType.Health:
                    preview.Health += reward.Amount;
                    break;
                case ResourceType.Hunger:
                    preview.Stamina += reward.Amount;
                    break;
            }
        }

        return preview;
    }
}

/// <summary>
/// Preview of an exchange for UI display.
/// </summary>
public class ExchangePreview
{
    public string ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public bool CanAfford { get; set; }
    public string CostDescription { get; set; }
    public string RewardDescription { get; set; }
    public int SuccessRate { get; set; }
    public bool IsRisky { get; set; }
    public ResourcePreview NewResourceState { get; set; }
}

/// <summary>
/// Preview of resource state after an exchange.
/// </summary>
public class ResourcePreview
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Stamina { get; set; }
}

// ADR-007: NpcInfo and LocationInfo classes DELETED
// These were redundant DTOs wrapping domain entities with IDs
// Use NPC and Location object references directly in ExchangeContext
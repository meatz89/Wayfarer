using System.Collections.Generic;
using System.Linq;

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
    /// Player's current inventory items.
    /// Key: ItemId, Value: Quantity
    /// </summary>
    public Dictionary<string, int> PlayerInventory { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Information about the NPC offering exchanges.
    /// Null for location-based exchanges.
    /// </summary>
    public NpcInfo NpcInfo { get; set; }

    /// <summary>
    /// Current Venue information.
    /// </summary>
    public LocationInfo LocationInfo { get; set; }

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
        if (Session?.AvailableExchanges == null)
            return new List<ExchangeCard>();

        return Session.AvailableExchanges
            .Where(e => e.ExchangeCard != null && e.ExchangeCard.IsAvailable(LocationInfo?.VenueId, CurrentTimeBlock))
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
        if (!exchange.Cost.MeetsTokenRequirements(PlayerTokens))
            return false;

        // Check consumed item requirements (resource costs)
        foreach (string itemId in exchange.Cost.ConsumedItemIds)
        {
            if (!PlayerInventory.ContainsKey(itemId) || PlayerInventory[itemId] <= 0)
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
        ExchangeOption? option = Session?.AvailableExchanges?.Find(e => e.ExchangeId == exchangeId);
        ExchangeCard? exchange = option?.ExchangeCard;
        if (exchange == null)
            return null;

        return new ExchangePreview
        {
            ExchangeId = exchange.Id,
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

/// <summary>
/// Basic NPC information for exchange context.
/// </summary>
public class NpcInfo
{
    public string NpcId { get; set; }
    public string Name { get; set; }
    public string Portrait { get; set; }
    public Dictionary<ConnectionType, int> TokenCounts { get; set; } = new Dictionary<ConnectionType, int>();
}

/// <summary>
/// Basic Venue information for exchange context.
/// </summary>
public class LocationInfo
{
    public string VenueId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
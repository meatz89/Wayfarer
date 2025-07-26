using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Service that provides UI data and handles UI actions for the Market
/// Bridges between UI and game logic, ensuring UI has no direct access to game state
/// </summary>
public class MarketUIService
{
    private readonly GameWorld _gameWorld;
    private readonly GameWorldManager _gameManager;
    private readonly ItemRepository _itemRepository;
    private readonly LocationRepository _locationRepository;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly CommandExecutor _commandExecutor;
    private readonly MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;

    public MarketUIService(
        GameWorld gameWorld,
        GameWorldManager gameManager,
        ItemRepository itemRepository,
        LocationRepository locationRepository,
        IGameRuleEngine ruleEngine,
        CommandExecutor commandExecutor,
        MessageSystem messageSystem,
        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _gameManager = gameManager;
        _itemRepository = itemRepository;
        _locationRepository = locationRepository;
        _ruleEngine = ruleEngine;
        _commandExecutor = commandExecutor;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Get market view model for a specific location
    /// </summary>
    public MarketViewModel GetMarketViewModel(string locationId)
    {
        Location location = _locationRepository.GetLocation(locationId);
        Player player = _gameWorld.GetPlayer();
        string marketStatus = _gameManager.GetMarketAvailabilityStatus(locationId);
        List<NPC> traders = _gameManager.GetTradingNPCs(locationId)
            .Where(npc => npc.IsAvailable(_timeManager.GetCurrentTimeBlock()))
            .ToList();

        // Get all available items
        List<Item> marketItems = _gameManager.GetAvailableMarketItems(locationId);

        // Convert items to view models
        HashSet<string> allCategories = new HashSet<string> { "All" };
        List<MarketItemViewModel> itemViewModels = new List<MarketItemViewModel>();

        foreach (Item item in marketItems)
        {
            if (item == null) continue;

            bool canBuy = _gameManager.CanBuyMarketItem(item.Id ?? item.Name, locationId);
            bool canSell = player.Inventory.HasItem(item.Name);

            itemViewModels.Add(new MarketItemViewModel
            {
                ItemId = item.Id ?? item.Name,
                Name = item.Name,
                BuyPrice = item.BuyPrice,
                SellPrice = item.SellPrice,
                CanBuy = canBuy,
                CanSell = canSell,
                Categories = item.Categories.Select(c => c.ToString()).ToList(),
                Item = item
            });

            // Collect categories
            foreach (ItemCategory category in item.Categories)
            {
                allCategories.Add(category.ToString());
            }
        }

        // Build view model
        MarketViewModel viewModel = new MarketViewModel
        {
            LocationName = location.Name,
            MarketStatus = marketStatus,
            IsOpen = marketStatus.Contains("Open"),
            TraderCount = traders.Count,
            PlayerCoins = player.Coins,
            InventoryUsed = player.Inventory.UsedCapacity,
            InventoryCapacity = player.Inventory.Size,
            Items = itemViewModels,
            AvailableCategories = allCategories.OrderBy(c => c).ToList()
        };

        return viewModel;
    }

    /// <summary>
    /// Filter market items by category
    /// </summary>
    public List<MarketItemViewModel> GetFilteredItems(MarketViewModel viewModel, string category)
    {
        if (category == "All")
            return viewModel.Items;

        return viewModel.Items
            .Where(item => item.Categories.Contains(category))
            .ToList();
    }

    /// <summary>
    /// Execute a trade action (buy/sell)
    /// </summary>
    public async Task<bool> ExecuteTradeAsync(string itemId, string action, string locationId)
    {
        MarketTradeCommand.TradeAction tradeAction = action.ToLower() == "buy"
            ? MarketTradeCommand.TradeAction.Buy
            : MarketTradeCommand.TradeAction.Sell;

        MarketTradeCommand command = new MarketTradeCommand(
            itemId,
            tradeAction,
            locationId,
            _itemRepository,
            _ruleEngine);

        CommandResult result = await _commandExecutor.ExecuteAsync(command);

        if (result.IsSuccess)
        {
            _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Success);
        }
        else
        {
            _messageSystem.AddSystemMessage(result.ErrorMessage, SystemMessageTypes.Danger);
        }

        return result.IsSuccess;
    }
}
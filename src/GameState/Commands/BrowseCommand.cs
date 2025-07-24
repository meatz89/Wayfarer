/// <summary>
/// Command to browse available items at markets/shops
/// </summary>
public class BrowseCommand : BaseGameCommand
{
    private readonly string _locationId;
    private readonly MarketManager _marketManager;
    private readonly MessageSystem _messageSystem;

    public BrowseCommand(
        string locationId,
        MarketManager marketManager,
        MessageSystem messageSystem)
    {
        _locationId = locationId ?? throw new ArgumentNullException(nameof(locationId));
        _marketManager = marketManager ?? throw new ArgumentNullException(nameof(marketManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = "Browse market offerings";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player is at a location
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Not at any location");
        }

        // Check if location has a market
        List<Item> availableItems = _marketManager.GetAvailableItems(_locationId);
        if (!availableItems.Any())
        {
            return CommandValidationResult.Failure("No market available at this location");
        }

        // Check time (browsing is free but requires daylight)
        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;
        if (currentTime == TimeBlocks.Night)
        {
            return CommandValidationResult.Failure(
                "Market is closed at night",
                true,
                "Come back during daylight hours");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        LocationSpot spot = player.CurrentLocationSpot;

        // Get available items and their prices
        List<Item> availableItems = _marketManager.GetAvailableItems(_locationId);

        if (!availableItems.Any())
        {
            _messageSystem.AddSystemMessage(
                "The market appears to be out of stock.",
                SystemMessageTypes.Info
            );
            return CommandResult.Success("No items available");
        }

        // Display market inventory
        _messageSystem.AddSystemMessage(
            $"📦 Browsing market at {spot.Name}:",
            SystemMessageTypes.Info
        );

        List<object> marketData = new List<object>();

        foreach (Item item in availableItems)
        {
            int buyPrice = _marketManager.GetItemPrice(_locationId, item.Id, true);
            int sellPrice = _marketManager.GetItemPrice(_locationId, item.Id, false);

            string affordableIndicator = player.Coins >= buyPrice ? "✓" : "✗";
            string priceInfo = $"{item.Name} - Buy: {buyPrice} coins {affordableIndicator}";

            if (sellPrice > 0)
            {
                priceInfo += $", Sell: {sellPrice} coins";
            }

            _messageSystem.AddSystemMessage($"  • {priceInfo}", SystemMessageTypes.Info);

            marketData.Add(new
            {
                Item = item.Name,
                BuyPrice = buyPrice,
                SellPrice = sellPrice,
                CanAfford = player.Coins >= buyPrice,
                Size = item.Size.ToString(),
                Categories = item.Categories.Select(c => c.ToString())
            });
        }

        // Show player's current coins
        _messageSystem.AddSystemMessage(
            $"💰 Your coins: {player.Coins}",
            SystemMessageTypes.Info
        );

        // Check if player has items to sell
        if (player.Inventory.ItemSlots.Any())
        {
            _messageSystem.AddSystemMessage(
                $"📦 You have {player.Inventory.UsedCapacity} items in inventory",
                SystemMessageTypes.Info
            );
        }

        return CommandResult.Success(
            "Market browsed successfully",
            new
            {
                Location = spot.Name,
                PlayerCoins = player.Coins,
                ItemCount = availableItems.Count,
                MarketItems = marketData
            }
        );
    }

}
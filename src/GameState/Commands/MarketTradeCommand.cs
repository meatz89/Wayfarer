using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Command for executing market trades (buy/sell items)
/// </summary>
public class MarketTradeCommand : BaseGameCommand
{
    private readonly string _itemId;
    private readonly TradeAction _action;
    private readonly string _locationId;
    private readonly ItemRepository _itemRepository;
    private readonly GameRuleEngine _ruleEngine;


    public enum TradeAction
    {
        Buy,
        Sell
    }

    public MarketTradeCommand(
        string itemId,
        TradeAction action,
        string locationId,
        ItemRepository itemRepository,
        GameRuleEngine ruleEngine)
    {
        _itemId = itemId;
        _action = action;
        _locationId = locationId;
        _itemRepository = itemRepository;
        _ruleEngine = ruleEngine;

        Description = $"{action} item {itemId} at {locationId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(_itemId);

        if (item == null)
            return CommandValidationResult.Failure("Item not found");

        if (_action == TradeAction.Buy)
        {
            // Check coins
            if (player.Coins < item.BuyPrice)
                return CommandValidationResult.Failure(
                    $"Not enough coins! Need {item.BuyPrice}, have {player.Coins}",
                    true,
                    "Earn more coins through work or deliveries");

            // Check inventory space
            int freeSlots = player.Inventory.Size - player.Inventory.UsedCapacity;
            if (freeSlots < 1)
                return CommandValidationResult.Failure(
                    "Inventory full!",
                    true,
                    "Sell or drop items to make space");
        }
        else // Sell
        {
            // Check if player has the item
            if (!player.Inventory.HasItem(item.Name))
                return CommandValidationResult.Failure("You don't have this item");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        Item item = _itemRepository.GetItemById(_itemId);


        if (_action == TradeAction.Buy)
        {
            // Deduct coins
            player.ModifyCoins(-item.BuyPrice);

            // Add item to inventory
            player.Inventory.AddItem(item.Name);

            return CommandResult.Success(
                $"Bought {item.Name} for {item.BuyPrice} coins",
                new { ItemName = item.Name, Cost = item.BuyPrice });
        }
        else // Sell
        {
            // Add coins
            player.ModifyCoins(item.SellPrice);

            // Remove item from inventory
            int slot = System.Array.IndexOf(player.Inventory.ItemSlots, item.Name);
            player.Inventory.ItemSlots[slot] = null;

            return CommandResult.Success(
                $"Sold {item.Name} for {item.SellPrice} coins",
                new { ItemName = item.Name, Profit = item.SellPrice });
        }
    }

}
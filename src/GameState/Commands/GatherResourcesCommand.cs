using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to gather resources at specific locations
/// </summary>
public class GatherResourcesCommand : BaseGameCommand
{
    private readonly string _locationSpotId;
    private readonly ItemRepository _itemRepository;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();


    public GatherResourcesCommand(
        string locationSpotId,
        ItemRepository itemRepository,
        MessageSystem messageSystem)
    {
        _locationSpotId = locationSpotId ?? throw new ArgumentNullException(nameof(locationSpotId));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Gather resources at location";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player is at a location
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Not at any location");
        }

        // Check if at the right spot
        if (player.CurrentLocationSpot.SpotID != _locationSpotId)
        {
            return CommandValidationResult.Failure("Not at the gathering location");
        }

        // Check if location supports gathering (only FEATURE spots)
        if (player.CurrentLocationSpot.Type != LocationSpotTypes.FEATURE)
        {
            return CommandValidationResult.Failure("This location doesn't have resources to gather");
        }

        // Time cost check removed - handled by executing service

        // Check stamina cost (2 stamina)
        if (player.Stamina < 2)
        {
            return CommandValidationResult.Failure(
                "Not enough stamina (need 2)",
                true,
                "Rest to recover stamina");
        }

        // Check inventory space (at least 1 slot)
        if (player.Inventory.UsedCapacity >= player.Inventory.Size)
        {
            return CommandValidationResult.Failure(
                "Inventory is full",
                true,
                "Sell or drop some items");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        LocationSpot spot = player.CurrentLocationSpot;

        // Time spending handled by executing service
        // Stamina cost still applied here
        player.ModifyStamina(-2);

        // Determine what resources can be gathered based on location
        List<Item> availableResources = GetGatherableResources(spot);

        if (!availableResources.Any())
        {
            _messageSystem.AddSystemMessage(
                "You search the area but find nothing of value.",
                SystemMessageTypes.Info
            );
            return CommandResult.Success("No resources found");
        }

        // Gather 1-3 resources based on luck
        int gatherCount = _random.Next(1, 4);
        gatherCount = Math.Min(gatherCount, player.Inventory.Size - player.Inventory.UsedCapacity);

        List<Item> gatheredItems = new List<Item>();
        for (int i = 0; i < gatherCount; i++)
        {
            Item resource = availableResources[_random.Next(availableResources.Count)];

            // Create a copy of the item for the player's inventory
            Item gatheredItem = new Item
            {
                Id = resource.Id,
                Name = resource.Name,
                Description = resource.Description,
                BuyPrice = resource.BuyPrice,
                SellPrice = resource.SellPrice,
                Weight = resource.Weight,
                InventorySlots = resource.InventorySlots,
                Categories = new List<ItemCategory>(resource.Categories),
                Size = resource.Size
            };

            player.Inventory.Add(gatheredItem);
            gatheredItems.Add(gatheredItem);
        }

        // Success message
        string itemList = string.Join(", ", gatheredItems.Select(i => i.Name));
        _messageSystem.AddSystemMessage(
            $"🌿 Gathered: {itemList}",
            SystemMessageTypes.Success
        );

        return CommandResult.Success(
            "Resources gathered successfully",
            new
            {
                Location = spot.Name,
                ItemsGathered = gatheredItems.Select(i => new { i.Name, i.SellPrice }),
                StaminaSpent = 2,
                TimeCost = 1  // Add time cost to result
            }
        );
    }


    private List<Item> GetGatherableResources(LocationSpot spot)
    {
        // Get all materials and food items that could be gathered
        List<Item> allItems = _itemRepository.GetAllItems();

        return allItems.Where(item =>
            item.Categories.Contains(ItemCategory.Materials) ||
            item.Categories.Contains(ItemCategory.Food) ||
            item.Categories.Contains(ItemCategory.Medicine)
        ).Where(item =>
            // Only small/tiny items can be gathered
            item.Size == SizeCategory.Tiny || item.Size == SizeCategory.Small
        ).ToList();
    }
}
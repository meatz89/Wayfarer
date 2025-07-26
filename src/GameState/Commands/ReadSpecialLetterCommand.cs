using System;
using System.Threading.Tasks;

/// <summary>
/// Command to read special letter items like the mysterious gold-sealed letter
/// </summary>
public class ReadSpecialLetterCommand : BaseGameCommand
{
    private readonly string _itemId;
    private readonly ItemRepository _itemRepository;
    private readonly FlagService _flagService;

    public ReadSpecialLetterCommand(string itemId, ItemRepository itemRepository, FlagService flagService)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        _itemId = itemId;
        _itemRepository = itemRepository;
        _flagService = flagService;

        Description = $"Read special letter: {_itemId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        
        // Check if the player has the item
        if (!player.Inventory.HasItem(_itemId))
        {
            return CommandValidationResult.Failure(
                $"You don't have '{_itemId}' in your inventory",
                canBeRemedied: false
            );
        }

        // Get the item and check if it's readable
        Item item = _itemRepository.GetItemById(_itemId);
        if (item == null)
        {
            return CommandValidationResult.Failure(
                $"Item '{_itemId}' not found",
                canBeRemedied: false
            );
        }

        if (!item.IsReadable())
        {
            return CommandValidationResult.Failure(
                $"'{item.Name}' cannot be read",
                canBeRemedied: false
            );
        }

        return CommandValidationResult.Success();
    }

    public override Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.Failure(validation.FailureReason));
        }

        Item item = _itemRepository.GetItemById(_itemId);

        // Set the flag if specified
        if (!string.IsNullOrEmpty(item.ReadFlagToSet))
        {
            _flagService.SetFlag(item.ReadFlagToSet, true);
        }

        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage($"Read {item.Name}"));

        var content = !string.IsNullOrEmpty(item.ReadableContent) 
            ? item.ReadableContent 
            : "The letter contains no readable text.";

        return Task.FromResult(CommandResult.Success(
            $"You carefully break the seal and read the letter...",
            new
            {
                ItemId = _itemId,
                ItemName = item.Name,
                Content = content,
                FlagSet = item.ReadFlagToSet
            }
        ));
    }
}
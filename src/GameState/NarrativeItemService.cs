using System;
using System.Threading.Tasks;

/// <summary>
/// Service that handles giving items to players based on narrative events
/// </summary>
public class NarrativeItemService
{
    private readonly FlagService _flagService;
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly CommandExecutor _commandExecutor;

    public NarrativeItemService(
        FlagService flagService,
        GameWorld gameWorld,
        ItemRepository itemRepository,
        CommandExecutor commandExecutor)
    {
        _flagService = flagService;
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _commandExecutor = commandExecutor;
    }

    /// <summary>
    /// Check for narrative item flags and give items to player
    /// </summary>
    public async Task CheckAndGiveNarrativeItems()
    {
        // Check for specific narrative_give_item_ flags
        // Since FlagService doesn't expose all flags, we need to check known item flags
        var knownNarrativeItems = new[] { "mysterious_letter", "patron_letter", "elena_keepsake" };
        
        foreach (var itemId in knownNarrativeItems)
        {
            var flagName = $"narrative_give_item_{itemId}";
            if (_flagService.HasFlag(flagName))
            {
                // Check if item exists
                var item = _itemRepository.GetItemById(itemId);
                if (item != null)
                {
                    // Give item to player
                    var command = new ModifyInventoryCommand(
                        itemId, 
                        ModifyInventoryCommand.InventoryOperation.Add,
                        "narrative gift"
                    );
                    
                    var result = await _commandExecutor.ExecuteAsync(command);
                    
                    if (result.IsSuccess)
                    {
                        // Clear the flag so we don't give it again
                        _flagService.SetFlag(flagName, false);
                        
                        // Add a system message about receiving the item
                        _gameWorld.SystemMessages.Add(new SystemMessage(
                            $"You received: {item.Name}\n{item.Description}",
                            SystemMessageTypes.Tutorial,
                            7000
                        ));
                    }
                }
            }
        }
    }
}
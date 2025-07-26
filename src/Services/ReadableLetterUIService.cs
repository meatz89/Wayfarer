using System;
using System.Threading.Tasks;

/// <summary>
/// Service for handling UI display of readable letters and special documents
/// </summary>
public class ReadableLetterUIService
{
    private readonly ItemRepository _itemRepository;
    private readonly FlagService _flagService;
    private readonly CommandExecutor _commandExecutor;
    private readonly MessageSystem _messageSystem;

    public ReadableLetterUIService(
        ItemRepository itemRepository,
        FlagService flagService,
        CommandExecutor commandExecutor,
        MessageSystem messageSystem)
    {
        _itemRepository = itemRepository;
        _flagService = flagService;
        _commandExecutor = commandExecutor;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check if an item can be read
    /// </summary>
    public bool CanReadItem(string itemId)
    {
        var item = _itemRepository.GetItemById(itemId);
        return item != null && item.IsReadable();
    }

    /// <summary>
    /// Read a special letter item and display its contents
    /// </summary>
    public async Task<bool> ReadLetterAsync(string itemId)
    {
        var item = _itemRepository.GetItemById(itemId);
        if (item == null || !item.IsReadable())
        {
            return false;
        }

        // Create and execute the read command
        var command = new ReadSpecialLetterCommand(itemId, _itemRepository, _flagService);
        var result = await _commandExecutor.ExecuteAsync(command);

        if (result.IsSuccess)
        {
            // Show message about reading the letter
            _messageSystem.AddSystemMessage($"You carefully read the {item.Name}...", SystemMessageTypes.Info);

            // Show any special effects or notifications
            if (!string.IsNullOrEmpty(item.ReadFlagToSet))
            {
                _messageSystem.AddSystemMessage("The letter's contents give you pause. This could change everything...", SystemMessageTypes.Tutorial);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Get display information for a readable item
    /// </summary>
    public ReadableLetterInfo GetLetterInfo(string itemId)
    {
        var item = _itemRepository.GetItemById(itemId);
        if (item == null || !item.IsReadable())
        {
            return null;
        }

        return new ReadableLetterInfo
        {
            ItemId = item.Id,
            Name = item.Name,
            Description = item.Description,
            IsRead = !string.IsNullOrEmpty(item.ReadFlagToSet) && _flagService.HasFlag(item.ReadFlagToSet),
            HasSpecialEffect = !string.IsNullOrEmpty(item.ReadFlagToSet)
        };
    }
}

public class ReadableLetterInfo
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRead { get; set; }
    public bool HasSpecialEffect { get; set; }
}
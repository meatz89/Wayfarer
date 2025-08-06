/// <summary>
/// Service that handles giving items to players based on narrative events
/// </summary>
public class NarrativeItemService
{
    private readonly FlagService _flagService;
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly MessageSystem _messageSystem;

    public NarrativeItemService(
        FlagService flagService,
        GameWorld gameWorld,
        ItemRepository itemRepository,
        MessageSystem messageSystem)
    {
        _flagService = flagService;
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check for narrative item flags and give items to player
    /// </summary>
    public void CheckAndGiveNarrativeItems()
    {
        // Check for specific narrative_give_item_ flags
        string[] knownNarrativeItems = new[] { "mysterious_letter", "patron_letter", "elena_keepsake" };
        Player player = _gameWorld.GetPlayer();

        foreach (string? itemId in knownNarrativeItems)
        {
            string flagName = $"narrative_give_item_{itemId}";
            if (_flagService.HasFlag(flagName))
            {
                // Check if item exists
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && player.Inventory.TryAddItem(itemId))
                {
                    // Clear the flag so we don't give it again
                    _flagService.SetFlag(flagName, false);

                    // Add a system message about receiving the item
                    _messageSystem.AddSystemMessage(
                        $"You received: {item.Name}\n{item.Description}",
                        SystemMessageTypes.Tutorial
                    );
                }
            }
        }
    }
}
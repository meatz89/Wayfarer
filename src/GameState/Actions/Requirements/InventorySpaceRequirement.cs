namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement that checks if player has enough inventory space
/// </summary>
public class InventorySpaceRequirement : IActionRequirement
{
    private readonly int _slotsRequired;
    private readonly string _forItemDescription;
    private readonly ItemRepository _itemRepository;
    
    public InventorySpaceRequirement(int slotsRequired, string forItemDescription, ItemRepository itemRepository)
    {
        _slotsRequired = slotsRequired;
        _forItemDescription = forItemDescription;
        _itemRepository = itemRepository;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        var availableSlots = player.Inventory.GetAvailableSlots(_itemRepository);
        return availableSlots >= _slotsRequired;
    }
    
    public string GetDescription()
    {
        return $"{_slotsRequired} inventory slot{(_slotsRequired > 1 ? "s" : "")} for {_forItemDescription}";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        var availableSlots = player.Inventory.GetAvailableSlots(_itemRepository);
        return $"Not enough inventory space! Need {_slotsRequired} slots, have {availableSlots} available";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        return "Drop items or equipment to free up inventory space";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        var availableSlots = player.Inventory.GetAvailableSlots(_itemRepository);
        if (availableSlots >= _slotsRequired) return 1.0;
        return (double)availableSlots / _slotsRequired;
    }
}
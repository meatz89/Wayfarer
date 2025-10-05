public class EquipmentRequirement
{
    public List<string> RequiredEquipment { get; set; } = new List<string>();
    public List<string> RequiredActions { get; set; } = new List<string>();

    public bool MeetsRequirements(Player player, ItemRepository itemRepository)
    {
        foreach (string equipmentId in RequiredEquipment)
        {
            if (!player.Inventory.HasItem(equipmentId))
            {
                return false;
            }
        }

        foreach (string action in RequiredActions)
        {
            if (!HasEnabledAction(player, action, itemRepository))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasEnabledAction(Player player, string action, ItemRepository itemRepository)
    {
        List<string> itemIds = player.Inventory.GetAllItems();
        foreach (string itemId in itemIds)
        {
            Item item = itemRepository.GetItemById(itemId);
            if (item is Equipment equipment && equipment.EnablesAction(action))
            {
                return true;
            }
        }
        return false;
    }

    public List<string> GetMissingRequirements(Player player, ItemRepository itemRepository)
    {
        List<string> missing = new List<string>();

        foreach (string equipmentId in RequiredEquipment)
        {
            if (!player.Inventory.HasItem(equipmentId))
            {
                missing.Add($"Equipment: {equipmentId}");
            }
        }

        foreach (string action in RequiredActions)
        {
            if (!HasEnabledAction(player, action, itemRepository))
            {
                missing.Add($"Action: {action}");
            }
        }

        return missing;
    }
}

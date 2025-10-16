public class EquipmentRequirement
{
    public List<string> RequiredEquipment { get; set; } = new List<string>();

    public bool MeetsRequirements(Player player, ItemRepository itemRepository)
    {
        foreach (string equipmentId in RequiredEquipment)
        {
            if (!player.Inventory.HasItem(equipmentId))
            {
                return false;
            }
        }

        return true;
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

        return missing;
    }
}

public static class ItemParser
{
    /// <summary>
    /// Convert an ItemDTO to an Item domain model (or Equipment if it has ApplicableContexts)
    /// </summary>
    public static Item ConvertDTOToItem(ItemDTO dto)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException("Item missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Item '{dto.Name}' missing required field 'Description'");

        Item item = new Item
        {
            Name = dto.Name,
            InitiativeCost = dto.InitiativeCost,
            BuyPrice = dto.BuyPrice,
            SellPrice = dto.SellPrice,
            Description = dto.Description
        };

        // Parse item categories - DTO has inline init, trust it
        foreach (string categoryStr in dto.Categories)
        {
            if (EnumParser.TryParse<ItemCategory>(categoryStr, out ItemCategory category))
            {
                item.Categories.Add(category);
            }
        }

        // Parse enhanced categorical properties - optional field, defaults if missing/invalid
        if (!string.IsNullOrEmpty(dto.SizeCategory))
        {
            if (EnumParser.TryParse<SizeCategory>(dto.SizeCategory, out SizeCategory size))
            {
                item.Size = size;
            }
        }

        // Parse token generation bonuses - DTO has inline init, trust it
        foreach (KeyValuePair<string, int> kvp in dto.TokenGenerationBonuses)
        {
            if (Enum.TryParse<ConnectionType>(kvp.Key, out ConnectionType connectionType))
            {
                item.TokenGenerationBonuses[connectionType] = kvp.Value;
            }
        }

        // Parse enabled token generation - DTO has inline init, trust it
        foreach (string tokenType in dto.EnablesTokenGeneration)
        {
            if (Enum.TryParse<ConnectionType>(tokenType, out ConnectionType connectionType))
            {
                item.EnablesTokenGeneration.Add(connectionType);
            }
        }

        return item;
    }

}
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ItemParser
{
    /// <summary>
    /// Convert an ItemDTO to an Item domain model
    /// </summary>
    public static Item ConvertDTOToItem(ItemDTO dto)
    {
        Item item = new Item
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Focus = dto.Focus,
            BuyPrice = dto.BuyPrice,
            SellPrice = dto.SellPrice,
            Description = dto.Description ?? ""
        };

        // Parse item categories
        if (dto.Categories != null)
        {
            foreach (string categoryStr in dto.Categories)
            {
                if (EnumParser.TryParse<ItemCategory>(categoryStr, out ItemCategory category))
                {
                    item.Categories.Add(category);
                }
            }
        }

        // Parse enhanced categorical properties
        if (!string.IsNullOrEmpty(dto.SizeCategory))
        {
            if (EnumParser.TryParse<SizeCategory>(dto.SizeCategory, out SizeCategory size))
            {
                item.Size = size;
            }
        }

        // Parse token generation modifiers
        if (dto.TokenGenerationModifiers != null)
        {
            foreach (KeyValuePair<string, float> kvp in dto.TokenGenerationModifiers)
            {
                if (Enum.TryParse<ConnectionType>(kvp.Key, out ConnectionType connectionType))
                {
                    item.TokenGenerationModifiers[connectionType] = kvp.Value;
                }
            }
        }

        // Parse enabled token generation
        if (dto.EnablesTokenGeneration != null)
        {
            foreach (string tokenType in dto.EnablesTokenGeneration)
            {
                if (Enum.TryParse<ConnectionType>(tokenType, out ConnectionType connectionType))
                {
                    item.EnablesTokenGeneration.Add(connectionType);
                }
            }
        }

        return item;
    }

}
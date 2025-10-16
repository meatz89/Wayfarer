using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.GameState.Enums;

public static class ItemParser
{
    /// <summary>
    /// Convert an ItemDTO to an Item domain model (or Equipment if it has EnabledActions)
    /// </summary>
    public static Item ConvertDTOToItem(ItemDTO dto)
    {
        Item item = new Item
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            InitiativeCost = dto.InitiativeCost,
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

        // If EnabledActions present, convert to Equipment
        if (dto.EnabledActions != null && dto.EnabledActions.Count > 0)
        {
            // Parse contexts from JSON strings to enum
            List<ObstacleContext> applicableContexts = new List<ObstacleContext>();
            if (dto.ApplicableContexts != null)
            {
                foreach (string contextString in dto.ApplicableContexts)
                {
                    if (Enum.TryParse<ObstacleContext>(contextString, ignoreCase: true, out ObstacleContext context))
                    {
                        applicableContexts.Add(context);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Invalid context '{contextString}' in equipment '{dto.Id}'. " +
                            $"Must be one of: {string.Join(", ", Enum.GetNames<ObstacleContext>())}");
                    }
                }
            }

            // Parse usage type
            EquipmentUsageType usageType = EquipmentUsageType.Permanent;
            if (!string.IsNullOrEmpty(dto.UsageType))
            {
                if (Enum.TryParse<EquipmentUsageType>(dto.UsageType, ignoreCase: true, out EquipmentUsageType parsedUsageType))
                {
                    usageType = parsedUsageType;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Invalid usage type '{dto.UsageType}' in equipment '{dto.Id}'. " +
                        $"Must be one of: {string.Join(", ", Enum.GetNames<EquipmentUsageType>())}");
                }
            }

            Equipment equipment = Equipment.FromItem(
                item,
                dto.EnabledActions,
                applicableContexts,
                dto.IntensityReduction ?? 0
            );
            equipment.UsageType = usageType;
            return equipment;
        }

        return item;
    }

}
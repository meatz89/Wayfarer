using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Wayfarer.GameState.Enums;

public static class ItemParser
{
    /// <summary>
    /// Convert an ItemDTO to an Item domain model (or Equipment if it has ApplicableContexts)
    /// </summary>
    public static Item ConvertDTOToItem(ItemDTO dto)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("Item missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"Item '{dto.Id}' missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Item '{dto.Id}' missing required field 'Description'");

        Item item = new Item
        {
            Id = dto.Id,
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

        // Parse token generation modifiers - DTO has inline init, trust it
        foreach (KeyValuePair<string, float> kvp in dto.TokenGenerationModifiers)
        {
            if (Enum.TryParse<ConnectionType>(kvp.Key, out ConnectionType connectionType))
            {
                item.TokenGenerationModifiers[connectionType] = kvp.Value;
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

        // If ApplicableContexts or IntensityReduction present, convert to Equipment
        // IntensityReduction is optional - defaults to 0 if missing
        if (dto.ApplicableContexts.Count > 0 || (dto.IntensityReduction ?? 0) > 0)
        {
            // Parse contexts from JSON strings to enum - DTO has inline init, trust it
            List<ObstacleContext> applicableContexts = new List<ObstacleContext>();
            foreach (string contextString in dto.ApplicableContexts)
            {
                if (Enum.TryParse<ObstacleContext>(contextString, ignoreCase: true, out ObstacleContext context))
                {
                    applicableContexts.Add(context);
                }
                else
                {
                    throw new InvalidDataException(
                        $"Invalid context '{contextString}' in equipment '{dto.Id}'. " +
                        $"Must be one of: {string.Join(", ", Enum.GetNames<ObstacleContext>())}");
                }
            }

            // Parse usage type - optional field, defaults to Permanent
            EquipmentUsageType usageType = EquipmentUsageType.Permanent;
            if (!string.IsNullOrEmpty(dto.UsageType))
            {
                if (Enum.TryParse<EquipmentUsageType>(dto.UsageType, ignoreCase: true, out EquipmentUsageType parsedUsageType))
                {
                    usageType = parsedUsageType;
                }
                else
                {
                    throw new InvalidDataException(
                        $"Invalid usage type '{dto.UsageType}' in equipment '{dto.Id}'. " +
                        $"Must be one of: {string.Join(", ", Enum.GetNames<EquipmentUsageType>())}");
                }
            }

            Equipment equipment = Equipment.FromItem(
                item,
                applicableContexts,
                dto.IntensityReduction ?? 0 // Optional - defaults to 0 if missing
            );
            equipment.UsageType = usageType;

            // Parse exhaustion fields (for Exhaustible equipment only)
            if (usageType == EquipmentUsageType.Exhaustible)
            {
                // Parse categorical durability property → translate to mechanical values via catalogue
                if (!string.IsNullOrEmpty(dto.Durability))
                {
                    if (EnumParser.TryParse<DurabilityType>(dto.Durability, out DurabilityType durability))
                    {
                        (int uses, int repairCost) = EquipmentDurabilityCatalog.GetDurabilityValues(durability);
                        equipment.ExhaustAfterUses = uses;
                        equipment.RepairCost = repairCost;
                    }
                    else
                    {
                        throw new InvalidDataException(
                            $"Invalid durability '{dto.Durability}' in equipment '{dto.Id}'. " +
                            $"Must be one of: {string.Join(", ", Enum.GetNames<DurabilityType>())}");
                    }
                }
                else
                {
                    throw new InvalidDataException(
                        $"Exhaustible equipment '{dto.Id}' missing required field 'Durability'. " +
                        $"Must specify: {string.Join(", ", Enum.GetNames<DurabilityType>())}");
                }

                equipment.CurrentUses = 0; // Always start fresh
                equipment.CurrentState = EquipmentState.Functional; // Always start functional
            }

            return equipment;
        }

        return item;
    }

}
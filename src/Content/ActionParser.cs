using System.Text.Json;
using Wayfarer.Game.ActionSystem;

public static class ActionParser
{
    public static ActionDefinition ParseAction(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "id");
        string name = GetStringProperty(root, "name", id);
        string spotId = GetStringProperty(root, "locationSpotId", "");
        string description = GetStringProperty(root, "description", "");

        ActionDefinition action = new ActionDefinition(id, name, spotId)
        {
            Description = description,
            SilverCost = GetIntProperty(root, "silverCost", 0),
            StaminaCost = GetIntProperty(root, "staminaCost", 0),
            ConcentrationCost = GetIntProperty(root, "concentrationCost", 0)
        };

        // Parse refresh card type if present
        string refreshCardType = GetStringProperty(root, "refreshCardType", "");
        if (!string.IsNullOrEmpty(refreshCardType))
        {
            if (Enum.TryParse(refreshCardType, true, out SkillCategories cardType))
            {
                action.RefreshCardType = cardType;
            }
        }

        // Parse time windows
        if (root.TryGetProperty("CurrentTimeBlocks", out JsonElement CurrentTimeBlocksElement) &&
            CurrentTimeBlocksElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement windowElement in CurrentTimeBlocksElement.EnumerateArray())
            {
                if (windowElement.ValueKind == JsonValueKind.String)
                {
                    string windowStr = windowElement.GetString();
                    if (!string.IsNullOrEmpty(windowStr))
                    {
                        if (Enum.TryParse(windowStr, true, out TimeBlocks windowType))
                        {
                            action.CurrentTimeBlocks.Add(windowType);
                        }
                    }
                }
            }

            // If no time windows are specified, default to all
            if (action.CurrentTimeBlocks.Count == 0)
            {
                action.CurrentTimeBlocks.Add(TimeBlocks.Morning);
                action.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
                action.CurrentTimeBlocks.Add(TimeBlocks.Evening);
                action.CurrentTimeBlocks.Add(TimeBlocks.Night);
            }
        }

        // Parse tag resonance properties
        action.ContextTags = GetStringArray(root, "contextTags");
        action.DomainTags = GetStringArray(root, "domainTags");

        // Parse movement details if present
        action.MoveToLocation = GetStringProperty(root, "moveToLocation", null);
        action.MoveToLocationSpot = GetStringProperty(root, "moveToLocationSpot", null);

        // Parse categorical properties
        ParseCategoricalProperties(root, action);

        return action;
    }

    // Property getters
    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    private static List<string> GetStringArray(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }

        return results;
    }

    private static void ParseCategoricalProperties(JsonElement root, ActionDefinition action)
    {
        // Parse physical demand
        string physicalDemand = GetStringProperty(root, "physicalDemand", "None");
        if (Enum.TryParse(physicalDemand, true, out PhysicalDemand demand))
        {
            action.PhysicalDemand = demand;
        }

        // Parse knowledge requirement
        string knowledgeRequirement = GetStringProperty(root, "knowledgeRequirement", "None");
        if (Enum.TryParse(knowledgeRequirement, true, out KnowledgeRequirement knowledge))
        {
            action.KnowledgeRequirement = knowledge;
        }

        // Parse time investment
        string timeInvestment = GetStringProperty(root, "timeInvestment", "Standard");
        if (Enum.TryParse(timeInvestment, true, out TimeInvestment time))
        {
            action.TimeInvestment = time;
        }

        // Parse tool requirements array
        if (root.TryGetProperty("toolRequirements", out JsonElement toolsElement) &&
            toolsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement toolElement in toolsElement.EnumerateArray())
            {
                if (toolElement.ValueKind == JsonValueKind.String)
                {
                    string toolStr = toolElement.GetString();
                    if (!string.IsNullOrEmpty(toolStr))
                    {
                        // Try to parse as ToolCategory first
                        if (Enum.TryParse(toolStr, true, out ToolCategory tool))
                        {
                            action.ToolRequirements.Add(tool);
                        }
                        // If that fails, try to parse as EquipmentCategory
                        else if (Enum.TryParse(toolStr, true, out EquipmentCategory equipment))
                        {
                            action.EquipmentRequirements.Add(equipment);
                        }
                    }
                }
            }
        }

        // Parse equipment requirements array (if explicitly specified)
        if (root.TryGetProperty("equipmentRequirements", out JsonElement equipmentElement) &&
            equipmentElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement equipElement in equipmentElement.EnumerateArray())
            {
                if (equipElement.ValueKind == JsonValueKind.String)
                {
                    string equipStr = equipElement.GetString();
                    if (!string.IsNullOrEmpty(equipStr) &&
                        Enum.TryParse(equipStr, true, out EquipmentCategory equipment))
                    {
                        action.EquipmentRequirements.Add(equipment);
                    }
                }
            }
        }

        // Parse effect categories array
        if (root.TryGetProperty("effectCategories", out JsonElement effectsElement) &&
            effectsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement effectElement in effectsElement.EnumerateArray())
            {
                if (effectElement.ValueKind == JsonValueKind.String)
                {
                    string effectStr = effectElement.GetString();
                    if (!string.IsNullOrEmpty(effectStr) &&
                        Enum.TryParse(effectStr, true, out EffectCategory effect))
                    {
                        action.EffectCategories.Add(effect);
                    }
                }
            }
        }
    }
}
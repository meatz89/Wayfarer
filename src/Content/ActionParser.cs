using System.Text.Json;

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
            ActionPointCost = GetIntProperty(root, "actionPointCost", 1),
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
        if (root.TryGetProperty("timeWindows", out JsonElement timeWindowsElement) &&
            timeWindowsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement windowElement in timeWindowsElement.EnumerateArray())
            {
                if (windowElement.ValueKind == JsonValueKind.String)
                {
                    string windowStr = windowElement.GetString();
                    if (!string.IsNullOrEmpty(windowStr))
                    {
                        if (Enum.TryParse(windowStr, true, out TimeBlocks windowType))
                        {
                            action.TimeWindows.Add(windowType);
                        }
                    }
                }
            }

            // If no time windows are specified, default to all
            if (action.TimeWindows.Count == 0)
            {
                action.TimeWindows.Add(TimeBlocks.Morning);
                action.TimeWindows.Add(TimeBlocks.Afternoon);
                action.TimeWindows.Add(TimeBlocks.Evening);
                action.TimeWindows.Add(TimeBlocks.Night);
            }
        }

        // Parse tag resonance properties
        action.ContextTags = GetStringArray(root, "contextTags");
        action.DomainTags = GetStringArray(root, "domainTags");

        // Parse movement details if present
        action.MoveToLocation = GetStringProperty(root, "moveToLocation", null);
        action.MoveToLocationSpot = GetStringProperty(root, "moveToLocationSpot", null);

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
}
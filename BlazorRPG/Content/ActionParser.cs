using System.Text.Json;

public static class ActionParser
{
    public static ActionDefinition ParseAction(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "id");
        string name = GetStringProperty(root, "name", id);
        string spotId = GetStringProperty(root, "spotId", "spotId");

        ActionDefinition action = new ActionDefinition(id, name, spotId)
        {
            Description = GetStringProperty(root, "description", ""),
        };

        // Parse approaches
        if (root.TryGetProperty("approaches", out JsonElement approachesElement) &&
            approachesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement approachElement in approachesElement.EnumerateArray())
            {
                string approachId = GetStringProperty(approachElement, "id", "");
                string approachName = GetStringProperty(approachElement, "name", "");

                Approach approach = new Approach(approachId, approachName)
                {
                    Description = GetStringProperty(approachElement, "description", ""),
                    CardType = GetStringProperty(approachElement, "cardType", ""),
                    Skill = GetStringProperty(approachElement, "skill", ""),
                    Difficulty = GetIntProperty(approachElement, "difficulty", 0)
                };

                // Parse rewards
                if (approachElement.TryGetProperty("rewards", out JsonElement rewardsElement) &&
                    rewardsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty reward in rewardsElement.EnumerateObject())
                    {
                        if (reward.Value.ValueKind == JsonValueKind.Number &&
                            reward.Value.TryGetInt32(out int rewardValue))
                        {
                            approach.Rewards.Add(reward.Name, rewardValue);
                        }
                    }
                }

                action.Approaches.Add(approach);
            }
        }

        // Parse requirements
        if (root.TryGetProperty("requirements", out JsonElement requirementsElement))
        {
            // Relationship level requirement
            action.RelationshipLevel = GetIntProperty(requirementsElement, "relationshipLevel", 0);

            // Resource requirements
            if (requirementsElement.TryGetProperty("resources", out JsonElement resourcesElement))
            {
                if (resourcesElement.TryGetProperty("COINS", out JsonElement coinsElement) &&
                    coinsElement.ValueKind == JsonValueKind.Number)
                {
                    action.CoinCost = coinsElement.GetInt32();
                }

                if (resourcesElement.TryGetProperty("FOOD", out JsonElement foodElement) &&
                    foodElement.ValueKind == JsonValueKind.Number)
                {
                    action.FoodCost = foodElement.GetInt32();
                }
            }
        }

        // Parse time windows - UPDATED for new TimeWindows structure
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
                        // Map string to TimeWindowTypes enum
                        if (Enum.TryParse<TimeWindowTypes>(windowStr, true, out TimeWindowTypes windowType))
                        {
                            action.TimeWindows.Add(windowType);
                        }
                    }
                }
            }

            // If no time windows are specified, default to all
            if (action.TimeWindows.Count == 0)
            {
                action.TimeWindows.Add(TimeWindowTypes.Morning);
                action.TimeWindows.Add(TimeWindowTypes.Afternoon);
                action.TimeWindows.Add(TimeWindowTypes.Evening);
                action.TimeWindows.Add(TimeWindowTypes.Night);
            }
        }

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
}
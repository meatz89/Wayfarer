using System;
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

        // Parse approach
        string approachString = GetStringProperty(root, "approach", "NEUTRAL");
        if (Enum.TryParse(approachString, true, out EncounterApproaches approach))
        {
            action.EncounterApproach = approach;
        }

        // Parse requirements
        if (root.TryGetProperty("requirements", out JsonElement requirementsElement))
        {
            // Relationship level requirement
            action.RelationshipLevel = GetIntProperty(requirementsElement, "relationshipLevel", 0);

            // Resource requirements
            if (requirementsElement.TryGetProperty("resources", out JsonElement resourcesElement))
            {
                action.CoinCost = GetIntProperty(resourcesElement, "COINS", 0);
                action.FoodCost = GetIntProperty(resourcesElement, "FOOD", 0);
            }
        }

        // Parse grants
        if (root.TryGetProperty("grants", out JsonElement grantsElement))
        {
            action.SpotXP = GetIntProperty(grantsElement, "spotXP", 0);
        }

        // Parse recovery
        if (root.TryGetProperty("recovery", out JsonElement recoveryElement))
        {
            foreach (JsonProperty property in recoveryElement.EnumerateObject())
            {
                string recoveryType = property.Value.GetString() ?? string.Empty;
                bool canParse = Enum.TryParse(recoveryType, true, out RecoveryLevels recoveryLevel);
                if (!canParse) continue;

                switch (property.Name)
                {
                    case "HUNGER":
                        action.HungerRecovery = recoveryLevel;
                        break;
                    case "ENERGY":
                        action.EnergyRecovery = recoveryLevel;
                        break;
                    case "EXHAUSTION":
                        action.ExhaustionRecovery = recoveryLevel;
                        break;
                    case "MENTAL_STRAIN":
                        action.MentalStrainRecovery = recoveryLevel;
                        break;
                    case "ISOLATION":
                        action.IsolationRecovery = recoveryLevel;
                        break;
                }
            }
        }

        // Parse characteristics
        if (root.TryGetProperty("characteristics", out JsonElement characteristicsElement))
        {
            string exertion = GetStringProperty(characteristicsElement, "exertion", "LOW");
            string mentalLoad = GetStringProperty(characteristicsElement, "mentalLoad", "LOW");
            string socialImpact = GetStringProperty(characteristicsElement, "socialImpact", "SOLITARY");

            if (Enum.TryParse(exertion, true, out ExertionLevels exertionLevel))
                action.Exertion = exertionLevel;
            if (Enum.TryParse(mentalLoad, true, out MentalLoadLevels mentalLoadLevel))
                action.MentalLoad = mentalLoadLevel;
            if (Enum.TryParse(socialImpact, true, out SocialImpactTypes socialImpactType))
                action.SocialImpact = socialImpactType;
        }

        // Parse time windows
        if (root.TryGetProperty("timeWindows", out JsonElement timeWindowsElement) &&
            timeWindowsElement.ValueKind == JsonValueKind.Array)
        {
            List<TimeWindows> allowedWindows = new List<TimeWindows>();
            foreach (JsonElement windowElement in timeWindowsElement.EnumerateArray())
            {
                if (windowElement.ValueKind == JsonValueKind.String &&
                    Enum.TryParse(windowElement.GetString(), true, out TimeWindows window))
                {
                    allowedWindows.Add(window);
                }
            }

            if (allowedWindows.Count > 0)
            {
                action.TimeWindows = allowedWindows;
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
using System.Text.Json;

public static class ContentParser
{
    public static Location ParseLocation(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        Location location = new Location(id)
        {
            Name = GetStringProperty(root, "name", id),
            Description = GetStringProperty(root, "description", ""),
            DetailedDescription = GetStringProperty(root, "detailedDescription", ""),
            ConnectedTo = GetStringArray(root, "connectedTo")
        };

        return location;
    }

    public static LocationSpot ParseLocationSpot(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string name = GetStringProperty(root, "name", "");
        string locationId = GetStringProperty(root, "locationId", "");

        LocationSpot spot = new LocationSpot(name, locationId)
        {
            Description = GetStringProperty(root, "description", ""),
            CurrentLevel = GetIntProperty(root, "currentLevel", 1),
            CurrentSpotXP = GetIntProperty(root, "currentXP", 0),
            XPToNextLevel = GetIntProperty(root, "xpToNextLevel", 100)
        };

        // Parse levels
        spot.LevelData = new List<SpotLevel>();
        if (root.TryGetProperty("levels", out JsonElement levelsArray) &&
            levelsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement levelElement in levelsArray.EnumerateArray())
            {
                SpotLevel level = new SpotLevel
                {
                    Level = GetIntProperty(levelElement, "level", 1),
                    Description = GetStringProperty(levelElement, "description", ""),
                    AddedActionIds = GetStringArray(levelElement, "actionIds")
                };

                // Parse level-up encounter if present
                if (levelElement.TryGetProperty("levelUpEncounter", out JsonElement encounterElement))
                {
                    level.EncounterActionId = GetStringProperty(encounterElement, "id", "");
                }

                // Parse removed actions
                level.RemovedActionIds = GetStringArray(levelElement, "removedActionIds");

                spot.LevelData.Add(level);
            }
        }

        return spot;
    }

    public static ActionDefinition ParseAction(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        string name = GetStringProperty(root, "name", id);

        ActionDefinition action = new ActionDefinition(id, name)
        {
            Description = GetStringProperty(root, "description", ""),
            Goal = GetStringProperty(root, "goal", ""),
            Complication = GetStringProperty(root, "complication", "")
        };

        // Parse time windows
        if (root.TryGetProperty("timeWindows", out JsonElement timeWindowsArray) &&
            timeWindowsArray.ValueKind == JsonValueKind.Array)
        {
            action.TimeWindows = new List<TimeWindow>();
            foreach (JsonElement windowElement in timeWindowsArray.EnumerateArray())
            {
                if (windowElement.ValueKind == JsonValueKind.String &&
                    Enum.TryParse<TimeWindow>(windowElement.GetString(), out TimeWindow window))
                {
                    action.TimeWindows.Add(window);
                }
            }
        }

        // Parse costs
        if (root.TryGetProperty("costs", out JsonElement costsElement))
        {
            action.CoinCost = GetIntProperty(costsElement, "coin", 0);
            action.EnergyCost = GetIntProperty(costsElement, "energy", 0);
            action.ConcentrationCost = GetIntProperty(costsElement, "concentration", 0);
            action.ConfidenceCost = GetIntProperty(costsElement, "confidence", 0);
        }

        // Parse yields
        if (root.TryGetProperty("yields", out JsonElement yieldsElement))
        {
            action.CoinGain = GetIntProperty(yieldsElement, "coin", 0);
            action.RestoresEnergy = GetIntProperty(yieldsElement, "energy", 0);
            action.RestoresHealth = GetIntProperty(yieldsElement, "health", 0);
            action.RestoresConcentration = GetIntProperty(yieldsElement, "concentration", 0);
            action.RestoresConfidence = GetIntProperty(yieldsElement, "confidence", 0);
            action.SpotXp = GetIntProperty(yieldsElement, "spotXp", 0);

            // Parse relationship gains
            if (yieldsElement.TryGetProperty("relationships", out JsonElement relationshipsArray) &&
                relationshipsArray.ValueKind == JsonValueKind.Array)
            {
                action.RelationshipGains = new List<RelationshipGain>();
                foreach (JsonElement relationshipElement in relationshipsArray.EnumerateArray())
                {
                    string characterName = GetStringProperty(relationshipElement, "characterName", "");
                    int amount = GetIntProperty(relationshipElement, "amount", 0);

                    if (!string.IsNullOrEmpty(characterName) && amount != 0)
                    {
                        action.RelationshipGains.Add(new RelationshipGain
                        {
                            CharacterName = characterName,
                            ChangeAmount = amount
                        });
                    }
                }
            }
        }

        return action;
    }

    // Helper methods for JSON parsing (same as in your LocationJsonParser)
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
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
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

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

        string encounterApproachString = GetStringProperty(root, "approach", "");
        if (!string.IsNullOrEmpty(encounterApproachString) &&
            Enum.TryParse(encounterApproachString, true, out EncounterApproaches encounterType))
        {
            action.EncounterApproach = encounterType;
        }

        // Parse action type
        string actionTypeStr = GetStringProperty(root, "actionType", "");
        if (!string.IsNullOrEmpty(actionTypeStr))
        {
            if (Enum.TryParse(actionTypeStr, true, out ActionTypes actionType))
            {
                action.ActionType = actionType;
            }
        }

        // Parse costs
        if (root.TryGetProperty("costs", out JsonElement costsElement))
        {
            action.EnergyCost = GetIntProperty(costsElement, "energy", 0);
            action.ConcentrationCost = GetIntProperty(costsElement, "focus", 0);
            action.ConfidenceCost = GetIntProperty(costsElement, "spirit", 0);
            action.HealthCost = GetIntProperty(costsElement, "health", 0);
            action.CoinCost = GetIntProperty(costsElement, "coin", 0);
        }

        // Parse yields
        if (root.TryGetProperty("yields", out JsonElement yieldsElement))
        {
            // Resource yields
            action.RestoresEnergy = GetIntProperty(yieldsElement, "energy", 0);
            action.RestoresConcentration = GetIntProperty(yieldsElement, "focus", 0);
            action.RestoresConfidence = GetIntProperty(yieldsElement, "spirit", 0);
            action.RestoresHealth = GetIntProperty(yieldsElement, "health", 0);
            action.CoinGain = GetIntProperty(yieldsElement, "coin", 0);

            // Relationship gains
            if (yieldsElement.TryGetProperty("relationships", out JsonElement relationshipsArray) &&
                relationshipsArray.ValueKind == JsonValueKind.Array)
            {
                action.RelationshipChanges = new List<RelationshipGain>();
                foreach (JsonElement relationshipElement in relationshipsArray.EnumerateArray())
                {
                    string characterName = GetStringProperty(relationshipElement, "characterName", "");
                    int amount = GetIntProperty(relationshipElement, "amount", 0);

                    if (!string.IsNullOrEmpty(characterName) && amount != 0)
                    {
                        action.RelationshipChanges.Add(new RelationshipGain
                        {
                            CharacterName = characterName,
                            ChangeAmount = amount
                        });
                    }
                }
            }

            // Location spot XP
            action.SpotXp = GetIntProperty(yieldsElement, "spotXp", 0);
        }

        // Parse requirements
        if (root.TryGetProperty("requirements", out JsonElement requirementsElement))
        {
            // Time window requirements
            if (requirementsElement.TryGetProperty("timeWindows", out JsonElement timeWindowsReq) &&
                timeWindowsReq.ValueKind == JsonValueKind.Array)
            {
                List<TimeWindow> allowedWindows = new List<TimeWindow>();
                foreach (JsonElement windowElement in timeWindowsReq.EnumerateArray())
                {
                    if (windowElement.ValueKind == JsonValueKind.String &&
                        Enum.TryParse(windowElement.GetString(), true, out TimeWindow window))
                    {
                        allowedWindows.Add(window);
                    }
                }

                if (allowedWindows.Count > 0)
                {
                    action.TimeWindows = allowedWindows;
                }
            }

            // Relationship requirements
            if (requirementsElement.TryGetProperty("relationships", out JsonElement relationshipsReq) &&
                relationshipsReq.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement relationshipElement in relationshipsReq.EnumerateArray())
                {
                    string characterName = GetStringProperty(relationshipElement, "characterName", "");
                    int minimumValue = GetIntProperty(relationshipElement, "minimumValue", 0);

                    if (!string.IsNullOrEmpty(characterName) && minimumValue > 0)
                    {
                        action.RelationshipRequirements.Add(new RelationshipRequirement(characterName, minimumValue));
                    }
                }
            }

            // Skill requirements
            if (requirementsElement.TryGetProperty("skills", out JsonElement skillsArray) &&
                skillsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement skillElement in skillsArray.EnumerateArray())
                {
                    string skillTypeStr = GetStringProperty(skillElement, "skillType", "");
                    int minimumLevel = GetIntProperty(skillElement, "minimumLevel", 0);

                    if (!string.IsNullOrEmpty(skillTypeStr) &&
                        Enum.TryParse(skillTypeStr, true, out SkillTypes skillType) &&
                        minimumLevel > 0)
                    {
                        action.SkillRequirements.Add(new SkillRequirement(skillType, minimumLevel));
                    }
                }
            }
        }

        // Parse encounter details
        if (root.TryGetProperty("encounterDetails", out JsonElement encounterElement))
        {
            action.Goal = GetStringProperty(encounterElement, "goal", "");
            action.Complication = GetStringProperty(encounterElement, "complication", "");
            action.IsOneTimeEncounter = GetBoolProperty(encounterElement, "isOneTimeEncounter", false);
            action.Difficulty = GetIntProperty(encounterElement, "difficulty", 1);
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

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.True)
                return true;
            else if (property.ValueKind == JsonValueKind.False)
                return false;
            else if (property.ValueKind == JsonValueKind.String &&
                     bool.TryParse(property.GetString(), out bool value))
            {
                return value;
            }
        }
        return defaultValue;
    }
}
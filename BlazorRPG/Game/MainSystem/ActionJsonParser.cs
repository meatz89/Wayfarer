using System.Text.Json;

public static class ActionJsonParser
{
    public static ActionCreationResult Parse(string json)
    {
        json = json.Replace("```json", "");
        json = json.Replace("```", "");

        // Initialize with default values in case parsing fails
        ActionCreationResult result = new ActionCreationResult();

        try
        {
            // Parse the JSON document
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Extract action data if available
            if (root.TryGetProperty("action", out JsonElement actionElement))
            {
                result.Action = ParseActionTemplate(actionElement);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing action and encounter JSON: {ex.Message}");
            Console.WriteLine($"Raw JSON: {json}");
            // Return the default result we already initialized
            return result;
        }
    }

    private static ActionDefinition ParseActionTemplate(JsonElement element)
    {
        string Name = GetStringProperty(element, "name", "name");

        // Parse basic action type
        string actionTypeStr = GetStringProperty(element, "actionType", "Encounter");

        EncounterTypes BasicActionType = EncounterTypes.Physical;

        if (element.TryGetProperty("basicActionType", out JsonElement basicTypeElement) &&
            basicTypeElement.ValueKind == JsonValueKind.String)
        {
            string basicTypeStr = basicTypeElement.GetString() ?? "";
            BasicActionType = ParseBasicActionType(basicTypeStr);
        }
        else
        {
            // Try to infer from actionType if basicActionType isn't specified
            BasicActionType = ParseBasicActionType(actionTypeStr);
        }

        ActionDefinition model = new ActionDefinition(Name, Name, 1, 50, BasicActionType, true);

        // Extract basic text fields
        model.Description = GetStringProperty(element, "description", model.Description);
        model.Goal = GetStringProperty(element, "goal", model.Goal);
        model.Complication = GetStringProperty(element, "complication", model.Complication);
        model.LocationName = GetStringProperty(element, "locationName", model.LocationName);
        model.LocationSpotName = GetStringProperty(element, "spotName", model.LocationSpotName);

        return model;
    }

    private static EncounterTypes ParseBasicActionType(string actionType)
    {
        // Direct enum parsing
        if (Enum.TryParse<EncounterTypes>(actionType, true, out EncounterTypes type))
        {
            return type;
        }

        // Handle common action type strings
        return actionType.ToLower() switch
        {
            "physical" => EncounterTypes.Physical,
            "intellectual" => EncounterTypes.Intellectual,
            "social" => EncounterTypes.Social,
            _ => EncounterTypes.Physical // Default fallback
        };
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString();
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }
        return defaultValue;
    }
}

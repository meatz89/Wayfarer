using System.Text.Json;

namespace Wayfarer.Content;

public static class NPCParser
{
    public static NPC ParseNPC(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        NPC npc = new NPC
        {
            ID = GetStringProperty(root, "id", ""),
            Name = GetStringProperty(root, "name", ""),
            Role = GetStringProperty(root, "role", ""),
            Description = GetStringProperty(root, "description", ""),
            Location = GetStringProperty(root, "location", ""),
        };

        // Parse profession
        string professionStr = GetStringProperty(root, "profession", "");
        if (Enum.TryParse(professionStr, out Professions profession))
        {
            npc.Profession = profession;
        }

        // Parse availability schedule
        string scheduleStr = GetStringProperty(root, "availabilitySchedule", "");
        if (Enum.TryParse(scheduleStr, out Schedule schedule))
        {
            npc.AvailabilitySchedule = schedule;
        }

        // Parse provided services
        List<string> serviceStrings = GetStringArray(root, "providedServices");
        foreach (string serviceStr in serviceStrings)
        {
            if (Enum.TryParse(serviceStr, out ServiceTypes service))
            {
                npc.ProvidedServices.Add(service);
            }
        }

        // Parse player relationship
        string relationshipStr = GetStringProperty(root, "playerRelationship", "");
        if (Enum.TryParse(relationshipStr, out NPCRelationship relationship))
        {
            npc.PlayerRelationship = relationship;
        }

        return npc;
    }

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
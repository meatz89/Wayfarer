using System.Text.Json;

public static class CardParser
{
    public static CardDefinition ParseCard(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "id");
        string name = GetStringProperty(root, "name", id);

        CardDefinition card = new CardDefinition(id, name);

        // Parse card type
        string typeString = GetStringProperty(root, "type", "PHYSICAL");
        if (Enum.TryParse(typeString, true, out CardTypes type))
        {
            card.Type = type;
        }

        // Parse skill
        string skillString = GetStringProperty(root, "skill", "STRENGTH");
        if (Enum.TryParse(skillString, true, out SkillTypes skill))
        {
            card.Skill = skill;
        }

        // Parse numeric properties
        card.Level = GetIntProperty(root, "level", 1);
        card.EnergyCost = GetIntProperty(root, "cost", 1);
        card.SkillBonus = GetIntProperty(root, "gain", 1);

        // Parse tags
        card.Tags = GetStringArray(root, "tags");

        return card;
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
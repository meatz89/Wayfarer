using System.Text.Json;
public static class LocationParser
{
    public static Location ParseLocation(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        string name = GetStringProperty(root, "name", "");

        Location location = new Location(id, name)
        {
            Description = GetStringProperty(root, "description", ""),
            ConnectedLocationIds = GetStringArrayFromProperty(root, "connectedTo"),
            LocationSpotIds = GetStringArrayFromProperty(root, "locationSpots"),
            DomainTags = GetStringArrayFromProperty(root, "domainTags")
        };

        if (root.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            location.MorningProperties = GetStringArrayFromProperty(envProps, "morning");
            location.AfternoonProperties = GetStringArrayFromProperty(envProps, "afternoon");
            location.EveningProperties = GetStringArrayFromProperty(envProps, "evening");
            location.NightProperties = GetStringArrayFromProperty(envProps, "night");
        }

        // Parse available professions by time
        if (root.TryGetProperty("availableProfessionsByTime", out JsonElement professionsByTime) &&
            professionsByTime.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeProperty in professionsByTime.EnumerateObject())
            {
                if (EnumParser.TryParse<TimeBlocks>(timeProperty.Name, out TimeBlocks timeBlock))
                {
                    List<Professions> professions = new List<Professions>();
                    if (timeProperty.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement professionElement in timeProperty.Value.EnumerateArray())
                        {
                            if (professionElement.ValueKind == JsonValueKind.String)
                            {
                                string professionStr = professionElement.GetString() ?? "";
                                if (EnumParser.TryParse<Professions>(professionStr, out Professions profession))
                                {
                                    professions.Add(profession);
                                }
                            }
                        }
                    }
                    location.AvailableProfessionsByTime[timeBlock] = professions;
                }
            }
        }

        // Parse access requirements
        if (root.TryGetProperty("accessRequirement", out JsonElement accessReqElement) &&
            accessReqElement.ValueKind == JsonValueKind.Object)
        {
            location.AccessRequirement = AccessRequirementParser.ParseAccessRequirement(accessReqElement);
        }

        return location;
    }

    private static List<string> GetStringArrayFromProperty(JsonElement element, string propertyName)
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
}
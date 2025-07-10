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
            ConnectedLocationIds = GetStringArray(root, "connectedTo"),
            LocationSpotIds = GetStringArray(root, "locationSpots"),
            DomainTags = GetStringArray(root, "domainTags")
        };

        if (root.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            location.MorningProperties = GetStringArrayFromProperty(envProps, "morning");
            location.AfternoonProperties = GetStringArrayFromProperty(envProps, "afternoon");
            location.EveningProperties = GetStringArrayFromProperty(envProps, "evening");
            location.NightProperties = GetStringArrayFromProperty(envProps, "night");
        }

        // Parse social categorical properties
        string socialExpectationStr = GetStringProperty(root, "socialExpectation", "");
        if (Enum.TryParse<Social_Expectation>(socialExpectationStr, out Social_Expectation socialExpectation))
        {
            location.SocialExpectation = socialExpectation;
        }

        string accessLevelStr = GetStringProperty(root, "accessLevel", "");
        if (Enum.TryParse<Access_Level>(accessLevelStr, out Access_Level accessLevel))
        {
            location.AccessLevel = accessLevel;
        }

        // Parse required social classes
        List<string> socialClassStrings = GetStringArray(root, "requiredSocialClasses");
        foreach (string socialClassStr in socialClassStrings)
        {
            if (Enum.TryParse<Social_Class>(socialClassStr, out Social_Class socialClass))
            {
                location.RequiredSocialClasses.Add(socialClass);
            }
        }

        // Parse available professions by time
        if (root.TryGetProperty("availableProfessionsByTime", out JsonElement professionsByTime) &&
            professionsByTime.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeProperty in professionsByTime.EnumerateObject())
            {
                if (Enum.TryParse<TimeBlocks>(timeProperty.Name, out TimeBlocks timeBlock))
                {
                    List<Professions> professions = new List<Professions>();
                    if (timeProperty.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement professionElement in timeProperty.Value.EnumerateArray())
                        {
                            if (professionElement.ValueKind == JsonValueKind.String)
                            {
                                string professionStr = professionElement.GetString() ?? "";
                                if (Enum.TryParse<Professions>(professionStr, out Professions profession))
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

        return location;
    }

    public static LocationSpot ParseLocationSpot(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        string name = GetStringProperty(root, "name", "");
        string locationId = GetStringProperty(root, "locationId", "");

        LocationSpot spot = new LocationSpot(id, name)
        {
            Description = GetStringProperty(root, "description", ""),
            InitialState = GetStringProperty(root, "initialState", ""),
            LocationId = locationId,
            Type = Enum.Parse<LocationSpotTypes>(GetStringProperty(root, "type", "FEATURE"), true),
            DomainTags = GetStringArray(root, "domainTags"),
            PreferredApproach = GetStringProperty(root, "preferredApproach", null),
            DislikedApproach = GetStringProperty(root, "dislikedApproach", null),
            DomainExpertise = GetStringProperty(root, "domainExpertise", null)
        };

        // Parse time windows
        List<string> timeWindowStrings = GetStringArray(root, "timeWindows");

        foreach (string windowString in timeWindowStrings)
        {
            if (Enum.TryParse(windowString, true, out TimeBlocks window))
            {
                spot.TimeWindows.Add(window);
            }
        }

        if (timeWindowStrings.Count == 0)
        {
            // Add all time windows as default
            spot.TimeWindows.Add(TimeBlocks.Morning);
            spot.TimeWindows.Add(TimeBlocks.Afternoon);
            spot.TimeWindows.Add(TimeBlocks.Evening);
            spot.TimeWindows.Add(TimeBlocks.Night);
        }

        return spot;
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
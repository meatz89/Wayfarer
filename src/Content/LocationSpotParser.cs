using System;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Parser for deserializing location spot data from JSON.
/// </summary>
public static class LocationSpotParser
{
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
            DomainTags = GetStringArrayFromProperty(root, "domainTags"),
            PreferredApproach = GetStringProperty(root, "preferredApproach", null),
            DislikedApproach = GetStringProperty(root, "dislikedApproach", null),
            DomainExpertise = GetStringProperty(root, "domainExpertise", null)
        };

        // Parse time windows
        List<string> CurrentTimeBlockStrings = GetStringArrayFromProperty(root, "CurrentTimeBlocks");

        foreach (string windowString in CurrentTimeBlockStrings)
        {
            if (Enum.TryParse(windowString, true, out TimeBlocks window))
            {
                spot.CurrentTimeBlocks.Add(window);
            }
        }

        if (CurrentTimeBlockStrings.Count == 0)
        {
            // Add all time windows as default
            spot.CurrentTimeBlocks.Add(TimeBlocks.Morning);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Evening);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Night);
        }

        // Parse access requirements
        if (root.TryGetProperty("accessRequirement", out JsonElement accessReqElement) &&
            accessReqElement.ValueKind == JsonValueKind.Object)
        {
            spot.AccessRequirement = AccessRequirementParser.ParseAccessRequirement(accessReqElement);
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
}
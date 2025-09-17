using System;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Parser for deserializing location spot data from JSON.
/// </summary>
public static class LocationSpotParser
{
    /// <summary>
    /// Convert a LocationSpotDTO to a LocationSpot domain model
    /// </summary>
    public static LocationSpot ConvertDTOToLocationSpot(LocationSpotDTO dto)
    {
        LocationSpot spot = new LocationSpot(dto.Id, dto.Name)
        {
            InitialState = dto.InitialState ?? "",
            LocationId = dto.LocationId ?? ""
        };

        // Parse time windows
        if (dto.CurrentTimeBlocks != null && dto.CurrentTimeBlocks.Count > 0)
        {
            foreach (string windowString in dto.CurrentTimeBlocks)
            {
                if (EnumParser.TryParse<TimeBlocks>(windowString, out TimeBlocks window))
                {
                    spot.CurrentTimeBlocks.Add(window);
                }
            }
        }
        else
        {
            // Add all time windows as default
            spot.CurrentTimeBlocks.Add(TimeBlocks.Morning);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Midday);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Evening);
        }

        // Parse spot properties from the new structure
        if (dto.Properties != null)
        {
            Console.WriteLine($"[LocationSpotParser] Parsing spot {spot.SpotID} properties");

            // Parse base properties (always active)
            if (dto.Properties.Base != null)
            {
                Console.WriteLine($"[LocationSpotParser] Found {dto.Properties.Base.Count} base properties");
                foreach (string propString in dto.Properties.Base)
                {
                    Console.WriteLine($"[LocationSpotParser] Trying to parse base property: {propString}");
                    if (EnumParser.TryParse<SpotPropertyType>(propString, out SpotPropertyType prop))
                    {
                        Console.WriteLine($"[LocationSpotParser] Successfully parsed: {prop}");
                        spot.SpotProperties.Add(prop);
                    }
                    else
                    {
                        Console.WriteLine($"[LocationSpotParser] Failed to parse property: {propString}");
                    }
                }
            }

            // Parse "all" properties (always active, alternative to "base")
            if (dto.Properties.All != null)
            {
                Console.WriteLine($"[LocationSpotParser] Found {dto.Properties.All.Count} 'all' properties");
                foreach (string propString in dto.Properties.All)
                {
                    Console.WriteLine($"[LocationSpotParser] Trying to parse 'all' property: {propString}");
                    if (EnumParser.TryParse<SpotPropertyType>(propString, out SpotPropertyType prop))
                    {
                        Console.WriteLine($"[LocationSpotParser] Successfully parsed: {prop}");
                        spot.SpotProperties.Add(prop);
                    }
                    else
                    {
                        Console.WriteLine($"[LocationSpotParser] Failed to parse property: {propString}");
                    }
                }
            }

            // Parse time-specific properties
            Dictionary<TimeBlocks, List<SpotPropertyType>> timeProperties = new Dictionary<TimeBlocks, List<SpotPropertyType>>();

            // Morning properties
            ParseTimeProperties(dto.Properties.Morning, TimeBlocks.Morning, timeProperties);
            // Midday properties
            ParseTimeProperties(dto.Properties.Midday, TimeBlocks.Midday, timeProperties);
            // Afternoon properties
            ParseTimeProperties(dto.Properties.Afternoon, TimeBlocks.Afternoon, timeProperties);
            // Evening properties
            ParseTimeProperties(dto.Properties.Evening, TimeBlocks.Evening, timeProperties);
            // Night properties
            ParseTimeProperties(dto.Properties.Night, TimeBlocks.Night, timeProperties);
            // Dawn properties
            ParseTimeProperties(dto.Properties.Dawn, TimeBlocks.Dawn, timeProperties);

            // Assign to spot
            foreach (KeyValuePair<TimeBlocks, List<SpotPropertyType>> kvp in timeProperties)
            {
                if (kvp.Value.Count > 0)
                {
                    spot.TimeSpecificProperties[kvp.Key] = kvp.Value;
                }
            }
        }

        // Parse access requirements
        if (dto.AccessRequirement != null)
        {
            spot.AccessRequirement = AccessRequirementParser.ConvertDTOToAccessRequirement(dto.AccessRequirement);
        }

        return spot;
    }

    /// <summary>
    /// Helper method to parse time-specific properties
    /// </summary>
    private static void ParseTimeProperties(List<string> propertyStrings, TimeBlocks timeBlock, Dictionary<TimeBlocks, List<SpotPropertyType>> timeProperties)
    {
        if (propertyStrings == null || propertyStrings.Count == 0)
            return;

        List<SpotPropertyType> properties = new List<SpotPropertyType>();
        foreach (string propString in propertyStrings)
        {
            if (!string.IsNullOrEmpty(propString) &&
                EnumParser.TryParse<SpotPropertyType>(propString, out SpotPropertyType prop))
            {
                properties.Add(prop);
            }
        }

        if (properties.Count > 0)
        {
            timeProperties[timeBlock] = properties;
        }
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
            // Description removed - generated from SpotPropertyType combinations
            InitialState = GetStringProperty(root, "initialState", ""),
            LocationId = locationId
        };

        // Parse time windows
        List<string> CurrentTimeBlockStrings = GetStringArrayFromProperty(root, "CurrentTimeBlocks");

        foreach (string windowString in CurrentTimeBlockStrings)
        {
            if (EnumParser.TryParse<TimeBlocks>(windowString, out TimeBlocks window))
            {
                spot.CurrentTimeBlocks.Add(window);
            }
        }

        if (CurrentTimeBlockStrings.Count == 0)
        {
            // Add all time windows as default
            spot.CurrentTimeBlocks.Add(TimeBlocks.Morning);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Midday);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
            spot.CurrentTimeBlocks.Add(TimeBlocks.Evening);
        }

        ParseSpotProperties(root, spot);

        // Parse access requirements
        if (root.TryGetProperty("accessRequirement", out JsonElement accessReqElement) &&
            accessReqElement.ValueKind == JsonValueKind.Object)
        {
            spot.AccessRequirement = AccessRequirementParser.ParseAccessRequirement(accessReqElement);
        }

        return spot;
    }

    private static void ParseSpotProperties(JsonElement root, LocationSpot spot)
    {
        // Parse spot properties
        List<string> spotPropertyStrings = GetStringArrayFromProperty(root, "spotProperties");
        Console.WriteLine($"[LocationSpotParser] Parsing spot {spot.SpotID}, found {spotPropertyStrings.Count} property strings");
        foreach (string propString in spotPropertyStrings)
        {
            Console.WriteLine($"[LocationSpotParser] Trying to parse property: {propString}");
            if (EnumParser.TryParse<SpotPropertyType>(propString, out SpotPropertyType prop))
            {
                Console.WriteLine($"[LocationSpotParser] Successfully parsed: {prop}");
                spot.SpotProperties.Add(prop);
            }
            else
            {
                Console.WriteLine($"[LocationSpotParser] Failed to parse property: {propString}");
            }
        }

        // Parse time-specific spot properties
        if (root.TryGetProperty("timeSpecificProperties", out JsonElement timePropsElement) &&
            timePropsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeProp in timePropsElement.EnumerateObject())
            {
                if (EnumParser.TryParse<TimeBlocks>(timeProp.Name, out TimeBlocks timeBlock))
                {
                    List<SpotPropertyType> properties = new List<SpotPropertyType>();
                    if (timeProp.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement propElement in timeProp.Value.EnumerateArray())
                        {
                            if (propElement.ValueKind == JsonValueKind.String)
                            {
                                string propString = propElement.GetString();
                                if (!string.IsNullOrEmpty(propString) &&
                                    EnumParser.TryParse<SpotPropertyType>(propString, out SpotPropertyType prop))
                                {
                                    properties.Add(prop);
                                }
                            }
                        }
                    }
                    if (properties.Count > 0)
                    {
                        spot.TimeSpecificProperties[timeBlock] = properties;
                    }
                }
            }
        }
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


public static class LocationParser
{
    /// <summary>
    /// Convert a LocationDTO to a Location domain model
    /// </summary>
    public static Location ConvertDTOToLocation(LocationDTO dto)
    {
        Location location = new Location(dto.Id, dto.Name)
        {
            Description = dto.Description ?? "",
            Tier = dto.Tier,
            TravelHubSpotId = dto.TravelHubSpotId,
            DomainTags = dto.DomainTags ?? new List<string>(),
            LocationType = Enum.TryParse<LocationTypes>(dto.LocationType ?? "Connective", out var locationType) ? locationType : LocationTypes.Connective,
            LocationTypeString = dto.LocationType,
            IsStartingLocation = dto.IsStartingLocation
        };

        // Parse environmental properties
        if (dto.EnvironmentalProperties != null)
        {
            location.MorningProperties = dto.EnvironmentalProperties.Morning ?? new List<string>();
            location.AfternoonProperties = dto.EnvironmentalProperties.Afternoon ?? new List<string>();
            location.EveningProperties = dto.EnvironmentalProperties.Evening ?? new List<string>();
            location.NightProperties = dto.EnvironmentalProperties.Night ?? new List<string>();
        }

        // Parse available professions by time
        if (dto.AvailableProfessionsByTime != null)
        {
            foreach (var kvp in dto.AvailableProfessionsByTime)
            {
                if (EnumParser.TryParse<TimeBlocks>(kvp.Key, out TimeBlocks timeBlock))
                {
                    List<Professions> professions = new List<Professions>();
                    foreach (string professionStr in kvp.Value)
                    {
                        if (EnumParser.TryParse<Professions>(professionStr, out Professions profession))
                        {
                            professions.Add(profession);
                        }
                    }
                    location.AvailableProfessionsByTime[timeBlock] = professions;
                }
            }
        }

        // Parse access requirement
        if (dto.AccessRequirement != null)
        {
            location.AccessRequirement = AccessRequirementParser.ConvertDTOToAccessRequirement(dto.AccessRequirement);
        }

        return location;
    }
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

        // Parse new mechanical properties that replace hardcoded location checks
        location.LocationTypeString = GetStringProperty(root, "locationType", "");

        if (root.TryGetProperty("isStartingLocation", out JsonElement isStartingElement) &&
            isStartingElement.ValueKind == JsonValueKind.True)
        {
            location.IsStartingLocation = true;
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

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
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Location DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Location {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.LocationType))
            throw new InvalidOperationException($"Location {dto.Id} missing required 'LocationType' field");

        if (!Enum.TryParse<LocationTypes>(dto.LocationType, out LocationTypes locationType))
            throw new InvalidOperationException($"Location {dto.Id} has invalid LocationType: '{dto.LocationType}'");

        Location location = new Location(dto.Id, dto.Name)
        {
            Description = dto.Description ?? string.Empty, // Description is optional
            Tier = dto.Tier,
            DomainTags = dto.DomainTags ?? new List<string>(), // Empty list is valid for no tags
            LocationType = locationType,
            LocationTypeString = dto.LocationType,
            IsStartingLocation = dto.IsStartingLocation
        };

        // Parse environmental properties
        if (dto.EnvironmentalProperties != null)
        {
            location.MorningProperties = dto.EnvironmentalProperties.Morning ?? new List<string>(); // Empty list valid
            location.AfternoonProperties = dto.EnvironmentalProperties.Afternoon ?? new List<string>(); // Empty list valid
            location.EveningProperties = dto.EnvironmentalProperties.Evening ?? new List<string>(); // Empty list valid
            location.NightProperties = dto.EnvironmentalProperties.Night ?? new List<string>(); // Empty list valid
        }

        // Parse available professions by time
        if (dto.AvailableProfessionsByTime != null)
        {
            foreach (KeyValuePair<string, List<string>> kvp in dto.AvailableProfessionsByTime)
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

        string id = GetRequiredStringProperty(root, "id");
        string name = GetRequiredStringProperty(root, "name");

        Location location = new Location(id, name)
        {
            Description = GetOptionalStringProperty(root, "description") ?? string.Empty,
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
                                string professionStr = professionElement.GetString();
                                if (string.IsNullOrEmpty(professionStr))
                                    throw new InvalidOperationException($"Location {location.Id} has empty profession string in availableProfessionsByTime");
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
        location.LocationTypeString = GetRequiredStringProperty(root, "locationType");

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
                    string value = item.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        throw new InvalidOperationException($"Array property '{propertyName}' contains empty string in Location JSON");
                    results.Add(value);
                }
            }
        }

        return results;
    }

    private static string GetRequiredStringProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
            throw new InvalidOperationException($"Missing required property '{propertyName}' in Location JSON");
        
        if (property.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Property '{propertyName}' must be a string in Location JSON");
        
        string value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Property '{propertyName}' cannot be empty in Location JSON");
        
        return value;
    }

    private static string GetOptionalStringProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
            return null;
        
        if (property.ValueKind != JsonValueKind.String)
            return null;
        
        return property.GetString();
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Parser for deserializing Venue location data from JSON.
/// </summary>
public static class LocationParser
{
    /// <summary>
    /// Convert a LocationDTO to a Location domain model
    /// </summary>
    public static Location ConvertDTOToLocation(LocationDTO dto)
    {
        Location location = new Location(dto.Id, dto.Name)
        {
            InitialState = dto.InitialState ?? "",
            VenueId = dto.VenueId ?? ""
        };

        // Parse time windows
        if (dto.CurrentTimeBlocks != null && dto.CurrentTimeBlocks.Count > 0)
        {
            foreach (string windowString in dto.CurrentTimeBlocks)
            {
                if (EnumParser.TryParse<TimeBlocks>(windowString, out TimeBlocks window))
                {
                    location.CurrentTimeBlocks.Add(window);
                }
            }
        }
        else
        {
            // Add all time windows as default
            location.CurrentTimeBlocks.Add(TimeBlocks.Morning);
            location.CurrentTimeBlocks.Add(TimeBlocks.Midday);
            location.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
            location.CurrentTimeBlocks.Add(TimeBlocks.Evening);
        }

        // Parse location properties from the new structure
        if (dto.Properties != null)
        {
            Console.WriteLine($"[LocationParser] Parsing location {location.Id} properties");

            // Parse base properties (always active)
            if (dto.Properties.Base != null)
            {
                Console.WriteLine($"[LocationParser] Found {dto.Properties.Base.Count} base properties");
                foreach (string propString in dto.Properties.Base)
                {
                    Console.WriteLine($"[LocationParser] Trying to parse base property: {propString}");
                    if (EnumParser.TryParse<LocationPropertyType>(propString, out LocationPropertyType prop))
                    {
                        Console.WriteLine($"[LocationParser] Successfully parsed: {prop}");
                        location.LocationProperties.Add(prop);
                    }
                    else
                    {
                        Console.WriteLine($"[LocationParser] Failed to parse property: {propString}");
                    }
                }
            }

            // Parse "all" properties (always active, alternative to "base")
            if (dto.Properties.All != null)
            {
                Console.WriteLine($"[LocationParser] Found {dto.Properties.All.Count} 'all' properties");
                foreach (string propString in dto.Properties.All)
                {
                    Console.WriteLine($"[LocationParser] Trying to parse 'all' property: {propString}");
                    if (EnumParser.TryParse<LocationPropertyType>(propString, out LocationPropertyType prop))
                    {
                        Console.WriteLine($"[LocationParser] Successfully parsed: {prop}");
                        location.LocationProperties.Add(prop);
                    }
                    else
                    {
                        Console.WriteLine($"[LocationParser] Failed to parse property: {propString}");
                    }
                }
            }

            // Parse time-specific properties
            Dictionary<TimeBlocks, List<LocationPropertyType>> timeProperties = new Dictionary<TimeBlocks, List<LocationPropertyType>>();

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

            // Assign to location
            foreach (KeyValuePair<TimeBlocks, List<LocationPropertyType>> kvp in timeProperties)
            {
                if (kvp.Value.Count > 0)
                {
                    location.TimeSpecificProperties[kvp.Key] = kvp.Value;
                }
            }
        }

        // Parse access requirements
        if (dto.AccessRequirement != null)
        {
            location.AccessRequirement = AccessRequirementParser.ConvertDTOToAccessRequirement(dto.AccessRequirement);
        }

        // Parse gameplay properties moved from Location
        location.DomainTags = dto.DomainTags ?? new List<string>();

        if (!string.IsNullOrEmpty(dto.LocationType) && Enum.TryParse(dto.LocationType, out LocationTypes locationType))
        {
            location.LocationType = locationType;
        }

        location.IsStartingLocation = dto.IsStartingLocation;

        if (!string.IsNullOrEmpty(dto.InvestigationProfile))
        {
            if (System.Enum.TryParse<InvestigationDiscipline>(dto.InvestigationProfile, out InvestigationDiscipline investigationProfile))
            {
                location.InvestigationProfile = investigationProfile;
            }
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

        // Parse available work actions
        if (dto.AvailableWork != null)
        {
            foreach (WorkActionDTO workDto in dto.AvailableWork)
            {
                WorkAction workAction = new WorkAction
                {
                    Id = workDto.Id,
                    Name = workDto.Name,
                    Description = workDto.Description,
                    Type = Enum.TryParse<WorkType>(workDto.Type, out WorkType workType) ? workType : WorkType.Standard,
                    BaseCoins = workDto.BaseCoins,
                    VenueId = workDto.VenueId,
                    LocationId = workDto.LocationId,
                    RequiredTokens = workDto.RequiredTokens,
                    RequiredTokenType = workDto.RequiredTokenType != null && Enum.TryParse<ConnectionType>(workDto.RequiredTokenType, out ConnectionType tokenType) ? tokenType : null,
                    RequiredPermit = workDto.RequiredPermit,
                    HungerReduction = workDto.HungerReduction,
                    HealthRestore = workDto.HealthRestore,
                    GrantedItem = workDto.GrantedItem
                };
                location.AvailableWork.Add(workAction);
            }
        }

        return location;
    }

    /// <summary>
    /// Helper method to parse time-specific properties
    /// </summary>
    private static void ParseTimeProperties(List<string> propertyStrings, TimeBlocks timeBlock, Dictionary<TimeBlocks, List<LocationPropertyType>> timeProperties)
    {
        if (propertyStrings == null || propertyStrings.Count == 0)
            return;

        List<LocationPropertyType> properties = new List<LocationPropertyType>();
        foreach (string propString in propertyStrings)
        {
            if (!string.IsNullOrEmpty(propString) &&
                EnumParser.TryParse<LocationPropertyType>(propString, out LocationPropertyType prop))
            {
                properties.Add(prop);
            }
        }

        if (properties.Count > 0)
        {
            timeProperties[timeBlock] = properties;
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
                    string value = item.GetString();
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

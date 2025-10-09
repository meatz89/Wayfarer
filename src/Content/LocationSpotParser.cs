using System;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Parser for deserializing Venue spot data from JSON.
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
            VenueId = dto.VenueId ?? ""
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
            Console.WriteLine($"[LocationSpotParser] Parsing spot {spot.Id} properties");

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

        // Parse gameplay properties moved from Location
        spot.DomainTags = dto.DomainTags ?? new List<string>();

        if (!string.IsNullOrEmpty(dto.LocationType) && Enum.TryParse(dto.LocationType, out LocationSpotTypes locationType))
        {
            spot.LocationType = locationType;
        }

        spot.IsStartingLocation = dto.IsStartingLocation;

        if (!string.IsNullOrEmpty(dto.InvestigationProfile))
        {
            if (System.Enum.TryParse<InvestigationDiscipline>(dto.InvestigationProfile, out InvestigationDiscipline investigationProfile))
            {
                spot.InvestigationProfile = investigationProfile;
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
                    spot.AvailableProfessionsByTime[timeBlock] = professions;
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
                    SpotId = workDto.SpotId,
                    RequiredTokens = workDto.RequiredTokens,
                    RequiredTokenType = workDto.RequiredTokenType != null && Enum.TryParse<ConnectionType>(workDto.RequiredTokenType, out ConnectionType tokenType) ? tokenType : null,
                    RequiredPermit = workDto.RequiredPermit,
                    HungerReduction = workDto.HungerReduction,
                    HealthRestore = workDto.HealthRestore,
                    GrantedItem = workDto.GrantedItem
                };
                spot.AvailableWork.Add(workAction);
            }
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
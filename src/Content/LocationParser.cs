using System.Text.Json;
/// <summary>
/// Parser for deserializing Venue location data from JSON.
/// </summary>
public static class LocationParser
{
    /// <summary>
    /// Convert a LocationDTO to a Location domain model
    /// </summary>
    public static Location ConvertDTOToLocation(LocationDTO dto, GameWorld gameWorld)
    {
        // ADR-007: Constructor uses Name only (no Id parameter)
        Location location = new Location(dto.Name)
        {
            InitialState = dto.InitialState ?? "" // Optional - defaults to empty if missing
        };

        // PURE PROCEDURAL PLACEMENT: Store categorical distance hint for placement phase
        // Flows from JSON → DTO → Parser → LocationPlacementService.PlaceLocation()
        // Default to "medium" if missing (content author forgot to specify)
        location.DistanceHintForPlacement = dto.DistanceFromPlayer ?? "medium";
        Console.WriteLine($"[LocationParser] Location '{dto.Name}' distance hint: '{location.DistanceHintForPlacement}'");

        // HIGHLANDER: NO hex position assignment here - happens in LocationPlacementService
        // HIGHLANDER: NO venue assignment here - happens in LocationPlacementService.PlaceLocation() via categorical matching
        // Parser creates Location entity with NO hex coordinates
        // Spatial properties set in post-parse initialization phase (PackageLoader.PlaceAllLocations)

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
        {// Parse base properties (always active)
            Console.WriteLine($"[LocationParser] Parsing properties for location '{dto.Id}'");

            if (dto.Properties.Base != null)
            {
                Console.WriteLine($"[LocationParser] Base properties: {string.Join(", ", dto.Properties.Base)}");
                foreach (string propString in dto.Properties.Base)
                {
                    if (EnumParser.TryParse<LocationPropertyType>(propString, out LocationPropertyType prop))
                    {
                        location.LocationProperties.Add(prop);
                        Console.WriteLine($"[LocationParser] ✅ Parsed base property: {propString} → {prop}");
                    }
                    else
                    {
                        Console.WriteLine($"[LocationParser] ⚠️ WARNING: Failed to parse base property '{propString}' for location '{dto.Id}'");
                    }
                }
            }

            Console.WriteLine($"[LocationParser] Final LocationProperties for '{dto.Id}': {string.Join(", ", location.LocationProperties)}");

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
            // Night and Dawn removed from 4-block system

            // Assign to location
            foreach (KeyValuePair<TimeBlocks, List<LocationPropertyType>> kvp in timeProperties)
            {
                if (kvp.Value.Count > 0)
                {
                    location.TimeSpecificProperties.Add(new TimeSpecificProperty
                    {
                        TimeBlock = kvp.Key,
                        Properties = kvp.Value
                    });
                }
            }
        }

        // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access

        // Parse gameplay properties moved from Location
        location.DomainTags = dto.DomainTags ?? new List<string>(); // Optional - defaults to empty list if missing

        if (!string.IsNullOrEmpty(dto.LocationType) && Enum.TryParse(dto.LocationType, out LocationTypes locationType))
        {
            location.LocationType = locationType;
        }

        location.IsStartingLocation = dto.IsStartingLocation;

        if (!string.IsNullOrEmpty(dto.ObligationProfile))
        {
            if (System.Enum.TryParse<ObligationDiscipline>(dto.ObligationProfile, out ObligationDiscipline obligationProfile))
            {
                location.ObligationProfile = obligationProfile;
            }
        }

        // Parse orthogonal categorical dimensions for entity resolution
        if (!string.IsNullOrEmpty(dto.Privacy) && Enum.TryParse(dto.Privacy, out LocationPrivacy privacy))
        {
            location.Privacy = privacy;
        }

        if (!string.IsNullOrEmpty(dto.Safety) && Enum.TryParse(dto.Safety, out LocationSafety safety))
        {
            location.Safety = safety;
        }

        if (!string.IsNullOrEmpty(dto.Activity) && Enum.TryParse(dto.Activity, out LocationActivity activity))
        {
            location.Activity = activity;
        }

        if (!string.IsNullOrEmpty(dto.Purpose) && Enum.TryParse(dto.Purpose, out LocationPurpose purpose))
        {
            location.Purpose = purpose;
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
                // HIGHLANDER: Resolve string IDs to object references
                Venue workVenue = gameWorld.Venues.FirstOrDefault(v => v.Name == workDto.VenueId);
                Location workLocation = gameWorld.Locations.FirstOrDefault(l => l.Name == workDto.LocationId);
                Item requiredPermit = !string.IsNullOrEmpty(workDto.RequiredPermit)
                    ? gameWorld.Items.FirstOrDefault(i => i.Name == workDto.RequiredPermit)
                    : null;
                Item grantedItem = !string.IsNullOrEmpty(workDto.GrantedItem)
                    ? gameWorld.Items.FirstOrDefault(i => i.Name == workDto.GrantedItem)
                    : null;

                WorkAction workAction = new WorkAction
                {
                    // HIGHLANDER: NO Id property - WorkAction identified by object reference
                    Name = workDto.Name,
                    Description = workDto.Description,
                    Type = Enum.TryParse<WorkType>(workDto.Type, out WorkType workType) ? workType : WorkType.Standard,
                    BaseCoins = workDto.BaseCoins,
                    Venue = workVenue,
                    Location = workLocation,
                    RequiredTokens = workDto.RequiredTokens,
                    RequiredTokenType = workDto.RequiredTokenType != null && Enum.TryParse<ConnectionType>(workDto.RequiredTokenType, out ConnectionType tokenType) ? tokenType : null,
                    RequiredPermit = requiredPermit,
                    HungerReduction = workDto.HungerReduction,
                    HealthRestore = workDto.HealthRestore,
                    GrantedItem = grantedItem
                };
                location.AvailableWork.Add(workAction);
            }
        }

        // NOTE: Old SceneDTO parsing removed - equipment-based Scene system deleted
        // ObservationScene (Mental) and TravelScene (Physical) are separate valid systems
        // NEW Scene-Situation architecture spawns Scenes via SceneTemplates (not embedded in location JSON)

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

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
        // Spatial properties set in post-parse initialization phase (PackageLoader.PlaceLocations)

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

        // Parse functional capabilities (what location CAN DO)
        if (dto.Capabilities != null && dto.Capabilities.Count > 0)
        {
            Console.WriteLine($"[LocationParser] Parsing capabilities for location '{dto.Id}'");
            Console.WriteLine($"[LocationParser] Capabilities: {string.Join(", ", dto.Capabilities)}");

            LocationCapability combinedCapabilities = LocationCapability.None;

            foreach (string capabilityString in dto.Capabilities)
            {
                if (EnumParser.TryParse<LocationCapability>(capabilityString, out LocationCapability capability))
                {
                    combinedCapabilities |= capability;  // Bitwise OR to combine flags
                    Console.WriteLine($"[LocationParser] ✅ Parsed capability: {capabilityString} → {capability}");
                }
                else
                {
                    Console.WriteLine($"[LocationParser] ⚠️ WARNING: Failed to parse capability '{capabilityString}' for location '{dto.Id}'");
                }
            }

            location.Capabilities = combinedCapabilities;
            Console.WriteLine($"[LocationParser] Final Capabilities for '{dto.Id}': {location.Capabilities}");
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

        // Parse ProximityConstraint (scaffolding metadata for dependent location placement)
        if (dto.ProximityConstraint != null)
        {
            if (!Enum.TryParse<PlacementProximity>(dto.ProximityConstraint.Proximity, ignoreCase: true, out PlacementProximity proximity))
            {
                throw new InvalidOperationException(
                    $"Invalid PlacementProximity value '{dto.ProximityConstraint.Proximity}' for location '{dto.Name}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames(typeof(PlacementProximity)))}"
                );
            }

            location.ProximityConstraintForPlacement = new ProximityConstraint
            {
                Proximity = proximity,
                ReferenceLocationKey = dto.ProximityConstraint.ReferenceLocation
            };

            Console.WriteLine($"[LocationParser] Location '{dto.Name}' proximity constraint: {proximity} relative to '{location.ProximityConstraintForPlacement.ReferenceLocationKey}'");
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

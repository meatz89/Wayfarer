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
        Location location = new Location(dto.Name);

        // PURE PROCEDURAL PLACEMENT: Store categorical distance hint for placement phase
        // Flows from JSON → DTO → Parser → LocationPlacementService.PlaceLocation()
        location.DistanceHintForPlacement = dto.DistanceFromPlayer ?? "near";
        Console.WriteLine($"[LocationParser] Location '{dto.Name}' distance hint: '{location.DistanceHintForPlacement}'");

        // HIGHLANDER: NO hex position assignment here - happens in LocationPlacementService
        // HIGHLANDER: NO venue assignment here - happens in LocationPlacementService.PlaceLocation() via categorical matching
        // Parser creates Location entity with NO hex coordinates
        // Spatial properties set in post-parse initialization phase (PackageLoader.PlaceLocations)

        // Parse functional capabilities (what location CAN DO)
        // EXPLICIT INITIALIZATION: Capabilities.None if no capabilities specified
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
        else
        {
            // EXPLICIT: No capabilities = Capabilities.None (must be set explicitly)
            location.Capabilities = LocationCapability.None;
        }

        // EXPLICIT INITIALIZATION: Skeleton tracking (false for authored locations)
        location.IsSkeleton = false;

        // EXPLICIT INITIALIZATION: Progression/mastery properties (0 for new locations)
        location.Familiarity = 0;
        location.MaxFamiliarity = 3; // Standard maximum
        location.HighestObservationCompleted = 0;
        location.InvestigationCubes = 0;

        // EXPLICIT INITIALIZATION: Flow modifier (0 = neutral, can be positive/negative)
        location.FlowModifier = 0;

        // EXPLICIT INITIALIZATION: Tier (MUST come from placement algorithm or be set to 1)
        // TODO: Tier should be determined by LocationPlacementService based on venue tier
        location.Tier = 1;

        // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access

        // Parse gameplay properties - FAIL-FAST: all required, no silent failures
        // DomainTags: Collection fallback acceptable (empty list = no tags)
        location.DomainTags = dto.DomainTags ?? new List<string>();

        // LocationType: REQUIRED, no defaults
        if (string.IsNullOrEmpty(dto.LocationType))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required LocationType property. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationTypes)))}");
        }
        if (!Enum.TryParse(dto.LocationType, out LocationTypes locationType))
        {
            throw new InvalidOperationException(
                $"Invalid LocationType value '{dto.LocationType}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationTypes)))}");
        }
        location.LocationType = locationType;

        // IsStartingLocation: explicit boolean required (dto must have value)
        location.IsStartingLocation = dto.IsStartingLocation;

        // ObligationProfile: REQUIRED, no defaults
        if (string.IsNullOrEmpty(dto.ObligationProfile))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required ObligationProfile property. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(ObligationDiscipline)))}");
        }
        if (!Enum.TryParse<ObligationDiscipline>(dto.ObligationProfile, out ObligationDiscipline obligationProfile))
        {
            throw new InvalidOperationException(
                $"Invalid ObligationProfile value '{dto.ObligationProfile}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(ObligationDiscipline)))}");
        }
        location.ObligationProfile = obligationProfile;

        // Parse orthogonal categorical dimensions for entity resolution
        // FAIL-FAST: All categorical dimensions REQUIRED, no defaults, no silent failures
        if (string.IsNullOrEmpty(dto.Privacy))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required Privacy property. " +
                $"Every location MUST have explicit Privacy. Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationPrivacy)))}");
        }
        if (!Enum.TryParse(dto.Privacy, out LocationPrivacy privacy))
        {
            throw new InvalidOperationException(
                $"Invalid Privacy value '{dto.Privacy}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationPrivacy)))}");
        }
        location.Privacy = privacy;

        if (string.IsNullOrEmpty(dto.Safety))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required Safety property. " +
                $"Every location MUST have explicit Safety. Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationSafety)))}");
        }
        if (!Enum.TryParse(dto.Safety, out LocationSafety safety))
        {
            throw new InvalidOperationException(
                $"Invalid Safety value '{dto.Safety}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationSafety)))}");
        }
        location.Safety = safety;

        if (string.IsNullOrEmpty(dto.Activity))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required Activity property. " +
                $"Every location MUST have explicit Activity. Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationActivity)))}");
        }
        if (!Enum.TryParse(dto.Activity, out LocationActivity activity))
        {
            throw new InvalidOperationException(
                $"Invalid Activity value '{dto.Activity}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationActivity)))}");
        }
        location.Activity = activity;

        if (string.IsNullOrEmpty(dto.Purpose))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required Purpose property. " +
                $"Every location MUST have explicit Purpose. Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationPurpose)))}");
        }
        if (!Enum.TryParse(dto.Purpose, out LocationPurpose purpose))
        {
            throw new InvalidOperationException(
                $"Invalid Purpose value '{dto.Purpose}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationPurpose)))}");
        }
        location.Purpose = purpose;

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

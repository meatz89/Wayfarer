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

        // Parse orthogonal environmental dimensions (replacing generic capabilities)
        // Environment: Indoor, Outdoor, Covered, Underground
        if (!string.IsNullOrEmpty(dto.Environment))
        {
            if (!Enum.TryParse(dto.Environment, out LocationEnvironment environment))
            {
                throw new InvalidOperationException(
                    $"Invalid Environment value '{dto.Environment}' for location '{dto.Name}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationEnvironment)))}");
            }
            location.Environment = environment;
        }
        else
        {
            // Default to Indoor if not specified
            location.Environment = LocationEnvironment.Indoor;
        }

        // Setting: Urban, Suburban, Rural, Wilderness
        if (!string.IsNullOrEmpty(dto.Setting))
        {
            if (!Enum.TryParse(dto.Setting, out LocationSetting setting))
            {
                throw new InvalidOperationException(
                    $"Invalid Setting value '{dto.Setting}' for location '{dto.Name}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationSetting)))}");
            }
            location.Setting = setting;
        }
        else
        {
            // Default to Urban if not specified
            location.Setting = LocationSetting.Urban;
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

        // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access

        // Parse gameplay properties - FAIL-FAST: all required, no silent failures
        // Role: REQUIRED, no defaults - describes functional/narrative role of location
        if (string.IsNullOrEmpty(dto.Role))
        {
            throw new InvalidOperationException(
                $"Location '{dto.Name}' missing required Role property. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationRole)))}");
        }
        if (!Enum.TryParse(dto.Role, out LocationRole locationRole))
        {
            throw new InvalidOperationException(
                $"Invalid Role value '{dto.Role}' for location '{dto.Name}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames(typeof(LocationRole)))}");
        }
        location.Role = locationRole;

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

        // Parse available professions by time - EXPLICIT PROPERTIES, not wrapper classes
        if (dto.AvailableProfessionsByTime != null)
        {
            foreach (ProfessionsByTimeEntry entry in dto.AvailableProfessionsByTime)
            {
                if (EnumParser.TryParse<TimeBlocks>(entry.TimeBlock, out TimeBlocks timeBlock))
                {
                    List<Professions> professions = new List<Professions>();
                    foreach (string professionStr in entry.Professions)
                    {
                        if (EnumParser.TryParse<Professions>(professionStr, out Professions profession))
                        {
                            professions.Add(profession);
                        }
                    }
                    // Set explicit property based on time block
                    switch (timeBlock)
                    {
                        case TimeBlocks.Morning:
                            location.MorningProfessions = professions;
                            break;
                        case TimeBlocks.Midday:
                            location.MiddayProfessions = professions;
                            break;
                        case TimeBlocks.Afternoon:
                            location.AfternoonProfessions = professions;
                            break;
                        case TimeBlocks.Evening:
                            location.EveningProfessions = professions;
                            break;
                    }
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

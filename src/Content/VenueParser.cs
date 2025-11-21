using System.IO;

public static class VenueParser
{
    /// <summary>
    /// Convert a VenueDTO to a Venue domain model
    /// </summary>
    public static Venue ConvertDTOToVenue(VenueDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Venue DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Venue {dto.Id} missing required 'Name' field");

        // HIGHLANDER: centerHex NO LONGER REQUIRED in JSON - calculated procedurally by VenueGeneratorService
        // Authored venues: Parsed WITHOUT centerHex, placed by VenueGeneratorService.PlaceAuthoredVenues()
        // Runtime venues: Generated WITH centerHex by VenueGeneratorService.GenerateVenue()

        // Parse hexAllocation strategy
        HexAllocationStrategy hexAllocation = HexAllocationStrategy.ClusterOf7; // Default
        if (!string.IsNullOrEmpty(dto.HexAllocation))
        {
            if (!Enum.TryParse<HexAllocationStrategy>(dto.HexAllocation, true, out hexAllocation))
            {
                throw new InvalidDataException(
                    $"Venue '{dto.Name}' has invalid hexAllocation '{dto.HexAllocation}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames(typeof(HexAllocationStrategy)))}");
            }
        }

        // Parse LocationType string to VenueType enum (fail fast on invalid values)
        VenueType venueType = VenueType.Wilderness;  // Default
        if (!string.IsNullOrEmpty(dto.LocationType))
        {
            if (!Enum.TryParse<VenueType>(dto.LocationType, true, out venueType))
            {
                // ADR-007: Use Name instead of Id in error messages
                throw new InvalidDataException($"Venue {dto.Name} has invalid LocationType '{dto.LocationType}'. Valid values: {string.Join(", ", Enum.GetNames(typeof(VenueType)))}");
            }
        }

        // ADR-007: Constructor uses Name only (no Id parameter)
        Venue venue = new Venue(dto.Name)
        {
            Description = dto.Description,
            // District object reference resolved in second pass by PackageLoader (LinkRegionDistrictVenueReferences)
            Tier = dto.Tier,
            Type = venueType,  // Strongly-typed enum (replaces LocationTypeString)
            // SPATIAL PROPERTIES:
            // CenterHex NOT set here - calculated procedurally by VenueGeneratorService.PlaceAuthoredVenues()
            HexAllocation = hexAllocation,  // Strategy (ClusterOf7, SingleHex) defined in JSON
            MaxLocations = dto.MaxLocations ?? 20  // Default capacity budget
        };

        return venue;

    }
}

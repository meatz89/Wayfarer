using System.IO;
using Wayfarer.GameState.Enums;

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

        // Parse LocationType string to VenueType enum (fail fast on invalid values)
        VenueType venueType = VenueType.Wilderness;  // Default
        if (!string.IsNullOrEmpty(dto.LocationType))
        {
            if (!Enum.TryParse<VenueType>(dto.LocationType, true, out venueType))
            {
                throw new InvalidDataException($"Venue {dto.Id} has invalid LocationType '{dto.LocationType}'. Valid values: {string.Join(", ", Enum.GetNames(typeof(VenueType)))}");
            }
        }

        Venue venue = new Venue(dto.Id, dto.Name)
        {
            Description = dto.Description,
            District = dto.DistrictId,
            Tier = dto.Tier,
            Type = venueType  // ✅ Strongly-typed enum (replaces LocationTypeString)
        };

        return venue;

    }
}

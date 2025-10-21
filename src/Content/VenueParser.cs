using System.Text.Json;

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

        Venue venue = new Venue(dto.Id, dto.Name)
        {
            Description = dto.Description,
            District = dto.DistrictId,
            Tier = dto.Tier,
            LocationTypeString = dto.LocationType
        };

        return venue;

    }
}

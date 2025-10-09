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

        // Location is a CONTAINER - only create basic organizational structure
        Location location = new Location(dto.Id, dto.Name)
        {
            Description = dto.Description,
            District = dto.DistrictId,
            Tier = dto.Tier,
            LocationTypeString = dto.LocationType // Display string only
        };

        // All gameplay properties (EnvironmentalProperties, AvailableProfessionsByTime,
        // AccessRequirement, AvailableWork, DomainTags, etc.) belong on LocationSpot.
        // They will be parsed by LocationSpotParser from LocationSpot JSON.

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

}

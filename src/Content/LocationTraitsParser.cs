public static class LocationTraitsParser
{
    /// <summary>
    /// Parse Venue traits from location type
    /// Location is the gameplay entity with all mechanical properties
    /// </summary>
    public static List<string> ParseLocationTraits(Location location, TimeBlocks currentTime)
    {
        List<string> traits = new List<string>();

        if (location == null)
            return traits;

        // Add location-type specific trait
        string locationTypeTrait = location.LocationType.ToString();
        if (!string.IsNullOrEmpty(locationTypeTrait))
        {
            traits.Add(locationTypeTrait);
        }

        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }

}

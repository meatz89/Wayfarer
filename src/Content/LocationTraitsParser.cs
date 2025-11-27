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

        // Add orthogonal categorical properties
        if (location.Role != default)
            traits.Add(location.Role.ToString());
        if (location.Purpose != default)
            traits.Add(location.Purpose.ToString());
        if (location.Environment != default)
            traits.Add(location.Environment.ToString());

        // Limit to 3-4 most relevant traits (as per mockup)
        return traits.Take(4).ToList();
    }

}

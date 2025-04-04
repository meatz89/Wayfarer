public static class LocationFactory
{
    public static Location Create(
        string name,
        string description,
        List<string> connectedLocations,
        List<LocationSpot> spots)
    {
        return new Location();
    }
}
public static class LocationSpotFactory
{
    public static LocationSpot Create(
        string name,
        string description,
        string locationName,
        string interactionType,
        string interactionDescription,
        string position)
    {
        return new LocationSpot();
    }

    public static LocationSpot Create(
        string name,
        string description,
        string locationName,
        string interactionType,
        string interactionDescription,
        string position,
        List<string> residentCharacterIds,
        List<string> associatedOpportunityIds)
    {
        return new LocationSpot();
    }
}

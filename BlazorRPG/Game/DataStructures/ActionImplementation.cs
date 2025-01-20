public record ActionImplementation
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; } = new();
    public List<Outcome> Rewards { get; set; } = new();
    public List<TimeSlots> TimeSlots { get; set; } = new();
    public LocationArchetypes LocationArchetype { get; set; } = new();
    public CrowdDensity CrowdDensity { get; set; } = new();
    public LocationScale LocationScale { get; set; } = new();
    public List<LocationPropertyCondition> SpotAvailabilityConditions { get; set; } = new();

    public bool CanExecute(PlayerState player)
    {
        return Requirements.All(r => r.IsSatisfied(player));
    }

    // Method to check if the action is available at a location
    public bool IsAvailableAt(Location location, LocationSpot locationSpot)
    {
        bool isMet = SpotAvailabilityConditions.All(c => c.IsMet(locationSpot.SpotProperties));

        if (LocationArchetype != location.LocationArchetype) isMet = false;
        if (CrowdDensity != location.CrowdDensity) isMet = false;
        if (LocationScale != location.LocationScale) isMet = false;

        return isMet;
    }

}

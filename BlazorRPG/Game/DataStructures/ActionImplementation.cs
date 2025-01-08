public record ActionImplementation
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; } = new();
    public List<Outcome> Rewards { get; set; } = new();
    public List<TimeSlots> TimeSlots { get; set; } = new();
    public List<LocationPropertyCondition> AvailabilityConditions { get; set; } = new();

    public bool CanExecute(PlayerState player)
    {
        return Requirements.All(r => r.IsSatisfied(player));
    }

    // Method to check if the action is available at a location
    public bool IsAvailableAt(Location location)
    {
        return AvailabilityConditions.All(c => c.IsMet(location.LocationProperties));
    }

}

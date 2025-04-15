public class LocationSpot
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; } 
    public List<string> ActionIds { get; set; } = new();

    public string LocationName { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    public string InteractionDescription { get; set; }

    public Population? Population { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Physical? Physical { get; set; }
    public Illumination? Illumination { get; set; }
    public string Character { get; set; }


    // Called when time changes
    public void OnTimeChanged(TimeWindows newTimeWindow)
    {
        // Update properties based on spot type and time
        switch (newTimeWindow)
        {
            case TimeWindows.Night:
                // Example: Marketplace spots become empty at night
                if (Name.Contains("Market") || Name.Contains("Shop"))
                {
                    // Make shops closed/empty at night
                    SetPopulationProperty(Population.Scholarly); // Empty
                }
                break;

            case TimeWindows.Morning:
                // Shops open in morning
                if (Name.Contains("Market") || Name.Contains("Shop"))
                {
                    SetPopulationProperty(Population.Quiet); // Just opening
                }
                break;

            case TimeWindows.Afternoon:
                // Busy shopping hours
                if (Name.Contains("Market") || Name.Contains("Shop"))
                {
                    SetPopulationProperty(Population.Crowded); // Busy
                }
                break;
        }
    }

    private void SetPopulationProperty(Population population)
    {
        // Similar property update logic as in Location
        // Implementation details...
    }

    public LocationSpot()
    {
    }

    public LocationSpot(
        string name,
        string description,
        string locationName,
        Population? population,
        Atmosphere? atmosphere,
        Physical? physical,
        Illumination? illumination,
        List<string> actionIds)
    {
        Name = name;
        Description = description;
        LocationName = locationName;
        Population = population;
        Atmosphere = atmosphere;
        Physical = physical;
        Illumination = illumination;
        ActionIds = actionIds;
    }
}
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
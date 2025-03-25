public class LocationSpot
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }

    // Connections
    public string LocationName { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    // Interaction
    public string InteractionType { get; set; }  // "Character", "Quest", "Shop", "Feature", etc.
    public string InteractionDescription { get; set; }

    // Visual/positioning data (for map display)
    public string Position { get; set; }  // "North", "Center", "Southeast", etc.

    public Population? Population { get; set; }
    public Economic? Economic { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Physical? Physical { get; set; }
    public Illumination? Illumination { get; set; }
    public List<ActionNames> ActionNames { get; set; } = new();
    public List<ActionImplementation> Actions { get; set; } = new();
    public string Character { get; internal set; }

    public void AddAction(ActionImplementation baseAction)
    {
        Actions.Add(baseAction);
    }

    public bool HasProperty<T>(T locationSpotProperty) where T : IEnvironmentalProperty
    {
        if (locationSpotProperty is Population Population)
        {
            return Population != null && Population == Population;
        }
        else if (locationSpotProperty is Economic Economic)
        {
            return Economic != null && Economic == Economic;
        }
        else if (locationSpotProperty is Physical Physical)
        {
            return Physical != null && Physical == Physical;
        }
        else if (locationSpotProperty is Illumination Illumination)
        {
            return Illumination != null && Illumination == Illumination;
        }
        else
        {
            // You can handle other types or throw an exception if needed
            throw new ArgumentException($"Unsupported property type: {typeof(T)}");
        }
    }

    public LocationSpot()
    {

    }

    public LocationSpot(
        string name,
        string locationName,
        Population? Population,
        Economic? Economic,
        Atmosphere? atmosphere,
        Physical? Physical,
        Illumination? Illumination,
        List<ActionNames> actionNames)
    {
        Name = name;
        LocationName = locationName;
        Population = Population;
        Economic = Economic;
        Atmosphere = atmosphere;
        Physical = Physical;
        Illumination = Illumination;
        ActionNames = actionNames;
    }
}
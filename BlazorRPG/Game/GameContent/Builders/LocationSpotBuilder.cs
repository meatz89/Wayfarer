public class LocationSpotBuilder
{
    // Core properties for the location spot
    private readonly LocationNames locationName;
    private readonly LocationTypes locationType;
    private readonly SpaceProperties spaceProperties;
    private readonly SocialContext socialContext;
    private readonly ActivityProperties activityProperties;
    private LocationSpotNames spotName;
    private CharacterNames character;
    private List<ActionImplementation> characterActions = new();
    private ActionContextBuilder contextBuilder;

    // Simple requirements and rewards for direct action configuration
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private int timeInvestment = 1;

    public LocationSpotBuilder(LocationNames location, LocationTypes locationType)
    {
        this.locationName = location;
        this.locationType = locationType;
        this.contextBuilder = new ActionContextBuilder(locationType);
    }

    public LocationSpotBuilder WithCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder WithEnergyCost(int cost, EnergyTypes energyType)
    {
        requirements.Add(new EnergyRequirement(energyType, cost));
        costs.Add(new EnergyOutcome(energyType, -cost));
        return this;
    }

    public LocationSpotBuilder WithCoinCost(int cost)
    {
        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public LocationSpotBuilder WithCoinReward(int reward)
    {
        rewards.Add(new CoinsOutcome(reward));
        return this;
    }

    public LocationSpotBuilder WithOutputResource(ResourceTypes resource, int count)
    {
        rewards.Add(new ResourceOutcome(resource, count));
        return this;
    }

    // Advanced context-based action generation
    public LocationSpotBuilder WithContext(Action<ActionContextBuilder> buildContext)
    {
        ActionContextBuilder builder = new ActionContextBuilder(locationType);
        buildContext(builder);
        return this;
    }


    public LocationSpot Build()
    {
        // Build the ActionGenerationContext
        ActionGenerationContext context = contextBuilder.Build();

        // Determine LocationSpotName based on the built context
        LocationSpotNames locationSpotName = LocationSpotMapper.GetLocationSpot(
            context.LocationType,
            context.Space.Exposure,
            context.Space.Scale);

        // Update the context with the LocationSpotName
        context = contextBuilder.WithLocationSpotName(locationSpotName).Build();

        return new LocationSpot(
            locationSpotName, // Use the generated name
            locationName,
            character,
            context,
            characterActions,
            TimeSlots.Night

        );
    }
}
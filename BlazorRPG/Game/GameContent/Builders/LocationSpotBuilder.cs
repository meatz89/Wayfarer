public class LocationSpotBuilder
{
    // Core properties for the location spot
    private readonly LocationNames location;
    private readonly LocationTypes locationType;
    private LocationSpotNames spotName;
    private CharacterNames character;
    private List<ActionImplementation> characterActions = new();

    // Simple requirements and rewards for direct action configuration
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private int timeInvestment = 1;
    private ActionGenerationContext actionGenerationContext;

    public LocationSpotBuilder(LocationNames location, LocationTypes locationType)
    {
        this.location = location;
        this.locationType = locationType;
    }

    // Core setup methods
    public LocationSpotBuilder ForLocationSpot(LocationSpotNames spot)
    {
        this.spotName = spot;
        return this;
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

        actionGenerationContext = builder.Build();
        return this;
    }

    public LocationSpot Build()
    {
        return new LocationSpot(
            spotName,
            location,
            character,
            actionGenerationContext,
            characterActions
        );
    }
}
public class LocationPropertiesBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private ActivityTypes activityType;

    private List<TimeSlots> timeSlots = new();
    private List<BasicActionDefinition> actions = new();

    internal LocationPropertiesBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    internal LocationPropertiesBuilder SetLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    internal LocationPropertiesBuilder SetActivityType(ActivityTypes mingle)
    {
        this.activityType = activityType;
        return this;
    }

    internal LocationPropertiesBuilder AddTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlots.Add(timeSlot);
        return this;
    }

    internal LocationPropertiesBuilder SetAccessType(AccessTypes open)
    {
        return this;
    }

    internal LocationPropertiesBuilder SetShelterStatus(ShelterStates none)
    {
        return this;
    }

    internal LocationPropertiesBuilder SetDangerLevel(DangerLevels safe)
    {
        return this;
    }

    public LocationPropertiesBuilder AddLaborAction()
    {
        actions.Add(BasicActionDefinitionContent.LaborAction);

        return this;
    }

    public LocationPropertiesBuilder AddDiscussAction()
    {
        actions.Add(BasicActionDefinitionContent.DiscussAction);

        return this;
    }

    public LocationPropertiesBuilder AddRestAction()
    {
        actions.Add(BasicActionDefinitionContent.RestAction);

        return this;
    }

    public LocationPropertiesBuilder AddTradeAction(TradeDirections tradeDirection, TradeResourceTypes tradeResource)
    {
        BasicActionDefinition tradeAction = (tradeDirection, tradeResource) switch
        {
            (TradeDirections.Buy, TradeResourceTypes.Food) => BasicActionDefinitionContent.FoodBuyAction,
            (TradeDirections.Sell, TradeResourceTypes.Food) => BasicActionDefinitionContent.FoodSellAction,
            _ => throw new ArgumentException("Invalid trade configuration")
        };

        actions.Add(tradeAction);

        return this;
    }

    public LocationProperties Build()
    {
        return new LocationProperties
        {
            Location = location,
            LocationType = locationType,
            ActivityType = activityType,
            TimeSlots = timeSlots,
            Actions = actions
        };
    }
}
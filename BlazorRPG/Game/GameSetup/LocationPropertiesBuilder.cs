public class LocationPropertiesBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private ActivityTypes activityType;
    private TradeResourceTypes tradeResourceType;
    private TradeDirections tradeDirection;
    private List<TimeSlots> timeSlots = new();
    private BasicActionDefinition primaryAction;

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

    internal LocationPropertiesBuilder SetTradeResourceType(TradeResourceTypes tradeResourceType)
    {
        this.tradeResourceType = tradeResourceType;
        return this;
    }

    internal LocationPropertiesBuilder SetTradeResourceType(TradeDirections tradeDirection)
    {
        this.tradeDirection = tradeDirection;
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

    public LocationPropertiesBuilder AddPrimaryAction(BasicActionTypes basicActionType)
    {
        if (basicActionType == BasicActionTypes.Trade)
        {
            primaryAction = (tradeDirection, tradeResourceType) switch
            {
                (TradeDirections.Buy, TradeResourceTypes.Food) => BasicActionDefinitionContent.FoodBuyAction,
                (TradeDirections.Sell, TradeResourceTypes.Food) => BasicActionDefinitionContent.FoodSellAction,
                _ => throw new ArgumentException("Invalid trade configuration")
            };
        }
        else
        {
            primaryAction = basicActionType switch
            {
                BasicActionTypes.Labor => BasicActionDefinitionContent.LaborAction,
                BasicActionTypes.Discuss => BasicActionDefinitionContent.DiscussAction,
                BasicActionTypes.Rest => BasicActionDefinitionContent.RestAction,
                _ => throw new ArgumentException("Invalid action type")
            };
        }

        return this;
    }

    public LocationProperties Build()
    {
        return new LocationProperties
        {
            Location = location,
            LocationType = locationType,
            ActivityType = activityType,
            TradeResourceType = tradeResourceType,
            TradeDirection = tradeDirection,
            TimeSlots = timeSlots,
            PrimaryAction = primaryAction
        };
    }
}
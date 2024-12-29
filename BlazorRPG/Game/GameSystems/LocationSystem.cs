
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<LocationProperties> locationProperties;
    private readonly List<BasicActionDefinition> basicActionDefinitions;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.locationProperties = contentProvider.GetLocationProperties();
        this.basicActionDefinitions = contentProvider.GetBasicActionDefinitions();
    }

    public List<BasicActionTypes> GetLocationActionsFor(LocationNames location)
    {
        LocationProperties actionsForLocation = locationProperties.FirstOrDefault(n => n.Location == location);
        if (actionsForLocation != null)
        {
            List<BasicActionTypes> actions = new();
            actions.Add(actionsForLocation.PrimaryAction);
            return actions;
        }

        return new List<BasicActionTypes>();
    }

    public SystemActionResult ExecuteAction(LocationNames location, BasicActionTypes basicAction)
    {
        BasicActionDefinition actionDefinition = FindBasicActionDefinition(location, basicAction);

        if (actionDefinition == null) return SystemActionResult.Failure("No action definition");

        foreach (IOutcome outcome in actionDefinition.Outcomes)
        {
            if (outcome is CoinsOutcome outcomeMoney)
            {
                gameState.AddCoinsChange(outcomeMoney);
            }
            if (outcome is FoodOutcome outcomeFood)
            {
                gameState.AddFoodChange(outcomeFood);
            }
            if (outcome is HealthOutcome outcomeHealth)
            {
                gameState.AddHealthChange(outcomeHealth);
            }
            if (outcome is PhysicalEnergyOutcome outcomePhysicalEnergy)
            {
                gameState.AddPhysicalEnergyChange(outcomePhysicalEnergy);
            }
            if (outcome is FocusEnergyOutcome outcomeFocusEnergy)
            {
                gameState.AddFocusEnergyChange(outcomeFocusEnergy);
            }
            if (outcome is SocialEnergyOutcome outcomeSocialEnergy)
            {
                gameState.AddSocialEnergyChange(outcomeSocialEnergy);
            }
            if (outcome is SkillLevelOutcome outcomeSkillLevel)
            {
                gameState.AddSkillLevelChange(outcomeSkillLevel);
            }
            if (outcome is ItemOutcome outcomeItem)
            {
                gameState.AddItemChange(outcomeItem);
            }
        }

        return SystemActionResult.Success("Action completed successfully");
    }

    private BasicActionDefinition FindBasicActionDefinition(LocationNames location, BasicActionTypes basicAction)
    {
        LocationProperties locationProperties = GetLocationProperties(location);
        List<BasicActionDefinition> basicActionDefinition = basicActionDefinitions.Where(x => x.ActionType == basicAction).ToList();

        ActivityTypes activityType = locationProperties.ActivityType;
        LocationTypes locationType = locationProperties.LocationType;
        TradeResourceTypes tradeResourceType = locationProperties.TradeResourceType;
        TradeDirections tradeDirection = locationProperties.TradeDirection;
        switch (activityType)
        {
            case ActivityTypes.Trade:

                switch (tradeDirection)
                {
                    case TradeDirections.Buy:

                        switch (tradeResourceType)
                        {
                            case TradeResourceTypes.Food:
                                return BasicActionDefinitionContent.FoodBuyAction;

                            default:
                                return basicActionDefinition.FirstOrDefault();
                        }
                        break;

                    case TradeDirections.Sell:

                        switch (tradeResourceType)
                        {
                            case TradeResourceTypes.Food:
                                return BasicActionDefinitionContent.FoodSellAction;

                            default:
                                return basicActionDefinition.FirstOrDefault();
                        }
                    default:
                        return basicActionDefinition.FirstOrDefault();
                }
                break;
            default:
                return basicActionDefinition.FirstOrDefault();
        }
    }

    private LocationProperties GetLocationProperties(LocationNames location)
    {
        return locationProperties.FirstOrDefault(x =>  x.Location == location);
    }
}
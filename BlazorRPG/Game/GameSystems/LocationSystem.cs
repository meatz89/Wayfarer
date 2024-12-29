
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<LocationProperties> allLocationProperties;
    private readonly List<BasicActionDefinition> basicActionDefinitions;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocationProperties = contentProvider.GetLocationProperties();
        this.basicActionDefinitions = contentProvider.GetBasicActionDefinitions();
    }

    public List<BasicActionTypes> GetLocationActionsFor(LocationNames location)
    {
        LocationProperties actionsForLocation = allLocationProperties.FirstOrDefault(n => n.Location == location);
        if (actionsForLocation != null)
        {
            List<BasicActionTypes> actions = new();
            actions.Add(actionsForLocation.PrimaryAction.ActionType);
            return actions;
        }

        return new List<BasicActionTypes>();
    }

    public SystemActionResult ExecuteAction(LocationNames location, BasicActionTypes basicAction)
    {
        LocationProperties locationProperties = allLocationProperties.FirstOrDefault(x => x.Location == location);
        if (locationProperties == null) return SystemActionResult.Failure("Location not found");

        BasicActionDefinition primaryAction = locationProperties.PrimaryAction;
        
        foreach (IOutcome outcome in primaryAction.Outcomes)
        {
            ApplyOutcome(outcome);
        }

        return SystemActionResult.Success("Action completed successfully");
    }

    private void ApplyOutcome(IOutcome outcome)
    {
        switch (outcome)
        {
            case CoinsOutcome coinsOutcome:
                gameState.AddCoinsChange(coinsOutcome);
                break;
            case FoodOutcome foodOutcome:
                gameState.AddFoodChange(foodOutcome);
                break;
            case HealthOutcome healthOutcome:
                gameState.AddHealthChange(healthOutcome);
                break;
            case PhysicalEnergyOutcome physicalEnergyOutcome:
                gameState.AddPhysicalEnergyChange(physicalEnergyOutcome);
                break;
            case FocusEnergyOutcome focusEnergyOutcome:
                gameState.AddFocusEnergyChange(focusEnergyOutcome);
                break;
            case SocialEnergyOutcome socialEnergyOutcome:
                gameState.AddSocialEnergyChange(socialEnergyOutcome);
                break;
            case SkillLevelOutcome skillLevelOutcome:
                gameState.AddSkillLevelChange(skillLevelOutcome);
                break;
            case ItemOutcome itemOutcome:
                gameState.AddItemChange(itemOutcome);
                break;
        }
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
        return allLocationProperties.FirstOrDefault(x =>  x.Location == location);
    }
}
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

    public SystemActionResult ExecuteAction(BasicActionTypes basicAction)
    {
        BasicActionDefinition actionDefinition = basicActionDefinitions.FirstOrDefault(x => x.ActionType == basicAction);

        foreach (IOutcome outcome in actionDefinition.Outcomes)
        {
            if (outcome is CoinsOutcome outcomeMoney)
            {
                gameState.AddCoinsChange(outcomeMoney);
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
}
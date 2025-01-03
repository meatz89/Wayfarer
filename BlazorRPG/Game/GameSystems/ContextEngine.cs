public class ContextEngine
{
    private readonly GameState gameState;

    private readonly StatusSystem statusSystem;
    private readonly ReputationSystem reputationSystem;
    private readonly AchievementSystem achievementSystem;

    private readonly ActionValidator actionValidator;
    private readonly MessageSystem messageSystem;

    public ContextEngine(
        GameState gameState,
        StatusSystem statusSystem,
        ReputationSystem reputationSystem,
        AchievementSystem achievementSystem,
        ActionValidator actionValidator,
        MessageSystem messageSystem
        )
    {
        this.gameState = gameState;

        this.statusSystem = statusSystem;
        this.reputationSystem = reputationSystem;
        this.achievementSystem = achievementSystem;
        this.actionValidator = actionValidator;
        this.messageSystem = messageSystem;
    }

    public bool CanExecuteInContext(BasicAction basicAction)
    {
        return actionValidator.CanExecuteAction(basicAction);
    }

    public bool MeetsRequirements(QuestStep questStep)
    {
        BasicAction basicAction = questStep.QuestAction;
        return actionValidator.CanExecuteAction(basicAction);
    }

    public void ProcessActionOutcome(BasicAction basicAction)
    {
        foreach (IOutcome outcome in basicAction.Outcomes)
        {
            ApplyOutcome(outcome);
        }
    }

    private void ApplyOutcome(IOutcome outcome)
    {
        switch (outcome)
        {
            case CoinsOutcome coinsOutcome:
                AddCoinsChange(coinsOutcome);
                break;
            case FoodOutcome foodOutcome:
                AddFoodChange(foodOutcome);
                break;
            case HealthOutcome healthOutcome:
                AddHealthChange(healthOutcome);
                break;
            case PhysicalEnergyOutcome physicalEnergyOutcome:
                AddPhysicalEnergyChange(physicalEnergyOutcome);
                break;
            case FocusEnergyOutcome focusEnergyOutcome:
                AddFocusEnergyChange(focusEnergyOutcome);
                break;
            case SocialEnergyOutcome socialEnergyOutcome:
                AddSocialEnergyChange(socialEnergyOutcome);
                break;
            case SkillLevelOutcome skillLevelOutcome:
                AddSkillLevelChange(skillLevelOutcome);
                break;
            case ResourceOutcome itemOutcome:
                AddResourceChange(itemOutcome);
                break;
            case EndDayOutcome endDayOutcome:
                AddEndDayChange(endDayOutcome);
                break;
        }
    }

    public void AddCoinsChange(CoinsOutcome money)
    {
        bool neededChange = gameState.ModifyCoins(money.Amount);
        if (neededChange)
        {
            messageSystem.changes.Coins.Add(money);
        }
    }

    public void AddFoodChange(FoodOutcome food)
    {
        bool neededChange = gameState.ModifyFood(food.Amount);
        if (neededChange)
        {
            messageSystem.changes.Food.Add(food);
        }
    }

    public void AddHealthChange(HealthOutcome health)
    {
        bool neededChange = gameState.ModifyHealth(health.Amount);
        if (neededChange)
        {
            messageSystem.changes.Health.Add(health);
        }
    }

    public void AddPhysicalEnergyChange(PhysicalEnergyOutcome physicalEnergy)
    {
        bool neededChange = gameState.ModifyPhysicalEnergy(physicalEnergy.Amount);
        if (neededChange)
        {
            messageSystem.changes.PhysicalEnergy.Add(physicalEnergy);
        }
    }

    public void AddFocusEnergyChange(FocusEnergyOutcome focusEnergy)
    {
        bool neededChange = gameState.ModifyFocusEnergy(focusEnergy.Amount);
        if (neededChange)
        {
            messageSystem.changes.FocusEnergy.Add(focusEnergy);
        }
    }

    public void AddSocialEnergyChange(SocialEnergyOutcome socialEnergy)
    {
        bool neededChange = gameState.ModifySocialEnergy(socialEnergy.Amount);
        if (neededChange)
        {
            messageSystem.changes.SocialEnergy.Add(socialEnergy);
        }
    }

    public void AddSkillLevelChange(SkillLevelOutcome skillLevel)
    {
        bool neededChange = gameState.ModifySkillLevel(skillLevel.SkillType, skillLevel.Amount);
        if (neededChange)
        {
            messageSystem.changes.SkillLevel.Add(skillLevel);
        }
    }

    public void AddResourceChange(ResourceOutcome resource)
    {
        bool neededChange = gameState.ModifyItem(resource.ChangeType, resource.Resource, resource.Count);
        if (neededChange)
        {
            messageSystem.changes.Resources.Add(resource);
        }
    }

    public void AddEndDayChange(EndDayOutcome endDay)
    {
        //this.AdvanceTimeTo(7 - 2);
    }
}

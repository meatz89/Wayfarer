public class NarrativeSystem
{
    private readonly GameState gameState;
    private readonly List<Narrative> narratives;

    public NarrativeSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.narratives = contentProvider.GetNarratives();
    }

    public Narrative GetNarrativeFor(BasicActionTypes actionType)
    {
        return narratives.FirstOrDefault(n => n.ActionType == actionType);
    }

    public bool CanExecute(NarrativeStage narrativeStage, int choiceId)
    {
        List<Choice> choices = narrativeStage.Choices;
        Choice choice = choices.FirstOrDefault(c => c.Index == choiceId);
        if (choice == null) return false;

        foreach (IRequirement req in choice.Requirements)
        {
            bool hasRequirement = CheckRequirement(req);
            if (!hasRequirement) return false;
        }

        return true;
    }

    private bool CheckRequirement(IRequirement requirement)
    {
        if (requirement is MoneyRequirement money)
        {
            return gameState.PlayerInfo.Money >= money.Amount;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.PlayerInfo.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return gameState.PlayerInfo.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return gameState.PlayerInfo.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return gameState.PlayerInfo.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.PlayerInfo.Skills[skillLevel.SkillType] >= skillLevel.Amount;
        }
        if (requirement is ItemRequirement item)
        {
            return false;
        }
        return false;
    }

    public SystemActionResult ExecuteChoice(Narrative narrative, NarrativeStage narrativeStage, int choiceId)
    {
        if (narrative?.Stages == null || !narrative.Stages.Any())
            return SystemActionResult.Failure("Invalid narrative state");

        Choice choice = narrative.Stages[0].Choices.FirstOrDefault(c => c.Index == choiceId);
        if (choice == null)
            return SystemActionResult.Failure("Invalid choice");

        if (!CanExecute(narrativeStage, choiceId))
            return SystemActionResult.Failure("Cannot execute this choice");

        foreach (IOutcome outcome in choice.Outcomes)
        {
            if (outcome is MoneyOutcome outcomeMoney)
            {
                gameState.AddMoneyChange(outcomeMoney);
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
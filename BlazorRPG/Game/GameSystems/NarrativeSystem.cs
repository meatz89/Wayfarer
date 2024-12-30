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

    public List<IOutcome> GetChoiceOutcomes(Narrative narrative, NarrativeStage narrativeStage, int choiceId)
    {
        if (narrative?.Stages == null || !narrative.Stages.Any())
            return null;

        Choice choice = narrative.Stages[0].Choices.FirstOrDefault(c => c.Index == choiceId);
        if (choice == null)
            return null;

        if (!CanExecute(narrativeStage, choiceId))
            return null;

        return choice.Outcomes;
    }
    
    private bool CheckRequirement(IRequirement requirement)
    {
        if (requirement is CoinsRequirement money)
        {
            return gameState.Player.Coins >= money.Amount;
        }
        if (requirement is FoodRequirement food)
        {
            return gameState.Player.Health >= food.Amount;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.Player.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return gameState.Player.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return gameState.Player.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return gameState.Player.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.Player.Skills[skillLevel.SkillType] >= skillLevel.Amount;
        }
        if (requirement is ItemRequirement item)
        {
            return false;
        }
        return false;
    }
}
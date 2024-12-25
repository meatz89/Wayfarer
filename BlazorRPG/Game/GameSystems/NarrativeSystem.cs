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

    public bool CanExecute(Narrative narrative, int choiceIndex)
    {
        if (narrative == null) return false;

        Choice choice = narrative.Choices.FirstOrDefault(n => n.Index == choiceIndex);
        if (choice == null) return false;

        foreach (IRequirement req in choice.Requirements)
        {
            if (req is ResourceReq resourceReq)
            {
                int currentAmount = GetResourceAmount(resourceReq.ResourceType);
                if (currentAmount < resourceReq.Amount) return false;
            }
            if (req is SkillReq skillReq)
            {
                // Add skill check logic when skills are implemented
                return false;
            }
        }

        return true;
    }

    private int GetResourceAmount(ResourceTypes resourceType)
    {
        return resourceType switch
        {
            ResourceTypes.Money => gameState.PlayerInfo.Money,
            ResourceTypes.Health => gameState.PlayerInfo.Health,
            ResourceTypes.PhysicalEnergy => gameState.PlayerInfo.PhysicalEnergy,
            ResourceTypes.FocusEnergy => gameState.PlayerInfo.FocusEnergy,
            ResourceTypes.SocialEnergy => gameState.PlayerInfo.SocialEnergy,
            _ => 0
        };
    }

    public SystemActionResult ExecuteChoice(Narrative narrative, int choiceIndex)
    {
        if (!CanExecute(narrative, choiceIndex))
        {
            return SystemActionResult.Failure("Cannot execute this choice");
        }

        Choice choice = narrative.Choices.FirstOrDefault(n => n.Index == choiceIndex);
        if (choice == null)
        {
            return SystemActionResult.Failure("Invalid choice");
        }

        ActionResultMessages changes = new();

        foreach (IOutcome outcome in choice.Outcomes)
        {
            if (outcome is ResourceOutcome resourceOutcome)
            {
                gameState.AddResourceChange(resourceOutcome);
                changes.Resources.Add(resourceOutcome);
            }
        }

        return SystemActionResult.Success("Action completed successfully", changes);
    }
}
public class NarrativeSystem
{
    private readonly GameState gameState;
    private readonly List<Narrative> narratives;

    private readonly KnowledgeSystem knowledgeSystem;
    private readonly CharacterRelationshipSystem relationshipSystem;

    public NarrativeSystem(
        GameState gameState,
        GameContentProvider contentProvider,
        KnowledgeSystem knowledgeSystem,
        CharacterRelationshipSystem relationshipSystem)
    {
        this.gameState = gameState;
        this.narratives = contentProvider.GetNarratives();
        this.knowledgeSystem = knowledgeSystem;
        this.relationshipSystem = relationshipSystem;
    }


    public Narrative GetAvailableNarrative(BasicActionTypes action, LocationNames location)
    {
        return new Narrative();
    }

    public List<Choice> GetAvailableChoices(NarrativeStage stage)
    {
        return new List<Choice>();
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

        foreach (Requirement req in choice.Requirements)
        {
            bool hasRequirement = CheckRequirement(req);
            if (!hasRequirement) return false;
        }

        return true;
    }

    public List<Outcome> GetChoiceOutcomes(Narrative narrative, NarrativeStage narrativeStage, int choiceId)
    {
        if (narrative?.Stages == null || !narrative.Stages.Any())
            return null;

        Choice choice = narrative.Stages[0].Choices.FirstOrDefault(c => c.Index == choiceId);
        if (choice == null)
            return null;

        if (!CanExecute(narrativeStage, choiceId))
            return null;

        List<Outcome> costs = choice.Costs;
        List<Outcome> rewards = choice.Rewards;

        var outcomes = new List<Outcome>();
        outcomes.AddRange(costs);
        outcomes.AddRange(rewards);
        return outcomes;
    }

    private bool CheckRequirement(Requirement requirement)
    {
        if (requirement is CoinsRequirement money)
        {
            return gameState.Player.Coins >= money.Count;
        }
        if (requirement is InventorySlotsRequirement inventorySlot)
        {
            return gameState.Player.Inventory.GetEmptySlots() >= inventorySlot.Count;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.Player.Health >= health.Count;
        }
        if (requirement is EnergyRequirement energy)
        {
            switch (energy.EnergyType)
            {
                case EnergyTypes.Physical:
                   return gameState.Player.PhysicalEnergy >= energy.Count;

                case EnergyTypes.Focus:
                    return gameState.Player.FocusEnergy >= energy.Count;

                case EnergyTypes.Social:
                    return gameState.Player.SocialEnergy >= energy.Count;
            }
            return false;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.Player.Skills[skillLevel.SkillType] >= skillLevel.Count;
        }
        if (requirement is ResourceRequirement resource)
        {
            return false;
        }
        return false;
    }
}
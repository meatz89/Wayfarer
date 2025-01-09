public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        ChoiceConsequences consequences = choice.Consequences;

        // Check if all modified requirements are met
        if (!AreRequirementsMet(consequences.ModifiedRequirements))
        {
            return;
        }

        // Apply encounter value changes
        foreach (ValueChange valueChange in consequences.ModifiedValueChanges)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(
                valueChange.ValueType,
                valueChange.Change);
        }

        // Apply costs (like energy costs) first
        foreach (Outcome cost in consequences.ModifiedCosts)
        {
            cost.Apply(gameState.Player);
        }

        // Then apply rewards if we could pay the costs
        foreach (Outcome reward in consequences.ModifiedRewards)
        {
            reward.Apply(gameState.Player);
        }
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState.Player));
    }
}
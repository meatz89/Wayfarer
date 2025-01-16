public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(EncounterChoice choice, ChoiceCalculationResult result)
    {
        // First verify all requirements are met
        if (!AreRequirementsMet(result.Requirements))
            return;

        // Apply energy cost
        if (!ApplyEnergyCost(result.EnergyType, result.EnergyCost))
            return;

        // Apply costs
        foreach (Outcome cost in result.Costs)
        {
            cost.Apply(gameState.Player);
        }

        // Apply all value changes as a single transaction
        Dictionary<ValueTypes, int> combinedChanges = result.GetCombinedValues();
        foreach (KeyValuePair<ValueTypes, int> kvp in combinedChanges)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(kvp.Key, kvp.Value);
        }

        // Apply rewards
        foreach (Outcome reward in result.Rewards)
        {
            reward.Apply(gameState.Player);
        }
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState.Player));
    }

    private bool ApplyEnergyCost(EnergyTypes energyType, int cost)
    {
        if (!gameState.Player.CanPayEnergy(energyType, cost))
            return false;

        gameState.Player.ModifyEnergy(energyType, -cost);
        return true;
    }
}
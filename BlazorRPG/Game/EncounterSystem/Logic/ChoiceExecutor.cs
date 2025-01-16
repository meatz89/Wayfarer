public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    // UPDATED to use ChoiceCalculationResult
    public void ExecuteChoice(ChoiceCalculationResult calculationResult)
    {
        // Apply energy cost
        gameState.Player.ModifyEnergy(calculationResult.EnergyTypes, -calculationResult.EnergyCost);

        // Apply costs
        foreach (Outcome cost in calculationResult.Costs)
        {
            cost.Apply(gameState.Player);
        }

        // Apply value changes
        foreach (ValueChange change in calculationResult.ValueChanges)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(change.ValueType, change.Amount);
        }

        // Apply rewards
        foreach (Outcome reward in calculationResult.Rewards)
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
        // Check if we can pay
        if (!gameState.Player.CanPayEnergy(energyType, cost))
        {
            return false;
        }

        // Apply the cost
        gameState.Player.ModifyEnergy(energyType, -cost);
        return true;
    }
}
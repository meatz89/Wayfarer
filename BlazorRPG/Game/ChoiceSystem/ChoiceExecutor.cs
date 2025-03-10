using BlazorRPG.Game.EncounterManager;

public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(Encounter encounter, Choice choice, ChoiceCalculationResult result)
    {
        // First verify all requirements are met
        if (!AreRequirementsMet(result.Requirements))
            return;

        // Apply high pressure complications if needed

        // Apply costs
        foreach (Outcome cost in result.Costs)
        {
            cost.Apply(gameState.Player);
        }

        // Apply all value changes as a single transaction
        Dictionary<ValueTypes, int> combinedChanges = result.GetCombinedValues();
        foreach (KeyValuePair<ValueTypes, int> kvp in combinedChanges)
        {
            switch (kvp.Key)
            {
                case ValueTypes.Momentum:
                    break;

                case ValueTypes.Pressure:
                    break;
            }
        }

        // Apply rewards
        foreach (Outcome reward in result.Rewards)
        {
            reward.Apply(gameState.Player);
        }
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState));
    }
}

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

        // Apply energy cost or alternative cost
        ApplyEnergyCost(choice, result);

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

    private void ApplyEnergyCost(EncounterChoice choice, ChoiceCalculationResult result)
    {
        int energyCost = result.EnergyCost;

        if (gameState.Player.CanPayEnergy(choice.EnergyType, energyCost))
        {
            gameState.Player.ModifyEnergy(choice.EnergyType, -energyCost);
        }
        else
        {
            // Apply alternative costs based on energy type
            switch (choice.EnergyType)
            {
                case EnergyTypes.Physical:
                    int healthCost = energyCost - gameState.Player.PhysicalEnergy;
                    gameState.Player.PhysicalEnergy = 0;
                    gameState.Player.ModifyHealth(-healthCost);
                    break;
                case EnergyTypes.Focus:
                    int concentrationCost = energyCost - gameState.Player.FocusEnergy;
                    gameState.Player.FocusEnergy = 0;
                    gameState.Player.ModifyConcentration(concentrationCost);
                    break;
                case EnergyTypes.Social:
                    int reputationCost = energyCost - gameState.Player.SocialEnergy;
                    gameState.Player.SocialEnergy = 0;
                    gameState.Player.ModifyReputation(-reputationCost);
                    break;
            }
        }
    }
}
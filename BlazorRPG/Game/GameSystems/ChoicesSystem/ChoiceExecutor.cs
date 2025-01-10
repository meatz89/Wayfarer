public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        // Verify requirements before execution
        if (!AreRequirementsMet(choice.ModifiedRequirements))
        {
            return;
        }

        // Execute in specific order: Costs -> Value Changes -> Rewards
        ExecuteCosts(choice.ModifiedCosts);
        ExecuteValueChanges(choice.ModifiedValueChanges);
        ExecuteRewards(choice.ModifiedRewards);
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState.Player));
    }

    private void ExecuteCosts(List<Outcome> costs)
    {
        foreach (Outcome cost in costs)
        {
            // Return early if any cost cannot be paid
            if (!CanApplyOutcome(cost))
            {
                return;
            }
        }

        // If we can pay all costs, apply them
        foreach (Outcome cost in costs)
        {
            cost.Apply(gameState.Player);
        }
    }

    private void ExecuteValueChanges(List<ValueChange> valueChanges)
    {
        foreach (ValueChange change in valueChanges)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(
                change.ValueType,
                change.Change);
        }
    }

    private void ExecuteRewards(List<Outcome> rewards)
    {
        foreach (Outcome reward in rewards)
        {
            if (CanApplyOutcome(reward))
            {
                reward.Apply(gameState.Player);
            }
        }
    }

    private bool CanApplyOutcome(Outcome outcome)
    {
        // Check if the outcome can be applied based on current game state
        switch (outcome)
        {
            case EnergyOutcome energyOutcome:
                return gameState.Player.CanPayEnergy(energyOutcome.EnergyType, energyOutcome.Amount);

            case ResourceOutcome resourceOutcome:
                if (resourceOutcome.Amount < 0)
                {
                    return gameState.Player.HasResource(resourceOutcome.ResourceType, -resourceOutcome.Amount);
                }
                return true;

            case ReputationOutcome reputationOutcome:
                if (reputationOutcome.Change < 0)
                {
                    return gameState.Player.CanLoseReputation(reputationOutcome.ReputationType, -reputationOutcome.Change);
                }
                return true;

            case CoinsOutcome coinsOutcome:
                if (coinsOutcome.Amount < 0)
                {
                    return gameState.Player.HasCoins(-coinsOutcome.Amount);
                }
                return true;

            default:
                return true;
        }
    }
}
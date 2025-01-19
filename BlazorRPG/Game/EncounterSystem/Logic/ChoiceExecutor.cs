
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
        
        // Apply high pressure complications if needed
        if (gameState.Actions.CurrentEncounter.Context.CurrentValues.Pressure >= 7)
        {
            ApplyComplications(choice);
        }

        // Apply costs
        foreach (Outcome cost in result.Costs)
        {
            cost.Apply(gameState.Player);
        }

        // Apply all value changes as a single transaction
        Dictionary<ChangeTypes, int> combinedChanges = result.GetCombinedValues();
        foreach (KeyValuePair<ChangeTypes, int> kvp in combinedChanges)
        {
            switch (kvp.Key)
            {
                case ChangeTypes.Outcome:
                    gameState.Actions.CurrentEncounter.ModifyValue(ValueTypes.Outcome, kvp.Value);
                    break;
                case ChangeTypes.Momentum:
                    gameState.Actions.CurrentEncounter.ModifyValue(ValueTypes.Momentum, kvp.Value);
                    break;
                case ChangeTypes.Insight:
                    gameState.Actions.CurrentEncounter.ModifyValue(ValueTypes.Insight, kvp.Value);
                    break;
                case ChangeTypes.Resonance:
                    gameState.Actions.CurrentEncounter.ModifyValue(ValueTypes.Resonance, kvp.Value);
                    break;
                case ChangeTypes.Pressure:
                    gameState.Actions.CurrentEncounter.ModifyValue(ValueTypes.Pressure, kvp.Value);
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

    private void ApplyComplications(EncounterChoice choice)
    {
        ComplicationEffect complication = GetComplicationForChoice(choice);
        if (complication != null)
        {
            complication.Consequence.Apply(gameState.Player);
        }
    }

    private ComplicationEffect GetComplicationForChoice(EncounterChoice choice)
    {
        return null;
    }
}

public class ComplicationEffect
{
    public ComplicationTypes Type { get; }
    public string Description { get; }
    public Outcome Consequence { get; }

    public ComplicationEffect(ComplicationTypes type, string description, Outcome consequence)
    {
        Type = type;
        Description = description;
        Consequence = consequence;
    }
}

public enum ComplicationTypes
{
    MinorInjury,
    StressIncrease,
    ReputationLoss,
    EquipmentDamage,
    ResourceLoss,
    TimeDelay
}
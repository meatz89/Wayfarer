public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice, ChoiceCalculationResult result)
    {
        // First verify all requirements are met
        if (!AreRequirementsMet(result.Requirements))
            return;

        // Apply high pressure complications if needed
        if (gameState.Actions.CurrentEncounter.Context.CurrentValues.Pressure >= 7)
        {
            //ApplyComplications(choice);
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

        if (choice.ChoiceSlotToRemove != null)
        {
            encounter.BaseSlots.Remove(choice.ChoiceSlotToRemove);
            encounter.ModifiedSlots.Remove(choice.ChoiceSlotToRemove);
        }
        foreach (EncounterChoiceSlot choiceSlotModification in choice.ChoiceSlotModifications)
        {
            encounter.ModifiedSlots.Add(choiceSlotModification);
        }
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState));
    }
}

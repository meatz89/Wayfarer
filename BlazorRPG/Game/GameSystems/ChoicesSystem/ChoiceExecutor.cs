public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        // Apply modifications to the choice
        ApplyModifications(choice);

        // Check if all modified requirements are met
        if (!AreRequirementsMet(choice.ModifiedRequirements))
        {
            return;
        }

        // Apply encounter value changes
        foreach (ValueChange valueChange in choice.ModifiedValueChanges)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(
                valueChange.ValueType,
                valueChange.Change);
        }

        // Apply costs (like energy costs) first
        foreach (Outcome cost in choice.ModifiedCosts)
        {
            cost.Apply(gameState.Player);
        }

        // Then apply rewards if we could pay the costs
        foreach (Outcome reward in choice.ModifiedRewards)
        {
            reward.Apply(gameState.Player);
        }
    }

    private void ApplyModifications(EncounterChoice choice)
    {
        foreach (ChoiceModification modification in choice.Modifications)
        {
            switch (modification.Type)
            {
                case ModificationType.ValueChange:
                    ApplyValueChangeModification(choice, modification.ValueChange);
                    break;
                case ModificationType.Requirement:
                    ApplyRequirementModification(choice, modification.Requirement);
                    break;
                case ModificationType.Cost:
                    ApplyOutcomeModification(choice.ModifiedCosts, modification.Cost);
                    break;
                case ModificationType.Reward:
                    ApplyOutcomeModification(choice.ModifiedRewards, modification.Reward);
                    break;
                case ModificationType.EnergyCost:
                    ApplyEnergyCostModification(choice, modification);
                    break;
            }
        }
    }

    private void ApplyValueChangeModification(EncounterChoice choice, ValueChangeModification modification)
    {
        // Find the ValueChange to modify
        ValueChange valueChangeToModify = choice.ModifiedValueChanges.FirstOrDefault(vc => vc.ValueType == modification.ValueType);

        if (valueChangeToModify != null)
        {
            // Apply the modification
            switch (modification.ValueTransformation.TransformationType)
            {
                case TransformationType.Convert:
                    // For Convert, we remove the source change and add a new change of the target type
                    choice.ModifiedValueChanges.Remove(valueChangeToModify);
                    choice.ModifiedValueChanges.Add(new ValueChange(modification.ValueTransformation.TargetValue, valueChangeToModify.Change));
                    break;
                case TransformationType.Reduce:
                case TransformationType.ReduceCost:
                    // Reduce subtracts the amount
                    valueChangeToModify.Change = Math.Max(0, valueChangeToModify.Change + modification.Amount);
                    break;
                case TransformationType.Increase:
                    // Increase adds the amount
                    valueChangeToModify.Change += modification.Amount;
                    break;
                case TransformationType.Set:
                    // Set will replace one with another
                    choice.ModifiedValueChanges.Remove(valueChangeToModify);
                    choice.ModifiedValueChanges.Add(new ValueChange(modification.ValueTransformation.TargetValue, modification.Amount));
                    break;
            }
        }
    }

    private void ApplyRequirementModification(EncounterChoice choice, RequirementModification modification)
    {
        if (modification.RequirementType == "Energy")
        {
            foreach (EnergyRequirement req in choice.ModifiedRequirements.OfType<EnergyRequirement>())
            {
                req.Amount = Math.Max(0, req.Amount + modification.Amount);
            }
        }
        else
        {
            // Find the Requirement to modify based on its type
            Requirement requirementToModify = choice.ModifiedRequirements.FirstOrDefault(r => r.GetType().Name == modification.RequirementType + "Requirement");
            if (requirementToModify != null)
            {
                // You might need to use reflection or type-specific logic to adjust the requirement
                // since the exact property to modify depends on the requirement type
                AdjustRequirement(requirementToModify, modification.Amount);
            }
        }
    }

    private void ApplyEnergyCostModification(EncounterChoice choice, ChoiceModification modification)
    {
        if (modification.Type == ModificationType.EnergyCost)
        {
            foreach (EnergyOutcome cost in choice.ModifiedCosts.OfType<EnergyOutcome>())
            {
                cost.Amount = Math.Max(0, cost.Amount + modification.Requirement.Amount);
            }
        }
    }

    private void ApplyOutcomeModification(List<Outcome> outcomes, OutcomeModification modification)
    {
        // Find the Outcome to modify
        Outcome outcomeToModify = outcomes.FirstOrDefault(o => o.GetType().Name == modification.OutcomeType + "Outcome");

        if (outcomeToModify != null)
        {
            // Similar to requirements, you'll need to use reflection or type-specific logic to adjust the outcome
            // since the exact property to modify depends on the outcome type
            AdjustOutcome(outcomeToModify, modification.Amount);
        }
    }

    private void AdjustRequirement(Requirement requirement, int amount)
    {
        // Use reflection or type-specific logic to adjust the requirement
        // Example using reflection:
        System.Reflection.PropertyInfo? property = requirement.GetType().GetProperties().FirstOrDefault(p => p.Name == "Amount" || p.Name == "Count" || p.Name == "Level");
        if (property != null && property.CanWrite)
        {
            int currentValue = (int)property.GetValue(requirement);
            property.SetValue(requirement, Math.Max(0, currentValue + amount)); // Ensure non-negative
        }
    }

    private void AdjustOutcome(Outcome outcome, int amount)
    {
        // Similar to AdjustRequirement, use reflection or type-specific logic
        System.Reflection.PropertyInfo? property = outcome.GetType().GetProperties().FirstOrDefault(p => p.Name == "Amount" || p.Name == "Count" || p.Name == "QuantityChange");
        if (property != null && property.CanWrite)
        {
            int currentValue = (int)property.GetValue(outcome);
            property.SetValue(outcome, currentValue + amount); // Here you might want to clamp to certain bounds
        }
    }

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        return requirements.All(req => req.IsSatisfied(gameState.Player));
    }
}
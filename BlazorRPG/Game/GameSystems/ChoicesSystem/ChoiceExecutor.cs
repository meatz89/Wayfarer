public class ChoiceExecutor
{
    private readonly GameState gameState;

    public ChoiceExecutor(GameState gameState)
    {
        this.gameState = gameState;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        // First verify all requirements are met
        if (!AreRequirementsMet(choice.ModifiedRequirements))
        {
            return;
        }

        // Now we need to execute everything in the correct order:

        // 1. Process all energy-related changes first
        // This includes both the base energy cost and any energy modifications
        if (!ProcessEnergyChanges(choice))
        {
            return;
        }

        // 4. Process all value changes, including:
        // - Base value changes
        // - Value modifications
        // - Value conversions
        // - Partial value conversions
        // - Value bonuses
        ProcessAllValueChanges(choice);
    }


    private bool ProcessEnergyChanges(EncounterChoice choice)
    {
        // First, calculate total energy costs for each energy type
        Dictionary<EnergyTypes, int> totalEnergyCosts = new();

        // Add base energy cost
        totalEnergyCosts[choice.EnergyType] = choice.ModifiedEnergyCost;

        // Add costs from modifications
        foreach (ChoiceModification mod in choice.Modifications
            .Where(m => m.Type == ModificationType.EnergyCost && m.EnergyChange != null))
        {
            EnergyTypes energyType = mod.EnergyChange.EnergyType;
            int modificationCost = mod.EnergyChange.NewValue - mod.EnergyChange.OriginalValue;

            if (!totalEnergyCosts.ContainsKey(energyType))
            {
                totalEnergyCosts[energyType] = 0;
            }
            totalEnergyCosts[energyType] += modificationCost;
        }

        // Verify we can pay all energy costs
        foreach (KeyValuePair<EnergyTypes, int> energyCost in totalEnergyCosts)
        {
            if (!gameState.Player.CanPayEnergy(energyCost.Key, energyCost.Value))
            {
                return false;
            }
        }

        // If we can pay all costs, apply them
        foreach (KeyValuePair<EnergyTypes, int> energyCost in totalEnergyCosts)
        {
            gameState.Player.ModifyEnergy(energyCost.Key, -energyCost.Value);
        }

        return true;
    }

    private bool ProcessAllValueChanges(EncounterChoice choice)
    {
        // First process direct value changes
        foreach (ValueChange change in choice.ModifiedValueChanges)
        {
            ProcessValueChange(change);
        }

        // Then process value conversions
        foreach (ChoiceModification mod in choice.Modifications
            .Where(m => m.Type == ModificationType.ValueConversion))
        {
            if (mod.ValueConversion != null)
            {
                ProcessValueConversion(mod.ValueConversion);
            }
        }

        // Finally process value bonuses
        foreach (ChoiceModification mod in choice.Modifications
            .Where(m => m.Type == ModificationType.ValueChange))
        {
            if (mod.ValueChange != null)
            {
                ProcessValueChangeModification(mod.ValueChange);
            }
        }

        return true;
    }

    private void ProcessValueChange(ValueChange change)
    {
        gameState.Actions.CurrentEncounter.ModifyValue(
            change.ValueType,
            change.Change);
    }

    private void ProcessValueConversion(ValueConversionModification conversion)
    {
        // First reduce the source value
        gameState.Actions.CurrentEncounter.ModifyValue(
            conversion.ValueType,
            -conversion.ConversionAmount);

        // Then increase the target value
        if (conversion.TargetValueType.HasValue)
        {
            gameState.Actions.CurrentEncounter.ModifyValue(
                conversion.TargetValueType.Value,
                conversion.ConversionAmount);
        }
    }

    private void ProcessValueChangeModification(ValueChangeModification change)
    {
        // Apply the difference between original and target values
        int difference = change.TargetValue - change.OriginalValue;
        gameState.Actions.CurrentEncounter.ModifyValue(
            change.ValueType,
            difference);
    }


    private void ExecuteValueChangesWithModifications(EncounterChoice choice)
    {
        // Group all value changes by their ValueType
        Dictionary<ValueTypes, ValueChangeDetail> valueDetails = new();

        // First, process base value changes
        foreach (ValueChange baseChange in choice.BaseValueChanges)
        {
            valueDetails[baseChange.ValueType] = new ValueChangeDetail
            {
                ValueType = baseChange.ValueType,
                BaseChange = baseChange.Change
            };
        }

        // Then process each modification in order
        foreach (ChoiceModification modification in choice.Modifications)
        {
            switch (modification.Type)
            {
                case ModificationType.ValueChange:
                    ProcessValueChange(valueDetails, modification);
                    break;
                case ModificationType.ValueConversion:
                    ProcessValueConversion(valueDetails, modification);
                    break;
            }
        }

        // Finally, apply the consolidated changes to the game state
        foreach (ValueChangeDetail detail in valueDetails.Values)
        {
            // Only apply if there was any actual change
            if (detail.BaseChange != 0 || detail.Transformations.Any())
            {
                int totalChange = detail.BaseChange + detail.FinalChange;
                gameState.Actions.CurrentEncounter.ModifyValue(detail.ValueType, totalChange);
            }
        }
    }

    private void ProcessValueChange(Dictionary<ValueTypes, ValueChangeDetail> details, ChoiceModification modification)
    {
        if (modification.ValueChange == null) return;

        ValueChangeDetail detail = GetOrCreateDetail(details, modification.ValueChange.ValueType);

        detail.Transformations.Add(new LocationArchetypeTransformation
        {
            Source = GetSourceArchetype(modification),
            Amount = modification.ValueChange.ConversionAmount,
            Explanation = modification.Effect
        });
    }

    private void ProcessValueConversion(Dictionary<ValueTypes, ValueChangeDetail> details, ChoiceModification modification)
    {
        if (modification.ValueConversion == null) return;

        // Handle source value reduction
        ValueChangeDetail sourceDetail = GetOrCreateDetail(details, modification.ValueConversion.ValueType);
        sourceDetail.Transformations.Add(new LocationArchetypeTransformation
        {
            Source = GetSourceArchetype(modification),
            Amount = -modification.ValueConversion.ConversionAmount,
            Explanation = $"Converted to {modification.ValueConversion.TargetValueType}"
        });

        // Handle target value increase
        if (modification.ValueConversion.TargetValueType.HasValue)
        {
            ValueChangeDetail targetDetail = GetOrCreateDetail(details, modification.ValueConversion.TargetValueType.Value);
            targetDetail.Transformations.Add(new LocationArchetypeTransformation
            {
                Source = GetSourceArchetype(modification),
                Amount = modification.ValueConversion.ConversionAmount,
                Explanation = $"Converted from {modification.ValueConversion.ValueType}"
            });
        }
    }

    private ValueChangeDetail GetOrCreateDetail(Dictionary<ValueTypes, ValueChangeDetail> details, ValueTypes valueType)
    {
        if (!details.ContainsKey(valueType))
        {
            details[valueType] = new ValueChangeDetail
            {
                ValueType = valueType,
                BaseChange = 0
            };
        }
        return details[valueType];
    }

    private LocationArchetypes GetSourceArchetype(ChoiceModification modification)
    {
        // Convert the modification source to an appropriate archetype
        // This might need to be expanded based on your actual source types
        return modification.Source switch
        {
            ModificationSource.LocationProperty => LocationArchetypes.Market, // Example mapping
            ModificationSource.LocationType => LocationArchetypes.Tavern,     // Example mapping
            _ => LocationArchetypes.Market // Default mapping
        };
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

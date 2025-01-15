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
            return;

        // Execute changes in correct order:

        // 1. Process energy changes first
        if (!ProcessEnergyChanges(choice))
            return;

        // 2. Process any permanent resource costs
        ExecuteCosts(choice.ModifiedCosts);

        // 3. Process all value changes with their modifications
        ExecuteValueChangesWithModifications(choice);

        // 4. Apply any rewards after the action is complete
        ExecuteRewards(choice.ModifiedRewards);
    }

    private bool ProcessEnergyChanges(EncounterChoice choice)
    {
        Dictionary<EnergyTypes, int> totalEnergyCosts = new();

        // 1. Get base energy cost
        totalEnergyCosts[choice.EnergyType] = choice.ModifiedEnergyCost;

        // 2. Apply pressure modifier
        int pressureLevel = gameState.Actions.CurrentEncounter.Context.CurrentValues.Pressure;
        if (pressureLevel >= 6)
        {
            int pressureModifier = pressureLevel - 5;
            totalEnergyCosts[choice.EnergyType] += pressureModifier;
        }

        // 3. Add costs from any modifications
        foreach (ChoiceModification mod in choice.Modifications
            .Where(m => m.Type == ModificationType.EnergyCost && m.EnergyChange != null))
        {
            EnergyTypes energyType = mod.EnergyChange.EnergyType;
            int modificationCost = mod.EnergyChange.NewValue - mod.EnergyChange.OriginalValue;

            if (!totalEnergyCosts.ContainsKey(energyType))
                totalEnergyCosts[energyType] = 0;

            totalEnergyCosts[energyType] += modificationCost;
        }

        // 4. Verify we can pay all costs
        foreach (var energyCost in totalEnergyCosts)
        {
            if (!gameState.Player.CanPayEnergy(energyCost.Key, energyCost.Value))
                return false;
        }

        // 5. Apply all energy costs
        foreach (var energyCost in totalEnergyCosts)
            gameState.Player.ModifyEnergy(energyCost.Key, -energyCost.Value);

        return true;
    }

    private void ExecuteValueChangesWithModifications(EncounterChoice choice)
    {
        // Group all value changes by their ValueType
        Dictionary<ValueTypes, ValueChangeDetail> valueDetails = new();

        // 1. Process base value changes first
        foreach (ValueChange baseChange in choice.BaseEncounterValueChanges)
        {
            valueDetails[baseChange.ValueType] = new ValueChangeDetail
            {
                ValueType = baseChange.ValueType,
                BaseChange = baseChange.Change
            };
        }

        // 2. Process each modification in order
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

        // 3. Apply consolidated changes to game state
        foreach (ValueChangeDetail detail in valueDetails.Values)
        {
            if (detail.BaseChange != 0 || detail.Transformations.Any())
            {
                int totalChange = detail.BaseChange + detail.FinalChange;
                gameState.Actions.CurrentEncounter.ModifyValue(detail.ValueType, totalChange);
            }
        }
    }

    private void ProcessValueChange(Dictionary<ValueTypes, ValueChangeDetail> details, ChoiceModification modification)
    {
        if (modification.ValueChange == null)
            return;

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
        if (modification.ValueConversion == null)
            return;

        // Handle source value reduction
        ValueChangeDetail sourceDetail = GetOrCreateDetail(details, modification.ValueConversion.ValueType);
        sourceDetail.Transformations.Add(new LocationArchetypeTransformation
        {
            Source = GetSourceArchetype(modification),
            Amount = -modification.ValueConversion.ConversionAmount,
            Explanation = $"Converted to {modification.ValueConversion.TargetValueType}"
        });

        // Handle target value increase if there is one
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
        return modification.Source switch
        {
            ModificationSource.LocationProperty => LocationArchetypes.Market,
            ModificationSource.LocationType => LocationArchetypes.Tavern,
            _ => LocationArchetypes.Market
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

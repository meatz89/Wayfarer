public class ChoiceCalculator
{
    private readonly List<LocationPropertyChoiceEffect> locationPropertyEffects;

    public ChoiceCalculator(List<LocationPropertyChoiceEffect> locationPropertyEffects)
    {
        this.locationPropertyEffects = locationPropertyEffects;
    }

    public void CalculateChoice(EncounterChoice choice, EncounterContext context)
    {
        // 1. Reset Modifications
        choice.Modifications.Clear();

        // 2. Use Base Values Directly
        choice.ModifiedValueChanges = new List<ValueChange>(choice.BaseValueChanges);
        choice.ModifiedRequirements = new List<Requirement>(choice.Requirements);
        choice.ModifiedCosts = new List<Outcome>(choice.BaseCosts);
        choice.ModifiedRewards = new List<Outcome>(choice.BaseRewards);

        // 3. Apply Effects
        foreach (LocationPropertyChoiceEffect effect in locationPropertyEffects)
        {
            ApplyEffect(choice, context, effect);
        }
    }

    private void ApplyEffect(EncounterChoice choice, EncounterContext context, LocationPropertyChoiceEffect effect)
    {
        ChoiceModification modification = new()
        {
            Source = ModificationSource.LocationProperty,
            SourceDetails = effect.LocationProperty.GetPropertyType().ToString(),
            Effect = effect.RuleDescription
        };

        // Set the appropriate modification type and data based on the effect
        switch (effect.ValueTypeEffect)
        {
            case ValueModification mod:
                modification.Type = ModificationType.ValueChange;
                modification.ValueChange = ApplyValueModification(choice, mod);
                break;
            case ValueConversion conv:
                modification.Type = ModificationType.ValueChange;
                modification.ValueChange = ApplyValueConversion(choice, conv);
                break;
            case PartialValueConversion pConv:
                modification.Type = ModificationType.ValueChange;
                modification.ValueChange = ApplyPartialValueConversion(choice, pConv);
                break;
            case EnergyModification eMod:
                modification.Type = ModificationType.Requirement;
                modification.ValueChange = ApplyEnergyModification(choice, eMod);
                break;
            case ValueBonus bonus:
                modification.Type = ModificationType.ValueChange;
                modification.ValueChange = ApplyValueBonus(choice, bonus);
                break;
        }

        // Only add the modification if something actually changed
        if (modification.ValueChange != null ||
            modification.Requirement != null ||
            modification.Cost != null ||
            modification.Reward != null)
        {
            choice.Modifications.Add(modification);
        }
    }

    private ValueChangeModification ApplyValueBonus(EncounterChoice choice, ValueBonus bonus)
    {
        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == bonus.ValueType);

        // If there's nothing to convert, no modification happened
        if (sourceChange == null)
            return null;

        int convertedAmount = sourceChange.Change;

        // Record both the removal of source value and addition of target value
        ValueChangeModification modification = new()
        {
            ValueType = bonus.ValueType,
            OriginalSourceValue = convertedAmount,
            NewSourceValue = 0,
            TargetValueType = bonus.ValueType,
            OriginalTargetValue = 0,
            NewTargetValue = convertedAmount,
            ConversionAmount = convertedAmount
        };

        // Perform the actual conversion
        choice.ModifiedValueChanges.Remove(sourceChange);
        choice.ModifiedValueChanges.Add(new ValueChange(bonus.ValueType, convertedAmount));

        return modification;
    }


    private ValueChangeModification ApplyValueModification(EncounterChoice choice, ValueModification modf)
    {
        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == modf.ValueType);

        // If there's nothing to convert, no modification happened
        if (sourceChange == null)
            return null;

        int convertedAmount = sourceChange.Change;

        // Record both the removal of source value and addition of target value
        ValueChangeModification modification = new()
        {
            ValueType = modf.ValueType,  
            OriginalSourceValue = convertedAmount,
            NewSourceValue = 0,
            TargetValueType = modf.ValueType,
            OriginalTargetValue = 0,
            NewTargetValue = convertedAmount,
            ConversionAmount = convertedAmount  
        };

        // Perform the actual conversion
        choice.ModifiedValueChanges.Remove(sourceChange);
        choice.ModifiedValueChanges.Add(new ValueChange(modf.ValueType, convertedAmount));

        return modification;
    }

    private ValueChangeModification ApplyValueConversion(EncounterChoice choice, ValueConversion conv)
    {
        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == conv.SourceValueType);

        // If there's nothing to convert, no modification happened
        if (sourceChange == null)
            return null;

        int convertedAmount = sourceChange.Change;

        // Record both the removal of source value and addition of target value
        ValueChangeModification modification = new()
        {
            ValueType = conv.SourceValueType,  // We track the source type since it's being converted
            OriginalSourceValue = convertedAmount,
            NewSourceValue = 0,
            TargetValueType = conv.TargetValueType,
            OriginalTargetValue = 0,
            NewTargetValue = convertedAmount,
            ConversionAmount = convertedAmount  // Full amount was converted
        };

        // Perform the actual conversion
        choice.ModifiedValueChanges.Remove(sourceChange);
        choice.ModifiedValueChanges.Add(new ValueChange(conv.TargetValueType, convertedAmount));

        return modification;
    }

    private ValueChangeModification ApplyPartialValueConversion(EncounterChoice choice, PartialValueConversion pConv)
    {
        // Early exit if this conversion doesn't apply to this choice type
        if (choice.Archetype != pConv.TargetArchetype)
            return null;

        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == pConv.SourceValueType);

        // If there's not enough value to convert, no modification happened
        if (sourceChange == null || sourceChange.Change < pConv.ConversionAmount)
            return null;

        // Find existing target value change if it exists
        ValueChange? targetChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == pConv.TargetValueType);

        int originalSourceValue = sourceChange.Change;
        int originalTargetValue = targetChange?.Change ?? 0;

        // Record both the partial reduction of source and increase of target
        ValueChangeModification modification = new()
        {
            ValueType = pConv.SourceValueType,  // We track the source type since it's being partially converted
            OriginalSourceValue = originalSourceValue,
            NewSourceValue = originalSourceValue - pConv.ConversionAmount,
            TargetValueType = pConv.TargetValueType,
            OriginalTargetValue = originalTargetValue,
            NewTargetValue = originalTargetValue + pConv.ConversionAmount,
            ConversionAmount = pConv.ConversionAmount  // Only part of the value was converted
        };

        // Perform the actual conversion
        sourceChange.Change -= pConv.ConversionAmount;

        if (targetChange != null)
        {
            targetChange.Change += pConv.ConversionAmount;
        }
        else
        {
            choice.ModifiedValueChanges.Add(new ValueChange(pConv.TargetValueType, pConv.ConversionAmount));
        }

        return modification;
    }

    private ValueChangeModification ApplyEnergyModification(EncounterChoice choice, EnergyModification eMod)
    {
        if (choice.Archetype != eMod.TargetArchetype)
            return null;

        EnergyRequirement? energyRequirement = choice.ModifiedRequirements
            .OfType<EnergyRequirement>()
            .FirstOrDefault();

        if (energyRequirement != null)
        {
            int originalAmount = energyRequirement.Amount;
            energyRequirement.Amount = Math.Max(0, energyRequirement.Amount + eMod.EnergyCostModifier);

            EnergyChangeModification modification = new()
            {
                EnergyType = EnergyTypes.Physical,
                ChoiceArchetype = eMod.TargetArchetype,
                OriginalValue = originalAmount,
                NewValue = energyRequirement.Amount
            };
        }

        return null;
    }


}
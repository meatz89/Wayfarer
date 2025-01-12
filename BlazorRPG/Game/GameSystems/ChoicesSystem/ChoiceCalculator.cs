public class ChoiceCalculator
{
    private readonly List<LocationPropertyChoiceEffect> locationPropertyEffects;

    public ChoiceCalculator(List<LocationPropertyChoiceEffect> locationEffects)
    {
        this.locationPropertyEffects = locationEffects;
    }

    public void CalculateChoice(EncounterChoice choice, EncounterContext context)
    {
        // 1. Reset Modifications
        choice.Modifications.Clear();

        // 2. Create fresh ValueChange objects for modifications
        choice.ModifiedValueChanges = choice.BaseValueChanges
            .Select(vc => new ValueChange(vc.ValueType, vc.Change))
            .ToList();

        choice.ModifiedRequirements = new List<Requirement>(choice.BaseRequirements);
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
                modification.Type = ModificationType.ValueConversion;
                modification.ValueConversion = ApplyValueConversion(choice, conv);
                break;
            case PartialValueConversion pConv:
                modification.Type = ModificationType.ValueConversion;
                modification.ValueConversion = ApplyPartialValueConversion(choice, pConv);
                break;
            case EnergyModification eMod:
                modification.Type = ModificationType.Requirement;
                modification.EnergyChange = ApplyEnergyModification(choice, eMod);
                break;
            case ValueBonus bonus:
                modification.Type = ModificationType.ValueChange;
                modification.ValueChange = ApplyValueBonus(choice, bonus);
                break;
        }

        // Only add the modification if something actually changed
        if (modification.ValueChange != null ||
            modification.ValueConversion != null ||
            modification.EnergyChange != null ||
            modification.Requirement != null ||
            modification.Cost != null ||
            modification.Reward != null)
        {
            choice.Modifications.Add(modification);
        }
    }

    private ValueChangeModification ApplyValueModification(EncounterChoice choice, ValueModification mod)
    {
        ValueChange? targetChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == mod.ValueType);

        if (targetChange == null) return null;

        // Get the true base value from BaseValueChanges (which stays untouched)
        int originalValue = choice.BaseValueChanges
            .First(vc => vc.ValueType == mod.ValueType).Change;

        ValueChangeModification modification = new()
        {
            ValueType = mod.ValueType,
            OriginalValue = originalValue,
            TargetValue = targetChange.Change + mod.ModifierAmount,
            ConversionAmount = mod.ModifierAmount,
            ValueChangeSourceType = ValueChangeSourceType.LocationProperty
        };

        targetChange.Change += mod.ModifierAmount;
        return modification;
    }

    private ValueConversionModification ApplyValueConversion(EncounterChoice choice, ValueConversion conv)
    {
        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == conv.SourceValueType);

        if (sourceChange == null) return null;

        ValueChange? targetChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == conv.TargetValueType);

        // Get original values from BaseValueChanges to ensure we're using unmodified values
        int originalSourceValue = choice.BaseValueChanges
            .First(vc => vc.ValueType == conv.SourceValueType).Change;
        int originalTargetValue = choice.BaseValueChanges
            .FirstOrDefault(vc => vc.ValueType == conv.TargetValueType)?.Change ?? 0;

        int convertedAmount = sourceChange.Change;
        sourceChange.Change = 0;

        if (targetChange != null)
        {
            targetChange.Change += convertedAmount;
        }
        else
        {
            choice.ModifiedValueChanges.Add(new ValueChange(conv.TargetValueType, convertedAmount));
        }

        return new ValueConversionModification
        {
            ValueType = conv.SourceValueType,
            OriginalValue = originalSourceValue,
            SourceTargetValue = 0,
            TargetValueType = conv.TargetValueType,
            OriginalTargetValue = originalTargetValue,
            NewTargetValue = originalTargetValue + convertedAmount,
            ConversionAmount = convertedAmount,
            ValueChangeSourceType = ValueChangeSourceType.LocationProperty
        };
    }

    private ValueChangeModification ApplyValueBonus(EncounterChoice choice, ValueBonus bonus)
    {
        ValueChange? targetChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == bonus.ValueType);

        if (targetChange == null) return null;

        // Get original value from BaseValueChanges to ensure we're using unmodified value
        int originalValue = choice.BaseValueChanges
            .First(vc => vc.ValueType == bonus.ValueType).Change;

        ValueChangeModification modification = new()
        {
            ValueType = bonus.ValueType,
            OriginalValue = originalValue,
            TargetValue = targetChange.Change + bonus.BonusAmount,
            ConversionAmount = bonus.BonusAmount,
            ValueChangeSourceType = ValueChangeSourceType.LocationProperty
        };

        targetChange.Change += bonus.BonusAmount;
        return modification;
    }

    private ValueConversionModification ApplyPartialValueConversion(EncounterChoice choice, PartialValueConversion pConv)
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

        // Create a ValueConversionModification record
        ValueConversionModification modification = new()
        {
            ValueType = pConv.SourceValueType,
            OriginalValue = originalSourceValue,
            SourceTargetValue = sourceChange.Change, // New source value after partial conversion
            TargetValueType = pConv.TargetValueType,
            OriginalTargetValue = originalTargetValue,
            NewTargetValue = originalTargetValue + pConv.ConversionAmount, // New target value after partial conversion
            ConversionAmount = pConv.ConversionAmount,
            ValueChangeSourceType = ValueChangeSourceType.LocationProperty
        };

        return modification;
    }

    private EnergyChangeModification ApplyEnergyModification(EncounterChoice choice, EnergyModification eMod)
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

            // Map the choice archetype to the correct energy type
            EnergyTypes energyType = choice.Archetype switch
            {
                ChoiceArchetypes.Physical => EnergyTypes.Physical,
                ChoiceArchetypes.Focus => EnergyTypes.Focus,
                ChoiceArchetypes.Social => EnergyTypes.Social,
                _ => throw new ArgumentOutOfRangeException(nameof(choice.Archetype),
                    "All choice archetypes must map to an energy type")
            };

            EnergyChangeModification modification = new()
            {
                EnergyType = energyType,  // Now using the correct energy type
                ChoiceArchetype = eMod.TargetArchetype,
                OriginalValue = originalAmount,
                NewValue = energyRequirement.Amount
            };
            return modification;
        }

        return null;
    }
}
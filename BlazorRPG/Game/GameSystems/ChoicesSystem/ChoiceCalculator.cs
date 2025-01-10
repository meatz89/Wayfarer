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
        switch (effect.ValueTypeEffect)
        {
            case ValueModification mod:
                ApplyValueModification(choice, mod);
                break;
            case ValueConversion conv:
                ApplyValueConversion(choice, conv);
                break;
            case PartialValueConversion pConv:
                ApplyPartialValueConversion(choice, pConv);
                break;
            case EnergyModification eMod:
                ApplyEnergyModification(choice, eMod);
                break;
            case ValueBonus bonus:
                ApplyValueBonus(choice, bonus);
                break;
        }

        // Add modification record
        choice.Modifications.Add(new ChoiceModification
        {
            Source = ModificationSource.LocationProperty,
            Effect = effect.RuleDescription,
            SourceDetails = effect.LocationProperty.GetPropertyType().ToString()
        });
    }

    private void ApplyValueModification(EncounterChoice choice, ValueModification mod)
    {
        ValueChange? valueChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == mod.ValueType);

        if (valueChange != null)
        {
            valueChange.Change += mod.ModifierAmount;
        }
        else
        {
            choice.ModifiedValueChanges.Add(new ValueChange(mod.ValueType, mod.ModifierAmount));
        }
    }

    private void ApplyValueConversion(EncounterChoice choice, ValueConversion conv)
    {
        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == conv.SourceValueType);

        if (sourceChange != null)
        {
            int amount = sourceChange.Change;
            choice.ModifiedValueChanges.Remove(sourceChange);
            choice.ModifiedValueChanges.Add(new ValueChange(conv.TargetValueType, amount));
        }
    }

    private void ApplyPartialValueConversion(EncounterChoice choice, PartialValueConversion pConv)
    {
        if (choice.Archetype != pConv.TargetArchetype) return;

        ValueChange? sourceChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == pConv.SourceValueType);

        if (sourceChange != null && sourceChange.Change >= pConv.ConversionAmount)
        {
            sourceChange.Change -= pConv.ConversionAmount;

            ValueChange? targetChange = choice.ModifiedValueChanges
                .FirstOrDefault(vc => vc.ValueType == pConv.TargetValueType);

            if (targetChange != null)
            {
                targetChange.Change += pConv.ConversionAmount;
            }
            else
            {
                choice.ModifiedValueChanges.Add(new ValueChange(pConv.TargetValueType, pConv.ConversionAmount));
            }
        }
    }

    private void ApplyEnergyModification(EncounterChoice choice, EnergyModification eMod)
    {
        if (choice.Archetype != eMod.TargetArchetype) return;

        EnergyRequirement? energyRequirement = choice.ModifiedRequirements
            .OfType<EnergyRequirement>()
            .FirstOrDefault();

        if (energyRequirement != null)
        {
            energyRequirement.Amount = Math.Max(0, energyRequirement.Amount + eMod.EnergyCostModifier);
        }
    }

    private void ApplyValueBonus(EncounterChoice choice, ValueBonus bonus)
    {
        if (choice.Archetype != bonus.ChoiceArchetype) return;

        ValueChange? valueChange = choice.ModifiedValueChanges
            .FirstOrDefault(vc => vc.ValueType == bonus.ValueType);

        if (valueChange != null)
        {
            valueChange.Change += bonus.BonusAmount;
        }
        else
        {
            choice.ModifiedValueChanges.Add(new ValueChange(bonus.ValueType, bonus.BonusAmount));
        }
    }

}
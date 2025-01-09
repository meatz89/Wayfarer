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

        // 3. Apply Modifiers (excluding transformations)
        ApplyLocationPropertyModifiers(choice, context);
        //ApplyPlayerSkillModifiers(choice, context);

        // 4. Apply Transformations
        ApplyValueTransformations(choice, context);

        // 5. Apply Energy Cost Reductions
        ApplyEnergyCostReductions(choice, context);
    }

    private void ApplyLocationPropertyModifiers(EncounterChoice choice, EncounterContext context)
    {
        foreach (var effect in locationPropertyEffects)
        {
            // Check if the effect's LocationProperty matches the context
            if (IsLocationPropertyMatch(effect.LocationProperty, context))
            {
                // Apply the effect based on the ValueTransformation type
                if (effect.ValueTypeEffect is ChangeValueTransformation changeTransformation)
                {
                    // Apply change transformation
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationProperty,
                        Type = ModificationType.ValueChange,
                        Effect = $"Changed {changeTransformation.ValueType} by {changeTransformation.ChangeInValue} due to {effect.LocationProperty.GetPropertyType()}",
                        SourceDetails = $"Changed {changeTransformation.ValueType} by {changeTransformation.ChangeInValue} due to {effect.LocationProperty.GetPropertyType()}",
                        ValueChange = new ValueChangeModification
                        {
                            ValueType = changeTransformation.ValueType,
                            Amount = changeTransformation.ChangeInValue,
                            ValueTransformation = changeTransformation
                        }
                    });
                }
                else if (effect.ValueTypeEffect is ConvertValueTransformation convertTransformation)
                {
                    // Apply convert transformation
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationProperty,
                        Type = ModificationType.ValueChange,
                        Effect = $"Converted {convertTransformation.SourceValueType} to {convertTransformation.TargetValueType} due to {effect.LocationProperty.GetPropertyType()}",
                        SourceDetails = $"Converted {convertTransformation.SourceValueType} to {convertTransformation.TargetValueType} due to {effect.LocationProperty.GetPropertyType()}",
                        ValueChange = new ValueChangeModification
                        {
                            ValueType = convertTransformation.SourceValueType, // Use SourceValueType for finding the ValueChange
                            Amount = 0, // Amount is not directly used in conversion, set to 0 or find a suitable value from context
                            ValueTransformation = convertTransformation
                        }
                    });
                }
                else if (effect.ValueTypeEffect is CancelValueTransformation cancelTransformation)
                {
                    // Apply cancel transformation
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationProperty,
                        Type = ModificationType.ValueChange,
                        Effect = $"Canceled {cancelTransformation.ValueType} due to {effect.LocationProperty.GetPropertyType()}",
                        SourceDetails = $"Canceled {cancelTransformation.ValueType} due to {effect.LocationProperty.GetPropertyType()}",
                        ValueChange = new ValueChangeModification
                        {
                            ValueType = cancelTransformation.ValueType,
                            Amount = 0, // Amount is not directly used in cancellation, set to 0
                            ValueTransformation = cancelTransformation
                        }
                    });
                }
                else if (effect.ValueTypeEffect is EnergyValueTransformation energyTransformation)
                {
                    // Apply energy transformation
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationProperty,
                        Type = ModificationType.EnergyCost,
                        Effect = $"Changed {energyTransformation.EnergyType} Energy cost by {energyTransformation.ChangeInValue} due to {effect.LocationProperty.GetPropertyType()}",
                        SourceDetails = $"Changed {energyTransformation.EnergyType} Energy cost by {energyTransformation.ChangeInValue} due to {effect.LocationProperty.GetPropertyType()}",
                        Requirement = new RequirementModification
                        {
                            RequirementType = "Energy", // Assuming "Energy" is the relevant requirement type
                            Amount = energyTransformation.ChangeInValue,
                            // Additional fields for energy modifications can be added here
                        }
                    });
                }
            }
        }
    }

    private bool IsLocationPropertyMatch(LocationPropertyTypeValue locationProperty, EncounterContext context)
    {
        switch (locationProperty.GetPropertyType())
        {
            case LocationPropertyTypes.Scale:
                return context.LocationProperties.Scale == ((ScaleValue)locationProperty).ScaleVariation;
            case LocationPropertyTypes.Exposure:
                return context.LocationProperties.Exposure == ((ExposureValue)locationProperty).ExposureCondition;
            case LocationPropertyTypes.Legality:
                return context.LocationProperties.Legality == ((LegalityValue)locationProperty).Legality;
            case LocationPropertyTypes.Pressure:
                return context.LocationProperties.Pressure == ((PressureValue)locationProperty).PressureState;
            case LocationPropertyTypes.Complexity:
                return context.LocationProperties.Complexity == ((ComplexityValue)locationProperty).Complexity;
            case LocationPropertyTypes.Resource:
                return context.LocationProperties.Resource == ((ResourceValue)locationProperty).Resource;
            case LocationPropertyTypes.CrowdLevel:
                return context.LocationProperties.CrowdLevel == ((CrowdLevelValue)locationProperty).CrowdLevel;
            case LocationPropertyTypes.ReputationType:
                return context.LocationProperties.ReputationType == ((LocationReputationTypeValue)locationProperty).ReputationType;
            default:
                return false;
        }
    }

    private void ApplyPlayerSkillModifiers(EncounterChoice choice, EncounterContext context)
    {
        // Example: Modify ValueChanges based on relevant skill
        int skillVsDifficulty = context.PlayerState.GetSkillLevel(choice.ChoiceRelevantSkill) - context.LocationDifficulty;

        if (skillVsDifficulty > 0)
        {
            // Only give a bonus to outcome if it is the right archetype
            if (choice.Archetype == ChoiceArchetypes.Physical)
            {
                choice.Modifications.Add(new ChoiceModification
                {
                    Source = ModificationSource.PlayerSkill,
                    Type = ModificationType.ValueChange,
                    Effect = $"Increased Outcome by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill",
                    SourceDetails = $"Increased Outcome by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill", // Add SourceDetails here
                    ValueChange = new ValueChangeModification
                    {
                        ValueType = ValueTypes.Outcome,
                        Amount = skillVsDifficulty,
                        ValueTransformation = new ChangeValueTransformation()
                    }
                });
            }
            else if (choice.Archetype == ChoiceArchetypes.Focus)
            {
                choice.Modifications.Add(new ChoiceModification
                {
                    Source = ModificationSource.PlayerSkill,
                    Type = ModificationType.ValueChange,
                    Effect = $"Increased Insight by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill",
                    SourceDetails = $"Increased Insight by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill",
                    ValueChange = new ValueChangeModification
                    {
                        ValueType = ValueTypes.Insight,
                        Amount = skillVsDifficulty,
                        ValueTransformation = new ChangeValueTransformation()
                    }
                });
            }
            else if (choice.Archetype == ChoiceArchetypes.Social)
            {
                choice.Modifications.Add(new ChoiceModification
                {
                    Source = ModificationSource.PlayerSkill,
                    Type = ModificationType.ValueChange,
                    Effect = $"Increased Resonance by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill",
                    SourceDetails = $"Increased Resonance by {skillVsDifficulty} due to {choice.ChoiceRelevantSkill} skill",
                    ValueChange = new ValueChangeModification
                    {
                        ValueType = ValueTypes.Resonance,
                        Amount = skillVsDifficulty,
                        ValueTransformation = new ChangeValueTransformation()
                    }
                });
            }
        }
    }

    private void ApplyValueTransformations(EncounterChoice choice, EncounterContext context)
    {
        // Create a list to track modifications made in this method
        List<ChoiceModification> modificationsMade = new List<ChoiceModification>();

        foreach (ChoiceModification modification in choice.Modifications)
        {
            if (modification.Type == ModificationType.ValueChange && modification.ValueChange != null)
            {
                if (modification.ValueChange.ValueTransformation != null)
                {
                    // Find the ValueChange in ModifiedValueChanges that matches the current modification's ValueType
                    // For Convert, we use SourceValueType
                    ValueChange? valueChangeToModify;
                    if (modification.ValueChange.ValueTransformation is ConvertValueTransformation)
                    {
                        valueChangeToModify = choice.ModifiedValueChanges.FirstOrDefault(vc => vc.ValueType == modification.ValueChange.ValueType);
                    }
                    else
                    {
                        valueChangeToModify = choice.ModifiedValueChanges.FirstOrDefault(vc => vc.ValueType == modification.ValueChange.ValueType);
                    }

                    if (valueChangeToModify != null)
                    {
                        switch (modification.ValueChange.ValueTransformation)
                        {
                            case ConvertValueTransformation convertTransformation:
                                ConvertValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify, convertTransformation);
                                modificationsMade.Add(modification);
                                break;
                            case ChangeValueTransformation changeTransformation:
                                if (changeTransformation.ChangeInValue > 0)
                                {
                                    IncreaseValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify, changeTransformation);
                                    modificationsMade.Add(modification);
                                }
                                else if (changeTransformation.ChangeInValue < 0)
                                {
                                    ReduceValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify, changeTransformation);
                                    modificationsMade.Add(modification);
                                }
                                break;
                            case CancelValueTransformation cancelTransformation:
                                CancelValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify, cancelTransformation);
                                modificationsMade.Add(modification);
                                break;
                        }
                    }
                }
            }
        }

        // Remove the modifications that were handled in this method
        foreach (ChoiceModification mod in modificationsMade)
        {
            choice.Modifications.Remove(mod);
        }
    }

    private void ConvertValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify, ConvertValueTransformation convertTransformation)
    {
        // Remove the source value change
        modifiedValueChanges.Remove(valueChangeToModify);

        // Add a new value change with the target type and the amount from the source
        modifiedValueChanges.Add(new ValueChange(convertTransformation.TargetValueType, valueChangeToModify.Change));

        // Update the modification effect
        choiceModification.Effect = $"Converted {valueChangeToModify.Change} {convertTransformation.SourceValueType} to {valueChangeToModify.Change} {convertTransformation.TargetValueType}";
    }

    private void ReduceValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify, ChangeValueTransformation changeTransformation)
    {
        // Ensure we don't reduce below zero
        int reductionAmount = Math.Min(Math.Abs(changeTransformation.ChangeInValue), valueChangeToModify.Change);
        valueChangeToModify.Change -= reductionAmount;

        choiceModification.Effect = $"Reduced {valueChangeToModify.ValueType} by {reductionAmount}";
    }

    private void IncreaseValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify, ChangeValueTransformation changeTransformation)
    {
        valueChangeToModify.Change += changeTransformation.ChangeInValue;

        choiceModification.Effect = $"Increased {valueChangeToModify.ValueType} by {changeTransformation.ChangeInValue}";
    }

    private void CancelValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify, CancelValueTransformation cancelTransformation)
    {
        // Remove the value change if it matches the type to be canceled
        if (valueChangeToModify.ValueType == cancelTransformation.ValueType)
        {
            modifiedValueChanges.Remove(valueChangeToModify);
            choiceModification.Effect = $"Canceled {cancelTransformation.ValueType}";
        }
    }

    private void ApplyEnergyCostReductions(EncounterChoice choice, EncounterContext context)
    {
        foreach (var effect in locationPropertyEffects)
        {
            // Check if the effect's LocationProperty matches the context
            if (IsLocationPropertyMatch(effect.LocationProperty, context))
            {
                // Apply energy cost reduction if the effect is an EnergyValueTransformation
                if (effect.ValueTypeEffect is EnergyValueTransformation energyTransformation)
                {
                    // Iterate through each energy cost requirement in the choice
                    foreach (EnergyRequirement requirement in choice.ModifiedRequirements.OfType<EnergyRequirement>())
                    {
                        if (requirement.EnergyType == energyTransformation.EnergyType)
                        {
                            // Reduce the energy cost requirement, ensuring it doesn't go below 0
                            requirement.Amount = Math.Max(0, requirement.Amount + energyTransformation.ChangeInValue);
                        }
                    }
                }
            }
        }
    }
}
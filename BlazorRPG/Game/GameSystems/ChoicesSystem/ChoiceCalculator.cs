public class ChoiceCalculator
{
    private readonly Dictionary<LocationArchetypes, ArchetypeEffect> locationArchetypeEffects;

    public ChoiceCalculator(Dictionary<LocationArchetypes, ArchetypeEffect> locationArchetypeEffects)
    {
        this.locationArchetypeEffects = locationArchetypeEffects;
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
        ApplyLocationArchetypeModifiers(choice, context);
        ApplyPlayerSkillModifiers(choice, context);

        // 4. Apply Transformations
        ApplyValueTransformations(choice, context);

        // 5. Apply Energy Cost Reductions
        ApplyEnergyCostReductions(choice, context);
    }

    private void ApplyLocationArchetypeModifiers(EncounterChoice choice, EncounterContext context)
    {
        var archetypeEffect = locationArchetypeEffects[context.LocationArchetype];

        // Iterate over each base value change in the choice
        foreach (var valueChange in choice.BaseValueChanges)
        {
            // Check if the archetype effect has any transformations for this value type
            if (archetypeEffect.ValueTransformations.TryGetValue(valueChange.ValueType, out var transformations))
            {
                // Apply each transformation
                foreach (var transformation in transformations)
                {
                    string effect = GetTransformationEffect(transformation); // Get the description of the transformation

                    // Add a modification representing the transformation
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationArchetype,
                        Type = ModificationType.ValueChange,
                        Effect = effect, // Use the effect as a general description
                        SourceDetails = effect, // Use the effect as source details
                        ValueChange = new ValueChangeModification
                        {
                            ValueType = valueChange.ValueType,
                            Amount = valueChange.Change,
                            ValueTransformation = transformation
                        }
                    });
                }
            }
        }

        // Iterate over each energy requirement in the choice
        foreach (var requirement in choice.Requirements.OfType<EnergyRequirement>())
        {
            // Check if the archetype effect has a cost reduction for this energy type
            if (archetypeEffect.EnergyCostReductions.TryGetValue(requirement.EnergyType, out int reductionAmount))
            {
                // Add a modification representing the energy cost reduction
                choice.Modifications.Add(new ChoiceModification
                {
                    Source = ModificationSource.LocationArchetype,
                    Type = ModificationType.EnergyCost,
                    Effect = $"Reduced {requirement.EnergyType} Energy cost by {reductionAmount}",
                    SourceDetails = $"Reduced {requirement.EnergyType} Energy cost by {reductionAmount}",
                    Requirement = new RequirementModification
                    {
                        RequirementType = "Energy",
                        Amount = -reductionAmount
                    }
                });
            }
        }

        // Modify Requirements based on LocationArchetype
        foreach (var requirement in choice.ModifiedRequirements)
        {
            if (requirement is EnergyRequirement energyRequirement)
            {
                if (archetypeEffect.EnergyCostReductions.TryGetValue(energyRequirement.EnergyType, out int reductionAmount))
                {
                    choice.Modifications.Add(new ChoiceModification
                    {
                        Source = ModificationSource.LocationArchetype,
                        Type = ModificationType.EnergyCost,
                        Effect = $"Reduced {energyRequirement.EnergyType} Energy cost by {reductionAmount}",
                        Requirement = new RequirementModification
                        {
                            RequirementType = "Energy",
                            Amount = -reductionAmount
                        }
                    });
                }
            }
        }
    }


    private string GetTransformationEffect(ValueTransformation transformation)
    {
        switch (transformation.TransformationType)
        {
            case TransformationType.Convert:
                return $"Each point of {transformation.SourceValue} converts one point of {transformation.SourceValue} gain into {transformation.TargetValue}";
            case TransformationType.Reduce:
            case TransformationType.ReduceCost:
                return $"Each point of {transformation.SourceValue} reduces one point of {transformation.TargetValue} gain";
            case TransformationType.Increase:
                return $"Each point of {transformation.SourceValue} increases one point of {transformation.TargetValue} gain";
            case TransformationType.Set:
                return $"Each point of {transformation.SourceValue} can be set to {transformation.TargetValue} instead";
            default:
                return string.Empty;
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
                        ValueTransformation = new ValueTransformation { } // Create a dummy ValueTransformation object
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
                        ValueTransformation = new ValueTransformation { }
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
                        ValueTransformation = new ValueTransformation { }
                    }
                });
            }
        }
    }

    private void ApplyValueTransformations(EncounterChoice choice, EncounterContext context)
    {
        // Create a list to track modifications made in this method
        var modificationsMade = new List<ChoiceModification>();

        foreach (var modification in choice.Modifications)
        {
            if (modification.Type == ModificationType.ValueChange && modification.ValueChange != null)
            {
                if (modification.ValueChange.ValueTransformation != null)
                {
                    // Find the ValueChange in ModifiedValueChanges that matches the current modification's ValueType
                    var valueChangeToModify = choice.ModifiedValueChanges.FirstOrDefault(vc => vc.ValueType == modification.ValueChange.ValueType);

                    if (valueChangeToModify != null)
                    {
                        ValueTransformation valueTransformation = modification.ValueChange.ValueTransformation;
                        switch (valueTransformation.TransformationType)
                        {
                            case TransformationType.Convert:
                                ConvertValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify);
                                modificationsMade.Add(modification);
                                break;
                            case TransformationType.Reduce:
                            case TransformationType.ReduceCost:
                                ReduceValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify);
                                modificationsMade.Add(modification);
                                break;
                            case TransformationType.Increase:
                                IncreaseValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify);
                                modificationsMade.Add(modification);
                                break;
                            case TransformationType.Set:
                                SetValueChange(choice, context, choice.ModifiedValueChanges, modification, valueChangeToModify);
                                modificationsMade.Add(modification);
                                break;
                        }
                    }
                }
            }
        }

        // Remove the modifications that were handled in this method
        foreach (var mod in modificationsMade)
        {
            choice.Modifications.Remove(mod);
        }
    }

    private void ConvertValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify)
    {
        // For Convert, we remove the source change and add a new change of the target type
        modifiedValueChanges.Remove(valueChangeToModify);
        modifiedValueChanges.Add(new ValueChange(choiceModification.ValueChange.ValueTransformation.TargetValue, valueChangeToModify.Change));

        choiceModification.Effect = $"Converted {valueChangeToModify.Change} {choiceModification.ValueChange.ValueTransformation.SourceValue} to {valueChangeToModify.Change} {choiceModification.ValueChange.ValueTransformation.TargetValue}";
    }

    private void ReduceValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify)
    {
        // Reduce modifies the value change directly, no new ValueChange is created
        if (choiceModification.ValueChange.Amount < 0)
        {
            // Ensure we don't reduce below zero
            int reductionAmount = Math.Min(Math.Abs(choiceModification.ValueChange.Amount), valueChangeToModify.Change);
            valueChangeToModify.Change -= reductionAmount;

            choiceModification.Effect = $"Reduced {reductionAmount} {choiceModification.ValueChange.ValueTransformation.TargetValue} due to {Math.Abs(choiceModification.ValueChange.Amount)} {choiceModification.ValueChange.ValueTransformation.SourceValue}";
        }
    }

    private void IncreaseValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify)
    {
        // Increase modifies the value change directly
        valueChangeToModify.Change += choiceModification.ValueChange.Amount;

        choiceModification.Effect = $"Increased {choiceModification.ValueChange.Amount} {choiceModification.ValueChange.ValueTransformation.TargetValue} due to {choiceModification.ValueChange.Amount} {choiceModification.ValueChange.ValueTransformation.SourceValue}";
    }

    private void SetValueChange(EncounterChoice choice, EncounterContext context, List<ValueChange> modifiedValueChanges, ChoiceModification choiceModification, ValueChange valueChangeToModify)
    {
        // Remove the existing change and add a new one with the set value
        modifiedValueChanges.Remove(valueChangeToModify);
        modifiedValueChanges.Add(new ValueChange(choiceModification.ValueChange.ValueTransformation.TargetValue, choiceModification.ValueChange.Amount));

        choiceModification.Effect = $"Set {valueChangeToModify.Change} {choiceModification.ValueChange.ValueTransformation.SourceValue} to {choiceModification.ValueChange.Amount} {choiceModification.ValueChange.ValueTransformation.TargetValue}";
    }

    private void ApplyEnergyCostReductions(EncounterChoice choice, EncounterContext context)
    {
        var archetypeEffect = locationArchetypeEffects[context.LocationArchetype];
        // Iterate through each energy cost requirement in the choice
        foreach (var requirement in choice.ModifiedRequirements.OfType<EnergyRequirement>())
        {
            // Check if there's a reduction for this energy type in the archetype's effects
            if (archetypeEffect.EnergyCostReductions.TryGetValue(requirement.EnergyType, out int reductionAmount))
            {
                // Reduce the energy cost requirement, ensuring it doesn't go below 0
                requirement.Amount = Math.Max(0, requirement.Amount - reductionAmount);
            }
        }
    }
}
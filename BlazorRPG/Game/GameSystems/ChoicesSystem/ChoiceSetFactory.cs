public class ChoiceSetFactory
{
    public ChoiceSet CreateFromTemplate(
        ChoiceSetTemplate template,
        EncounterActionContext context)
    {
        // Check if template is valid for this context
        if (!IsTemplateValid(template, context))
            return null;

        // Create base choices from patterns
        List<EncounterChoice> choices = new();
        foreach (ChoicePattern pattern in template.ChoicePatterns)
        {
            EncounterChoice choice = CreateChoiceFromPattern(
                pattern,
                context,
                CalculateModifiers(pattern, context));
            choices.Add(choice);
        }

        return new ChoiceSet(choices);
    }

    private EncounterChoice CreateChoiceFromPattern(
        ChoicePattern pattern,
        EncounterActionContext context,
        ChoiceValueModifiers modifiers)
    {
        // Start with base value changes
        List<ValueChange> baseValueChanges = new(pattern.BaseValueChanges);
        Dictionary<ValueTypes, List<(string Source, int Amount)>> valueChangeDetails = new();

        // Initialize the dictionary with base values
        foreach (ValueChange baseChange in baseValueChanges)
        {
            valueChangeDetails[baseChange.ValueType] = new List<(string Source, int Amount)>
            {
                ("Base", baseChange.Change)
            };
        }

        // Calculate the modifiers based on the pattern and context
        modifiers = CalculateModifiers(pattern, context);

        // Create new ValueChange instances for each modified value
        List<ValueChange> modifiedValueChanges = new List<ValueChange>();

        foreach (ValueChange baseChange in baseValueChanges)
        {
            int modifiedValue = baseChange.Change;
            string modifierDetailKey = baseChange.ValueType.ToString();

            switch (baseChange.ValueType)
            {
                case ValueTypes.Outcome:
                    modifiedValue += modifiers.OutcomeModifier;
                    break;
                case ValueTypes.Pressure:
                    modifiedValue += modifiers.PressureGainModifier;
                    break;
                case ValueTypes.Insight:
                    modifiedValue += modifiers.InsightGainModifier;
                    break;
                case ValueTypes.Resonance:
                    modifiedValue += modifiers.ResonanceGainModifier;
                    break;
            }

            // Add the modified value change to the list
            modifiedValueChanges.Add(new ValueChange(baseChange.ValueType, modifiedValue));

            // Update the modifier details for the UI preview
            if (!string.IsNullOrEmpty(modifierDetailKey))
            {
                foreach (KeyValuePair<string, int> modDetail in modifiers.ModifierDetails)
                {
                    valueChangeDetails[baseChange.ValueType].Add((modDetail.Key, modDetail.Value));
                }
            }
        }

        // Calculate the final energy cost with modifiers and strain
        int strainModifier = context.CurrentValues.Pressure / 3;
        int energyCost = pattern.BaseCost + modifiers.EnergyCostModifier + strainModifier;

        // Add energy cost details to valueChangeDetails
        if (!valueChangeDetails.ContainsKey(ValueTypes.Energy))
        {
            valueChangeDetails[ValueTypes.Energy] = new List<(string Source, int Amount)>();
        }
        valueChangeDetails[ValueTypes.Energy].Add(("Base", pattern.BaseCost));
        if (modifiers.EnergyCostModifier != 0)
        {
            valueChangeDetails[ValueTypes.Energy].Add(("Modifier", modifiers.EnergyCostModifier));
        }

        string description = GenerateDescription(pattern, context);

        // Create the choice using the builder
        ChoiceBuilder choiceBuilder = new ChoiceBuilder()
            .WithName(description)
            .RequiresEnergy(pattern.EnergyType, energyCost)
            .WithValueChanges(modifiedValueChanges)
            .WithRequirements(pattern.Requirements)
            .WithCosts(pattern.Costs)
            .WithRewards(pattern.Rewards);

        // Add the modifier details to the choice
        foreach (KeyValuePair<string, int> modifierDetail in modifiers.ModifierDetails)
        {
            choiceBuilder.WithValueModifier(modifierDetail.Key, modifierDetail.Value);
        }

        List<ValueChangeDetail> valueChangeDetailList = GetValueChangeDetails(modifiers, valueChangeDetails);
        
        EncounterChoice choice = choiceBuilder.Build();
        choice.ValueChangeDetails = valueChangeDetailList;

        return choice;
    }

    private static List<ValueChangeDetail> GetValueChangeDetails(ChoiceValueModifiers modifiers, Dictionary<ValueTypes, List<(string Source, int Amount)>> valueChangeDetails)
    {
        List<ValueChangeDetail> valueChangeDetailList = new List<ValueChangeDetail>();
        foreach (KeyValuePair<ValueTypes, List<(string Source, int Amount)>> detail in valueChangeDetails)
        {
            ValueTypes valueType = detail.Key;
            List<(string Source, int Amount)> sourceAmountList = detail.Value;

            var listValueChangeSource = new List<ValueChangeSource>();
            foreach (var sourceAmount in sourceAmountList)
            {
                ValueChangeSource valueChangeSource = new ValueChangeSource(sourceAmount.Source, sourceAmount.Amount);
                listValueChangeSource.Add(valueChangeSource);
            }
            ValueChangeDetail valueChangeDetail = new ValueChangeDetail(valueType, listValueChangeSource);
            valueChangeDetailList.Add(valueChangeDetail);
        }

        // Build and return the choice
        return valueChangeDetailList;
    }

    private ChoiceValueModifiers CalculateModifiers(
        ChoicePattern pattern,
        EncounterActionContext context)
    {
        ChoiceValueModifiers mods = new ChoiceValueModifiers();

        // **Skill vs. Difficulty**
        int skillVsDifficulty = context.PlayerState.GetSkillLevel(GetRelevantSkill(context)) - context.LocationDifficulty;
        mods.OutcomeModifier += skillVsDifficulty;
        mods.AddModifierDetail("Skill vs. Difficulty", skillVsDifficulty);

        // **Insight reduces Pressure gain**
        mods.PressureGainModifier -= context.CurrentValues.Insight;
        mods.AddModifierDetail("Insight Modifier", -context.CurrentValues.Insight);

        return mods;
    }

    private SkillTypes GetRelevantSkill(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => SkillTypes.Strength,
            BasicActionTypes.Investigate => SkillTypes.Perception,
            BasicActionTypes.Mingle => SkillTypes.Charisma,
            _ => SkillTypes.None
        };
    }

    private string GenerateDescription(ChoicePattern pattern, EncounterActionContext context)
    {
        string description = "";

        // Action type
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                description += "Work";
                break;
            case BasicActionTypes.Investigate:
                description += "Investigate";
                break;
            case BasicActionTypes.Mingle:
                description += "Mingle";
                break;
            default:
                description += context.ActionType.ToString(); // Use the action type as a fallback
                break;
        }

        // Location
        description += $" at the {context.LocationArchetype}";

        // Dynamically determine choice type based on value changes
        string choiceType = GetChoiceType(pattern.BaseValueChanges);
        if (!string.IsNullOrEmpty(choiceType))
        {
            description += $" ({choiceType})";
        }

        // Requirements
        if (pattern.Requirements.Any())
        {
            description += " (Requires: ";
            description += string.Join(", ", pattern.Requirements.Select(r => r.GetDescription()));
            description += ")";
        }

        return description;
    }

    // Helper method to determine choice type based on value changes
    private string GetChoiceType(List<ValueChange> valueChanges)
    {
        // If a choice increases Outcome significantly and decreases Pressure or increases Insight, it could be considered "Careful"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 2) &&
            (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0) ||
             valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change > 0)))
        {
            return "Carefully";
        }

        // If a choice significantly increases Pressure, it could be considered "Aggressive"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change >= 3))
        {
            return "Aggressively";
        }

        // If a choice increases both Outcome and Insight, it could be considered "Tactical"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 1) &&
            valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change >= 2))
        {
            return "Tactically";
        }

        // If a choice restores values like Health, Energy, or reduces Pressure, it could be considered "Recovery"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0))
        {
            return "Carefully";
        }

        // Default: no specific choice type
        return "";
    }

    private bool IsTemplateValid(ChoiceSetTemplate template, EncounterActionContext context)
    {
        // A choice set is invalid if any of its availability conditions are not met
        bool hasLocationConditions = template.AvailabilityConditions.Any();
        bool locationConditionsMet = hasLocationConditions && template.AvailabilityConditions.All(cond => cond.IsMet(context.LocationProperties));
        if (hasLocationConditions && !locationConditionsMet)
        {
            return false;
        }

        // A choice set is invalid if any of its state conditions are not met
        bool hasStateConditions = template.StateConditions.Any();
        bool stateConditionsMet = hasStateConditions && template.StateConditions.All(cond => cond.IsMet(context.CurrentValues));
        if (hasStateConditions && !stateConditionsMet)
        {
            return false;
        }

        return true;
    }
}